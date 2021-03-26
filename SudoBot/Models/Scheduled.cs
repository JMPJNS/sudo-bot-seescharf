using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using SudoBot.Database;
using SudoBot.Specifics;

namespace SudoBot.Models
{
    public class Scheduled
    {
        [BsonId]
        public ObjectId _id { get; private set; }
        private ulong _guildId;
        public ulong GuildId
        {
            get => _guildId;
            set { _guildId = value; SaveScheduled().GetAwaiter().GetResult();}
        }

        private ulong _channelId;
        public ulong ChannelId
        {
            get => _channelId;
            set { _channelId = value; SaveScheduled().GetAwaiter().GetResult();}
        }
        
        private ulong _userId;
        public ulong UserId
        {
            get => _userId;
            set { _userId = value; SaveScheduled().GetAwaiter().GetResult();}
        }
        
        private ulong _messageId;
        public ulong MessageId
        {
            get => _messageId;
            set { _messageId = value; SaveScheduled().GetAwaiter().GetResult();}
        }
        
        private List<ScheduledType> _type;
        public List<ScheduledType> Type
        {
            get => _type;
            set { _type = value; SaveScheduled().GetAwaiter().GetResult();}
        }

        private DateTime _insertDate;
        public DateTime InsertDate
        {
            get => _insertDate;
            set { _insertDate = value; SaveScheduled().GetAwaiter().GetResult();}
        }
        
        private DateTime _scheduledOn;
        public DateTime ScheduledOn
        {
            get => _scheduledOn;
            set { _scheduledOn = value; SaveScheduled().GetAwaiter().GetResult();}
        }
        
        private string _message;
        public string Message
        {
            get => _message;
            set { _message = value; SaveScheduled().GetAwaiter().GetResult();}
        }

        private bool _active;
        public bool Active
        {
            get => _active;
            set { _active = value; SaveScheduled().GetAwaiter().GetResult();}
        }
        
        public Scheduled(List<ScheduledType> type, string message, DateTime scheduledOn, ulong guildId = 0, ulong channelId = 0, ulong userId = 0, ulong messageId = 0)
        {
            _scheduledOn = scheduledOn;
            _message = message;
            _type = type;
            _guildId = guildId;
            _channelId = channelId;
            _userId = userId;
            _messageId = messageId;
                       
            _insertDate = DateTime.Now;

            _active = true;
            
            InsertScheduled().GetAwaiter().GetResult();
        }
        
        private async Task InsertScheduled()
        {
            await Mongo.Instance.InsertScheduled(this);
        }
        private async Task SaveScheduled()
        {
            await Mongo.Instance.UpdateScheduled(this);
        }

        public static async Task RunSchedule(ScheduledType t)
        {
            var scheduled = await Mongo.Instance.GetDueScheduled();
            foreach (var stuff in scheduled)
            {
                if (stuff.Type.Contains(ScheduledType.Minute))
                {
                    if (t == ScheduledType.Minute)
                        await MinuteScheduler(stuff);
                } else if (stuff.Type.Contains(ScheduledType.Day))
                {
                    if (t == ScheduledType.Day)
                        await DayScheduler(stuff);
                } else if (stuff.Type.Contains(ScheduledType.Hour))
                {
                    if (t == ScheduledType.Hour)
                        await HourScheduler(stuff);
                } else if (stuff.Type.Contains(ScheduledType.SixHour))
                {
                    if (t == ScheduledType.SixHour)
                        await SixHourScheduler(stuff);
                }
            }
        }

        private static async Task SixHourScheduler(Scheduled stuff)
        {
            throw new NotImplementedException();
        }

        private static async Task HourScheduler(Scheduled stuff)
        {
            throw new NotImplementedException();
        }

        private static async Task DayScheduler(Scheduled stuff)
        {
            throw new NotImplementedException();
        }

        private static async Task MinuteScheduler(Scheduled stuff)
        {
            try
            {
                // Reminders
                if (stuff.Type.Contains(ScheduledType.Reminder))
                {
                    var guild = await Globals.Client.GetGuildAsync(stuff.GuildId);
                    var channel = guild.GetChannel(stuff.ChannelId);
                    var member = await guild.GetMemberAsync(stuff.UserId);
                        
                    var tz = Environment.OSVersion.Platform == PlatformID.Win32NT ? TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time") : TimeZoneInfo.FindSystemTimeZoneById("Europe/Vienna");
                    var time = TimeZoneInfo.ConvertTimeFromUtc(stuff.InsertDate, tz);

                    // Prevent Escape to ping people
                    if (stuff.Message == null)
                        stuff.Message = "";
                    var message = stuff.Message.Replace("```", "'''");
                    var minutes = time.Minute < 10 ? $"0{time.Minute}" : time.Minute.ToString();
                    var sendMessage = message != "" ? $" ```{message}```" : "";

                    var messageLink = $"https://discord.com/channels/{stuff.GuildId}/{stuff.ChannelId}/{stuff.MessageId}";
                        
                    await channel.SendMessageAsync(
                        $"{member.Mention} am `{time.Day}.{time.Month}.{time.Year} {time.Hour}:{minutes}` {messageLink}" +sendMessage);
                    stuff.Active = false;
                }
                
                // HungerGames
                if (stuff.Type.Contains(ScheduledType.HungerGames))
                {
                    var guild = await Globals.Client.GetGuildAsync(stuff.GuildId);
                    var channel = guild.GetChannel(stuff.ChannelId);
                    var message = await channel.GetMessageAsync(stuff.MessageId);

                    var emoji = DiscordEmoji.FromUnicode("🏟");
                    var joinedUsers = await message.GetReactionsAsync(emoji, 9999);

                    int maxPlayers = 512;
                    Boolean useBots = false;
                    
                    try
                    {
                        maxPlayers = Int32.Parse(stuff.Message.Split(":")[0]);

                        var b = stuff.Message.Split(":")[1];
                        if (b == "True") useBots = true;

                    }catch (Exception _)
                    {
                        // ignored
                    }


                    var names = new List<HungerGamesPlayer>();
                    foreach (var user in joinedUsers)
                    {
                        if (names.Count >= maxPlayers) break;
                        
                        var member = await guild.GetMemberAsync(user.Id);
                        if (names.Any(x => x.Name == member.DisplayName))
                        {
                            var p = new HungerGamesPlayer();
                            p.Name = member.DisplayName + Globals.RandomString(2);
                            p.Id = member.Id;
                            
                            names.Add(p);
                        }
                        else
                        {
                            var p = new HungerGamesPlayer();
                            p.Name = member.DisplayName;
                            p.Id = member.Id;
                            
                            names.Add(p);
                        }
                    }

                    if (useBots)
                    {
                        var c = maxPlayers - names.Count;
                        for (int i = 1; i <= maxPlayers; i++)
                        {
                            var p = new HungerGamesPlayer();
                            p.Name = $"Bot{i}";
                            p.Id = 0;
                            
                            names.Add(p);
                        }
                    }
                    
                    var hg = new HungerGames(names);
                    try
                    {
                        while (!await hg.RunCycle(channel))
                        {
                            // Wait for 30 seconds between cycles
                            await Task.Delay(30 * 1000);
                        }

                        if (hg.Winner != null)
                        {
                            // Give Winner XP
                        }
                    }
                    catch (Exception e)
                    {
                        await channel.SendMessageAsync($"Error: {e.Message}");
                    }
                    finally
                    {
                        stuff.Active = false;   
                    }
                }
            }
            catch (Exception e)
            {
                Globals.Client.Logger.Log(LogLevel.Error,  $"Error in Scheduler: {e.Message}", DateTime.Now);
            }
        }
    }
}