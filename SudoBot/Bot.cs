using System;
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
                StringPrefixes = new []{"$"},
                EnableMentionPrefix = true,
                EnableDms = false
            };

            Commands = Client.UseCommandsNext(commandsConfig);
            
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