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
using SudoBot.DataInterfaces;
using SudoBot.Handlers;

namespace SudoBot
{
    public class Bot
    {
        public DiscordClient Client { get; private set; }
        public CommandsNextExtension Commands { get; private set; }
        public InteractivityExtension Interactivity { get; private set; }
        
        private MessageHandler _messageHandler = new MessageHandler();
        
        private Firebase _fb = Firebase.Instance;

        public async Task RunAsync()
        {
            BotConfig bc = await _fb.GetBotConfig();

            // Bot
            var config = new DiscordConfiguration
            {
                Token = bc.Token,
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
                StringPrefixes = bc.Prefixes,
                EnableMentionPrefix = true,
                EnableDms = false
            };

            Commands = Client.UseCommandsNext(commandsConfig);
            
            //Interactivity
            var interactivityConfig = new InteractivityConfiguration
            {
                Timeout = TimeSpan.FromMinutes(5)
            };

            Interactivity = Client.UseInteractivity(interactivityConfig);
            
            Commands.RegisterCommands<FunCommands>();
            Commands.RegisterCommands<UtilityCommands>();
            Commands.RegisterCommands<RankCommands>();
            Commands.RegisterCommands<CustomGamesCommands>();
                        
            // Start Bot
            await Client.ConnectAsync();
            await Task.Delay(-1);
        }

        private Task OnClientReady(ReadyEventArgs e)
        {
            e.Client.DebugLogger.LogMessage(LogLevel.Info, "SudoBot", $"Bot Started", DateTime.Now);

            return Task.CompletedTask;
        }

        private Task OnGuildAvailable(GuildCreateEventArgs e)
        {
            e.Client.DebugLogger.LogMessage(LogLevel.Info, "SudoBot", $"Bot Logged in on: {e.Guild.Name}", DateTime.Now);

            return Task.CompletedTask;
        }
        
        private Task OnClientError(ClientErrorEventArgs e)
        {
            e.Client.DebugLogger.LogMessage(LogLevel.Error, "SudoBot", $"Exception occured: {e.Exception.GetType()}: {e.Exception.Message}", DateTime.Now);
            
            return Task.CompletedTask;
        }
        
        private Task MessageCreated(MessageCreateEventArgs e)
        {
            e.Client.DebugLogger.LogMessage(LogLevel.Debug, "SudoBot", $"Message sent: ${e.Message}", DateTime.Now);

            _messageHandler.HandleMessage(e.Message, e.Author, e.Guild).GetAwaiter().GetResult();
            return Task.CompletedTask;
        }
    }
}