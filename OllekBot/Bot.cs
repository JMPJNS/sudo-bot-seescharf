using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.EventArgs;
using Newtonsoft.Json;
using OllekBot.Commands;

namespace OllekBot
{
    public class Bot
    {
        public DiscordClient Client { get; private set; }
        public CommandsNextExtension Commands { get; private set; }
        
        public async Task RunAsync()
        {
            //Config
            var botConfigJson = string.Empty;
            
            using(var fs = File.OpenRead("config.json"))
                using (var sr = new StreamReader(fs, new UTF8Encoding(false)))
                    botConfigJson = await sr.ReadToEndAsync().ConfigureAwait(false);

            var botConfig = JsonConvert.DeserializeObject<BotConfig>(botConfigJson);
            
            // Bot
            var config = new DiscordConfiguration
            {
                Token = botConfig.Token,
                TokenType = TokenType.Bot,
                AutoReconnect = true,
                LogLevel = LogLevel.Debug,
                UseInternalLogHandler = true
            };
            
            Client = new DiscordClient(config);
            Client.Ready += OnClientReady;
            
            //Commands
            var commandsConfig = new CommandsNextConfiguration
            {
                StringPrefixes = new string[] {botConfig.Prefix},
                EnableMentionPrefix = true,
                EnableDms = false
            };

            Commands = Client.UseCommandsNext(commandsConfig);
            Commands.RegisterCommands<FunCommands>();
            
            // Start Bot
            await Client.ConnectAsync();
            await Task.Delay(-1);
        }

        private Task OnClientReady(ReadyEventArgs e)
        {
            Console.WriteLine("Bot Started");
            return Task.CompletedTask;
        }
    }
}