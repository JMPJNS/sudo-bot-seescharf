using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DateTime = System.DateTime;

namespace SudoBot.DataInterfaces
{
    public class User
    {
        public int Id { get; set; }
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

            int days = (int)(DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc) - JoinDate).TotalDays;

            return messages + points + days*50;
            
        }
        
        public bool AddCountedMessages(DiscordMessage message)
        {
            if (Blocked) return false;
            
            TimeSpan minDelay = TimeSpan.FromMinutes(0.1);

            if (DateTime.Now - minDelay <= LastUpdated)
            {
                return false;
            }

            int countedMessageLength = 100;
            int count = (message.Content.Length + countedMessageLength)/countedMessageLength;

            LastUpdated = DateTime.Now;
            CountedMessages += count;

            SaveUser();
            
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
            // User user = await Firebase.Instance.GetUser(member.Id, member.Guild.Id);
            // return user ?? await GetFreshUser(member);
            return await GetFreshUser(member);
        }
        
        private static async Task<User> GetFreshUser(DiscordMember member)
        {
            User u =  new User(
                member.Id,
                member.Guild.Id,
                member.JoinedAt,
                0,
                0,
                false
            );

            // return await Firebase.Instance.CreateUser(u);
            return u;
        }

        private void SaveUser()
        {
            // Firebase.Instance.SaveUser(this).GetAwaiter().GetResult();
        }
        
        private User(ulong userId, ulong guildId, DateTimeOffset joinDate, int countedMessages, int specialPoints, bool blocked)
        {
            UserId = userId;
            GuildId = guildId;
            JoinDate = joinDate.DateTime;

            Blocked = blocked;

            CountedMessages = countedMessages;
            SpecialPoints = specialPoints;

            LastUpdated = DateTime.Now;
        }
        
        public User() {}
    }
}