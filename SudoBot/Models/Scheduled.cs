using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using SudoBot.Database;

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
            }
            catch (Exception e)
            {
                Globals.Logger.Log(LogLevel.Error,  $"Error in Scheduler: {e.Message}", DateTime.Now);
            }
        }
    }
}