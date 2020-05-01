using System;
using System.Linq;
using System.Threading.Tasks;
using Google.Cloud.Firestore;
using SudoBot.DataInterfaces;

namespace SudoBot.Database
{
    public class Firebase
    {
        public static Firebase Instance { get; } = new Firebase();

        private FirestoreDb _db;
        private CollectionReference _users;
        private CollectionReference _customGames;
        private CollectionReference _botConfig;
        private CollectionReference _guildConfig;
        
        private CollectionReference _test;

        private Firebase()
        {
            _db = FirestoreDb.Create("jmp-sudobot");
            
            _users = _db.Collection("Users");
            _customGames = _db.Collection("CustomGames");
            _test = _db.Collection("Test");
            _botConfig = _db.Collection("BotConfig");
            _guildConfig = _db.Collection("GuildConfig");
        }
        
        public async Task<BotConfig> GetBotConfig()
        {
            var edition = Environment.GetEnvironmentVariable("SUDOBOT_EDITION");
            
            var qs = await _botConfig.WhereEqualTo("Name", edition ?? "SudoBotDev").GetSnapshotAsync();;

            return qs.Documents.First().ConvertTo<BotConfig>();
        }
        
        public async Task<BotConfig> GetGuildConfig(ulong guildId)
        {
            
            var qs = await _botConfig.WhereEqualTo("GuildId", guildId).GetSnapshotAsync();

            return qs.Documents.First().ConvertTo<BotConfig>();
        }

        public async Task<User> GetUser(ulong userId, ulong guildId)
        {
            try
            {
                var qs = await _users.WhereEqualTo("UserId", userId).WhereEqualTo("GuildId", guildId)
                    .GetSnapshotAsync();
                return qs.Documents.First().ConvertTo<User>();
            }
            catch (Exception e)
            {
                return null;
            }
            
        }
    }
}