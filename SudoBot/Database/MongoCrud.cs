using System;
using System.Data;
using System.Threading.Tasks;
using DSharpPlus;
using MongoDB.Driver;
using SudoBot.Models;

namespace SudoBot.Database
{
    public class MongoCrud
    {
        public static MongoCrud Instance { get; } = new MongoCrud();
        
        private IMongoDatabase _db;
        private IMongoCollection<User> _users;
        private MongoCrud()
        {
            try
            {
                var client = new MongoClient(Environment.GetEnvironmentVariable("DBSTRING"));
                _db = client.GetDatabase("SudoBotDev");
                
                _users = _db.GetCollection<User>("Users");
            }
            catch (Exception e)
            {
                Globals.Logger.LogMessage(LogLevel.Error, "SudoBot", $"Exception occured While connecting to Database: {e.GetType()}: {e.Message}", DateTime.Now);
            }
        }


        public async Task<User> GetUser(ulong userId, ulong guildId)
        {
            try
            {
                return _users.FindAsync(user => user.UserId == userId && user.GuildId == guildId).Result.First();
            }
            catch (Exception e)
            {
                return null;
            }
            
        }

        public async Task InsertUser(User user)
        {
            await _users.InsertOneAsync(user);
        }

        public async Task UpdateUser(User user)
        {
            await _users.ReplaceOneAsync(
                u => user.UserId == u.UserId && user.GuildId == u.GuildId,
                user);
        }
        
        public int GetUserRank(User user)
        {
            throw new NotImplementedException();
        }
    }
}