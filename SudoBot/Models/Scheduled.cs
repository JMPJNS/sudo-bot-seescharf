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
using SudoBot.Schedulers;
using SudoBot.Specifics;

namespace SudoBot.Models
{
    public class Scheduled
    {
        [BsonId]
        public ObjectId Id { get; private set; }

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

        private Dictionary<string, string> _arguments;
        public Dictionary<string, string> Arguments
        {
            get => _arguments;
            set { _arguments = value; SaveScheduled().GetAwaiter().GetResult();}
        }

        private bool _active;
        public bool Active
        {
            get => _active;
            set { _active = value; SaveScheduled().GetAwaiter().GetResult();}
        }
        
        public Scheduled(List<ScheduledType> type, DateTime scheduledOn, Dictionary<string, string> arguments = null)
        {
            _scheduledOn = scheduledOn;
            _type = type;

            _arguments = arguments;

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

        public static async Task RunSchedule()
        {
            var scheduled = await Mongo.Instance.GetDueScheduled();
            foreach (var stuff in scheduled)
            {
                try
                {
                    await Scheduler.Execute(stuff);
                    stuff.Active = false;
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Error running Schedule: {e.Message}");
                }
            }
        }
    }
}