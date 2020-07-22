using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using Newtonsoft.Json;
using SudoBot.Attributes;
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

        public static Timer MinuteScheduler;
        public static Timer HourScheduler;
        public static Timer SixHourScheduler;
        public static Timer DayScheduler;

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
            Commands.RegisterCommands<TagCommands>();
            Commands.RegisterCommands<ParserCommands>();
            Commands.RegisterCommands<SearchCommands>();
                        
            MinuteScheduler = new Timer(60*1000);
            MinuteScheduler.Elapsed += OnMinuteEvent;
            
            HourScheduler = new Timer(60*60*1000);
            HourScheduler.Elapsed += OnHourEvent;
            
            SixHourScheduler = new Timer(6*60*60*1000);
            SixHourScheduler.Elapsed += OnSixHourEvent;
            
            DayScheduler = new Timer(24*60*60*1000);
            DayScheduler.Elapsed += OnDayEvent;
            
            // Start Bot
            await Client.ConnectAsync();
            await Task.Delay(-1);
        }

        private static void OnMinuteEvent(object source, ElapsedEventArgs e)
        {
            Scheduled.RunSchedule(ScheduledType.Minute).GetAwaiter().GetResult();
        }
        
        private static void OnHourEvent(object source, ElapsedEventArgs e)
        {
            Scheduled.RunSchedule(ScheduledType.Hour).GetAwaiter().GetResult();
        }
        
        private static void OnSixHourEvent(object source, ElapsedEventArgs e)
        {
            Scheduled.RunSchedule(ScheduledType.SixHour).GetAwaiter().GetResult();
        }
        
        private static void OnDayEvent(object source, ElapsedEventArgs e)
        {
            Scheduled.RunSchedule(ScheduledType.Day).GetAwaiter().GetResult();
        }

        private Task OnClientReady(ReadyEventArgs e)
        {
            Globals.Logger = e.Client.DebugLogger;
            Globals.Client = e.Client;
            Globals.Logger.LogMessage(LogLevel.Info, "SudoBot", $"Bot Started", DateTime.Now);

            Task.Run(() =>
                {
                    Task.Delay(10000).GetAwaiter().GetResult();
                    MinuteScheduler.Start();
                    
                    e.Client.UpdateStatusAsync(new DiscordActivity("$guild", ActivityType.ListeningTo));
                    Globals.LogChannel.SendMessageAsync("Bot Started");
                });

            return Task.CompletedTask;
        }

        private Task OnCommandErrored(CommandErrorEventArgs e)
        {
            if (e.Exception.Message == "Specified command was not found.")
            {
                var sentMessage = e.Context.Channel.SendMessageAsync($"Command nicht Gefunden").GetAwaiter().GetResult();
                Task.Delay(2000).GetAwaiter().GetResult();
                sentMessage.DeleteAsync();

            }
            
            else if (e.Command.Name == "list")
            {
                if (e.Exception.Message == "Value cannot be null. (Parameter 'source')")
                {
                    e.Context.Channel.SendMessageAsync("Keine Rollen im Ranking, füge eine mit `$rank set-role {@rolle} {punkte}` hinzu");
                } 
            }


            else if (e.Exception.Message == "NO LOG CHANNEL")
            {
                e.Context.Channel.SendMessageAsync("Es ist ein fehler aufgetreten, allerdings konnte dieser nicht gemeldet werden da kein Error Log channel festgelegt wurde, bitte lege einen mit `$admin set-log-channel #channel` fest");
            }
            
            else if (e.Exception.Message == "No matching subcommands were found, and this group is not executable.")
            {
                e.Context.Channel.SendMessageAsync("Dies ist eine Command Gruppe, bitte einen Subcommand Spezifizieren").GetAwaiter().GetResult();
                var commandName = e.Command.Name;
                var help = e.Context.CommandsNext.FindCommand("help", out commandName);

                var helpContext = e.Context.CommandsNext.CreateFakeContext(e.Context.User, e.Context.Channel,
                    e.Context.Message.Content, e.Context.Prefix, help, e.Command.Name);
                
                help.ExecuteAsync(helpContext);
            }
            
            else if (e.Exception is ChecksFailedException)
            {
                var exception = (ChecksFailedException) e.Exception;
                var failed = exception.FailedChecks;
                foreach (var f in failed)
                {
                    if (f is CheckForPermissionsAttribute)
                    {
                        e.Context.RespondAsync("Keine Berechtigung diesen command zu verwenden!");
                    } else if (f is DSharpPlus.CommandsNext.Attributes.RequireOwnerAttribute)
                    {
                        e.Context.RespondAsync("Nix Da!");
                    } else if (f is DSharpPlus.CommandsNext.Attributes.CooldownAttribute)
                    {
                        var attr = (DSharpPlus.CommandsNext.Attributes.CooldownAttribute) f;
                        var reset = attr.GetRemainingCooldown(e.Context);
                        string remaining = "";
                        if (reset.Days > 0)
                        {
                            remaining += $"{reset.Days} Tage ";
                        }
                        if (reset.Hours > 0)
                        {
                            remaining += $"{reset.Hours} Stunden ";
                        }
                        if (reset.Minutes > 0)
                        {
                            remaining += $"{reset.Minutes} Minuten ";
                        }
                        if (reset.Seconds > 0)
                        {
                            remaining += $"{reset.Seconds} Sekunden ";
                        }

                        var sent = e.Context.RespondAsync($"Zurzeit im Cooldown, bitte {remaining}warten").GetAwaiter().GetResult();
                        Task.Delay(2000).GetAwaiter().GetResult();
                        e.Context.Message.DeleteAsync();
                        Task.Delay(1000).GetAwaiter().GetResult();
                        sent.DeleteAsync();
                    }
                    else
                    {
                        e.Context.RespondAsync($"Exception: ```{e.Exception.Message}``` Check: ```{f.TypeId}```Wenn dies ein unbekannter Fehler ist bitte auf den `$guild` Discord kommen und JMP#7777 kontaktieren."); 
                    }
                }
            }

            else if (e.Exception.Message == "Could not find a suitable overload for the command.")
            {
                e.Context.Channel.SendMessageAsync("Invalide Argumente").GetAwaiter().GetResult();
                var commandName = e.Command.Name;
                var help = e.Context.CommandsNext.FindCommand("help", out commandName);

                var helpContext = e.Context.CommandsNext.CreateFakeContext(e.Context.User, e.Context.Channel,
                    e.Context.Message.Content, e.Context.Prefix, help, $"{e.Command.QualifiedName}");
                
                help.ExecuteAsync(helpContext);
            }

            else {
                e.Context.Channel.SendMessageAsync($"```{e.Exception.Message}```Wenn dies ein unbekannter Fehler ist bitte auf den `$guild` Discord kommen und JMP#7777 kontaktieren.").GetAwaiter().GetResult();
            }

            Task.Delay(2000).GetAwaiter().GetResult();
            e.Context.Message.DeleteAsync();

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
            var foundGuild = Guild.GetGuild(e.Guild.Id).GetAwaiter().GetResult();
            if (foundGuild == null)
            {
                Mongo.Instance.InsertGuild(g).GetAwaiter().GetResult();
            }
            else
            {
                Mongo.Instance.UpdateGuild(g).GetAwaiter().GetResult();
            }
            
            
            return Task.CompletedTask;
        }
        
        private Task OnGuildAvailable(GuildCreateEventArgs e)
        {
            Globals.Logger.LogMessage(LogLevel.Info, "SudoBot", $"Bot Logged in on: [{e.Guild.Id}] {e.Guild.Name}", DateTime.Now);

            // var embed = new DiscordEmbedBuilder()
            //     .WithColor(DiscordColor.Aquamarine)
            //     .WithThumbnailUrl(e.Guild.IconUrl)
            //     .WithTitle("Bot Logged In")
            //     .WithDescription(e.Guild.Name);
            //
            // Globals.LogChannel.SendMessageAsync(embed: embed.Build());
            
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
            try
            {
                _messageHandler.HandleMessage(e).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                if (ex.Message == "NO LOG CHANNEL")
                {
                }
                else
                {
                    throw ex;
                }
            }
            
            return Task.CompletedTask;
        }
    }
}
