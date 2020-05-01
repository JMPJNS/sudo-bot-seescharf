using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
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
        public ulong UserId { private set; get; }
        
        [FirestoreProperty]
        public ulong GuildId { private set; get; }

        [FirestoreProperty]
        public bool Blocked { private set; get; }
        
        [FirestoreProperty]
        public Timestamp LastUpdated { private set; get; }
        
        [FirestoreProperty]
        public Timestamp JoinDate { private set; get; }

        [FirestoreProperty]
        public int SpecialPoints { private set; get; }
        
        [FirestoreProperty]
        public int CountedMessages { private set; get; }
        
        // Logic Starts Here

        public DocumentReference userReference;
        
        public int CalculatePoints()
        {
            int messages = CountedMessages;
            int points = SpecialPoints;

            int days = (int)(DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc) - JoinDate.ToDateTime()).TotalDays;

            return messages + points + days*50;
            
        }
        
        public bool AddCountedMessages(DiscordMessage message)
        {
            if (Blocked) return false;
            
            TimeSpan minDelay = TimeSpan.FromMinutes(0.1);

            if (Timestamp.FromDateTime(DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc) - minDelay) <= LastUpdated)
            {
                return false;
            }

            int countedMessageLength = 100;
            int count = (message.Content.Length + countedMessageLength)/countedMessageLength;

            LastUpdated = Timestamp.FromDateTime(DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc));
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
            User user = await Firebase.Instance.GetUser(member.Id, member.Guild.Id);
            return user ?? await GetFreshUser(member);
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

            return await Firebase.Instance.CreateUser(u);
        }

        private void SaveUser()
        {
            Firebase.Instance.SaveUser(this).GetAwaiter().GetResult();
        }
        
        private User(ulong userId, ulong guildId, DateTimeOffset joinDate, int countedMessages, int specialPoints, bool blocked)
        {
            UserId = userId;
            GuildId = guildId;
            JoinDate = Timestamp.FromDateTimeOffset(DateTime.SpecifyKind(joinDate.DateTime, DateTimeKind.Utc));

            Blocked = blocked;

            CountedMessages = countedMessages;
            SpecialPoints = specialPoints;

            LastUpdated = Timestamp.FromDateTime(DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc));
        }
        
        public User() {}
    }
}