using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using SudoBot.Database;
using DateTime = System.DateTime;

namespace SudoBot.Models
{
    public class User
    {
        [BsonId]
        public ObjectId Id { get; set; }
        public ulong UserId { private set; get; }
        
        public ulong GuildId { private set; get; }

        public bool Blocked { private set; get; }
        
        public DateTime LastUpdated { private set; get; }
        
        public DateTime JoinDate { private set; get; }

        public int SpecialPoints { private set; get; }
        
        public int CountedMessages { private set; get; }
        
        // Logic Starts Here

        
        public int CalculatePoints()
        {
            int messages = CountedMessages;
            int points = SpecialPoints;

            int days = (int)(DateTime.UtcNow - JoinDate).TotalDays;

            return messages + points + days*24;
            
        }
        
        public async Task<bool> AddCountedMessages(DiscordMessage message)
        {
            if (Blocked) return false;
            
            TimeSpan minDelay = TimeSpan.FromMinutes(0.1);

            if (DateTime.UtcNow - minDelay <= LastUpdated)
            {
                return false;
            }

            int countedMessageLength = 100;
            int count = (message.Content.Length + countedMessageLength)/countedMessageLength;

            LastUpdated = DateTime.UtcNow;
            CountedMessages += count;

            await SaveUser();
            
            return true;
        }

        public void AddSpecialPoints(int points)
        {
            SpecialPoints += points;
            SaveUser();
        }
        
        // Database Management stuff here

        public static async Task<User> GetOrCreateUser(DiscordMember member)
        {
            User user = await MongoCrud.Instance.GetUser(member.Id, member.Guild.Id);
            if (user != null) return user;

            user = GetFreshUser(member);
            MongoCrud.Instance.InsertUser(user);
            return user;
        }
        
        private static User GetFreshUser(DiscordMember member)
        {
            User u =  new User(
                member.Id,
                member.Guild.Id,
                member.JoinedAt,
                0,
                0,
                false
            );
            return u;
        }

        private async Task SaveUser()
        {
            await MongoCrud.Instance.UpdateUser(this);
        }
        
        private User(ulong userId, ulong guildId, DateTimeOffset joinDate, int countedMessages, int specialPoints, bool blocked)
        {
            UserId = userId;
            GuildId = guildId;
            JoinDate = joinDate.DateTime;

            Blocked = blocked;

            CountedMessages = countedMessages;
            SpecialPoints = specialPoints;

            LastUpdated = DateTime.UtcNow;
        }
        
        public User() {}
    }
}