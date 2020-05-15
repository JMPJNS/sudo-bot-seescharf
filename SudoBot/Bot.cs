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
                        
            // Start Bot
            await Client.ConnectAsync();
            await Task.Delay(-1);
        }

        private Task OnClientReady(ReadyEventArgs e)
        {
            Globals.Logger = e.Client.DebugLogger;
            Globals.Logger.LogMessage(LogLevel.Info, "SudoBot", $"Bot Started", DateTime.Now);

            return Task.CompletedTask;
        }

        private Task OnGuildAvailable(GuildCreateEventArgs e)
        {
            Globals.Logger.LogMessage(LogLevel.Info, "SudoBot", $"Bot Logged in on: {e.Guild.Name}", DateTime.Now);
            
            var guild = Mongo.Instance.GetGuild(e.Guild.Id).GetAwaiter().GetResult();
            if (guild == null)
            {
                Mongo.Instance.InsertGuild(new Guild(e.Guild.Id)).GetAwaiter().GetResult();
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
            Globals.Logger.LogMessage(LogLevel.Info, "SudoBot", $"Message Created: [{e.Guild}] ({e.Author}): ${e.Message}", DateTime.Now);
            _messageHandler.HandleMessage(e).GetAwaiter().GetResult();
            return Task.CompletedTask;
        }
    }
}