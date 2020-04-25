using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Newtonsoft.Json;
using OllekBot.Commands;
using OllekBot.Handlers;

namespace OllekBot
{
    public class Bot
    {
        public DiscordClient Client { get; private set; }
        public CommandsNextExtension Commands { get; private set; }
        
        private MessageHandler _messageHandler = new MessageHandler();
        
        public async Task RunAsync()
        {

            // Bot
            var config = new DiscordConfiguration
            {
                Token = BotConfig.Token,
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
            
            //Commands
            var commandsConfig = new CommandsNextConfiguration
            {
                StringPrefixes = new string[] {BotConfig.Prefix},
                EnableMentionPrefix = true,
                EnableDms = false
            };

            Commands = Client.UseCommandsNext(commandsConfig);
            Commands.RegisterCommands<FunCommands>();
            Commands.RegisterCommands<UtilityCommands>();
            Commands.RegisterCommands<RankCommands>();
            
            // Start Bot
            await Client.ConnectAsync();
            await Task.Delay(-1);
        }

        private Task OnClientReady(ReadyEventArgs e)
        {
            e.Client.DebugLogger.LogMessage(LogLevel.Info, "OllekBot", $"Bot Started", DateTime.Now);

            return Task.CompletedTask;
        }

        private Task OnGuildAvailable(GuildCreateEventArgs e)
        {
            e.Client.DebugLogger.LogMessage(LogLevel.Info, "OllekBot", $"Bot Logged in on: {e.Guild.Name}", DateTime.Now);

            return Task.CompletedTask;
        }
        
        private Task OnClientError(ClientErrorEventArgs e)
        {
            e.Client.DebugLogger.LogMessage(LogLevel.Error, "OllekBot", $"Exception occured: {e.Exception.GetType()}: {e.Exception.Message}", DateTime.Now);
            
            return Task.CompletedTask;
        }
        
        private Task MessageCreated(MessageCreateEventArgs e)
        {
            e.Client.DebugLogger.LogMessage(LogLevel.Debug, "OllekBot", $"Message sent: ${e.Message}", DateTime.Now);

            _messageHandler.HandleMessage(e.Message, e.Author, e.Guild).GetAwaiter().GetResult();
            return Task.CompletedTask;
        }
    }
}