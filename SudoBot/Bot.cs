﻿using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using Newtonsoft.Json;
using SudoBot.Commands;
using SudoBot.Database;
using SudoBot.Models;
using SudoBot.Handlers;

namespace SudoBot
{
    public class Bot
    {
        public DiscordClient Client { get; private set; }
        public CommandsNextExtension Commands { get; private set; }
        public InteractivityExtension Interactivity { get; private set; }
        
        private MessageHandler _messageHandler = new MessageHandler();
        private MemberUpdateHandler _memberUpdateHandler = new MemberUpdateHandler();

        public async Task RunAsync()
        {

            // Bot
            var config = new DiscordConfiguration
            {
                Token = Environment.GetEnvironmentVariable("BOTTOKEN"),
                TokenType = TokenType.Bot,
                AutoReconnect = true,
                LogLevel = LogLevel.Debug,
                UseInternalLogHandler = true
            };

            Client = new DiscordClient(config);
            Client.Ready += OnClientReady;
            Client.GuildAvailable += OnGuildAvailable;
            Client.GuildCreated += OnGuildCreated;
            
            Client.ClientErrored += OnClientError;
            Client.MessageCreated += MessageCreated;

            Client.GuildMemberUpdated += OnMemberUpdated;

            //Commands
            var commandsConfig = new CommandsNextConfiguration
            {
                StringPrefixes = new []{"$", "sudo "},
                EnableMentionPrefix = true,
                EnableDms = false,
                IgnoreExtraArguments = true
            };

            Commands = Client.UseCommandsNext(commandsConfig);
            Commands.SetHelpFormatter<SudoHelpFormatter>();

            Commands.CommandErrored += OnCommandErrored;
            
            //Interactivity
            var interactivityConfig = new InteractivityConfiguration
            {
                Timeout = TimeSpan.FromMinutes(1)
            };

            Interactivity = Client.UseInteractivity(interactivityConfig);
            
            Commands.RegisterCommands<FunCommands>();
            Commands.RegisterCommands<UtilityCommands>();
            Commands.RegisterCommands<RankCommands>();
            Commands.RegisterCommands<CustomGamesCommands>();
            Commands.RegisterCommands<TestCommands>();
            Commands.RegisterCommands<AdminCommands>();
            Commands.RegisterCommands<ModCommands>();
            Commands.RegisterCommands<FileCommands>();
            Commands.RegisterCommands<GlobalCommands>();
                        
            // Start Bot
            await Client.ConnectAsync();
            await Task.Delay(-1);
        }

        private Task OnClientReady(ReadyEventArgs e)
        {
            Globals.Logger = e.Client.DebugLogger;
            Globals.Client = e.Client;
            Globals.Logger.LogMessage(LogLevel.Info, "SudoBot", $"Bot Started", DateTime.Now);

            return Task.CompletedTask;
        }

        private Task OnCommandErrored(CommandErrorEventArgs e)
        {
            if (e.Exception.Message == "Specified command was not found.")
                e.Context.Channel.SendMessageAsync($"Command nicht Gefunden");

            if (e.Exception.Message == "No matching subcommands were found, and this group is not executable.")
            {
                e.Context.Channel.SendMessageAsync("Dies ist eine Command Gruppe, bitte einen Subcommand Spezifizieren").GetAwaiter().GetResult();
                var commandName = e.Command.Name;
                var help = e.Context.CommandsNext.FindCommand("help", out commandName);

                var helpContext = e.Context.CommandsNext.CreateFakeContext(e.Context.User, e.Context.Channel,
                    e.Context.Message.Content, e.Context.Prefix, help, e.Command.Name);
                
                help.ExecuteAsync(helpContext);
            }

            if (e.Exception.Message == "Could not find a suitable overload for the command.")
            {
                e.Context.Channel.SendMessageAsync("Invalide Argumente").GetAwaiter().GetResult();
                var commandName = e.Command.Name;
                var help = e.Context.CommandsNext.FindCommand("help", out commandName);

                var helpContext = e.Context.CommandsNext.CreateFakeContext(e.Context.User, e.Context.Channel,
                    e.Context.Message.Content, e.Context.Prefix, help, $"{e.Command.QualifiedName}");
                
                help.ExecuteAsync(helpContext);
            }

            return Task.CompletedTask;
            }

        private Task OnGuildCreated(GuildCreateEventArgs e)
        {
            Globals.Logger.LogMessage(LogLevel.Info, "SudoBot", $"Bot Joined: [{e.Guild.Id}] {e.Guild.Name}", DateTime.Now);

            var embed = new DiscordEmbedBuilder()
                .WithColor(DiscordColor.Aquamarine)
                .WithThumbnailUrl(e.Guild.IconUrl)
                .WithTitle("Bot Joined")
                .WithDescription(e.Guild.Name)
                .AddField("ID", e.Guild.Id.ToString())
                .AddField("User Count", e.Guild.MemberCount.ToString());
            
            Globals.LogChannel.SendMessageAsync(embed: embed.Build());
            
            Guild g = new Guild(e.Guild.Id);
            g.Name = e.Guild.Name;
            g.MemberCount = e.Guild.MemberCount;
            Mongo.Instance.InsertGuild(g).GetAwaiter().GetResult();
            
            return Task.CompletedTask;
        }
        
        private Task OnGuildAvailable(GuildCreateEventArgs e)
        {
            Globals.Logger.LogMessage(LogLevel.Info, "SudoBot", $"Bot Logged in on: [{e.Guild.Id}] {e.Guild.Name}", DateTime.Now);

            var embed = new DiscordEmbedBuilder()
                .WithColor(DiscordColor.Aquamarine)
                .WithThumbnailUrl(e.Guild.IconUrl)
                .WithTitle("Bot Logged In")
                .WithDescription(e.Guild.Name);
            
            Globals.LogChannel.SendMessageAsync(embed: embed.Build());
            
            var guild = Guild.GetGuild(e.Guild.Id).GetAwaiter().GetResult();
            if (guild == null)
            {
                Guild g = new Guild(e.Guild.Id);
                g.Name = e.Guild.Name;
                g.MemberCount = e.Guild.MemberCount;
                Mongo.Instance.InsertGuild(g).GetAwaiter().GetResult();
            }
            else
            {
                if (guild.MemberCount == 0 || guild.Name == null)
                {
                    guild.Name = e.Guild.Name;
                    guild.MemberCount = e.Guild.MemberCount;
                    guild.SaveGuild().GetAwaiter().GetResult();
                }

                if (guild.Name != e.Guild.Name || guild.MemberCount != e.Guild.MemberCount)
                {
                    guild.Name = e.Guild.Name;
                    guild.MemberCount = e.Guild.MemberCount;
                    guild.SaveGuild().GetAwaiter().GetResult();
                }
            }

            return Task.CompletedTask;
        }

        private Task OnMemberUpdated(GuildMemberUpdateEventArgs e)
        {
            Globals.Logger.LogMessage(LogLevel.Info, "SudoBot", $"Member Updated: [{e.Guild}] ({e.Member})", DateTime.Now);
            _memberUpdateHandler.HandleRoleChange(e).GetAwaiter().GetResult();
            return Task.CompletedTask;
        }
        
        private Task OnClientError(ClientErrorEventArgs e)
        {
            Globals.Logger.LogMessage(LogLevel.Error, "SudoBot", $"Exception occured: {e.Exception.GetType()}: {e.Exception.Message}", DateTime.Now);
            
            return Task.CompletedTask;
        }
        
        private Task MessageCreated(MessageCreateEventArgs e)
        {
            Globals.Logger.LogMessage(LogLevel.Info, "SudoBot", $"Message Created: [{e.Guild.Id} : {e.Channel.Id}] ({e.Author.Username}): {e.Message.Content}", DateTime.Now);
            _messageHandler.HandleMessage(e).GetAwaiter().GetResult();
            return Task.CompletedTask;
        }
    }
}