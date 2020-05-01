using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using Google.Cloud.Firestore;
using SudoBot.Database;
using DateTime = System.DateTime;

namespace SudoBot.DataInterfaces
{
    [FirestoreData]
    public class User
    {
        [FirestoreProperty]
        public ulong UserId { set; get; }
        
        [FirestoreProperty]
        public ulong GuildId { set; get; }

        [FirestoreProperty]
        public bool Blocked { set; get; }
        
        [FirestoreProperty]
        public DateTime LastUpdated { set; get; }
        
        [FirestoreProperty]
        public DateTimeOffset JoinDate { set; get; }

        [FirestoreProperty]
        public int SpecialPoints { set; get; }
        
        [FirestoreProperty]
        public int CountedMessages { set; get; }
        
        // Logic Starts Here
        
        private Firebase _fb = Firebase.Instance;
        
        public int CalculatePoints()
        {
            int messages = this.CountedMessages;
            int points = this.SpecialPoints;

            int days = (int)(DateTime.Now - this.JoinDate).TotalDays;

            return messages + points + days*50;
            
        }
        
        public bool AddCountedMessages(DiscordMessage message)
        {
            if (this.Blocked) return false;
            
            TimeSpan minDelay = TimeSpan.FromMinutes(0.1);

            if (DateTimeOffset.Now - minDelay <= this.LastUpdated)
            {
                return false;
            }

            int countedMessageLength = 100;
            int count = (message.Content.Length + countedMessageLength)/countedMessageLength;

            this.LastUpdated = DateTime.Now;
            this.CountedMessages += count;
            
            return true;
        }

        public static async Task<User> GetOrCreateUser(DiscordMember member)
        {
            User user = await Firebase.Instance.GetUser(member.Id, member.Guild.Id);

            // if (user is null)
            // {
            //     User fresh = GetFreshUser(member);
            //     return fresh;
            // }
            
            return user ?? GetFreshUser(member);
        }
        
        public static User GetFreshUser(DiscordMember member)
        {
            return new User(
                member.Id,
                member.Guild.Id,
                member.JoinedAt,
                0,
                0,
                false
            );
        }
        
        private User(ulong userId, ulong guildId, DateTimeOffset joinDate, int countedMessages, int specialPoints, bool blocked)
        {
            this.UserId = userId;
            this.GuildId = guildId;
            this.JoinDate = joinDate;

            this.Blocked = blocked;

            this.CountedMessages = countedMessages;
            this.SpecialPoints = specialPoints;

            this.LastUpdated = DateTime.Now;
        }
    }
}