using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using DSharpPlus;
using MongoDB.Bson;
using MongoDB.Driver;
using SudoBot.Models;

namespace SudoBot.Database
{
    public class Mongo
    {
        public static Mongo Instance { get; } = new Mongo();
        
        private IMongoDatabase _db;
        private IMongoCollection<User> _users;
        private IMongoCollection<Guild> _guilds;
        private Mongo()
        {
            try
            {
                var client = new MongoClient(Environment.GetEnvironmentVariable("DBSTRING"));
                _db = client.GetDatabase(Environment.GetEnvironmentVariable("DBNAME"));
                
                _users = _db.GetCollection<User>("Users");
                _guilds = _db.GetCollection<Guild>("Guilds");
            }
            catch (Exception e)
            {
                Globals.Logger.LogMessage(LogLevel.Error, "SudoBot", $"Exception occured While connecting to Database: {e.GetType()}: {e.Message}", DateTime.Now);
            }
        }
        
        // Guild Stuff

        public async Task<Guild> GetGuild(ulong guildId)
        {
            try
            {
                return await _guilds.FindAsync(guild => guild.GuildId == guildId).Result.FirstAsync();
            }
            catch (Exception e)
            {
                return null;
            }
        }
        
        public async Task InsertGuild(Guild guild)
        {
            await _guilds.InsertOneAsync(guild);
        }
        
        public async Task UpdateGuild(Guild guild)
        {
            await _guilds.ReplaceOneAsync(
                g => guild.GuildId == g.GuildId,
                guild);
        }


        // User Stuff
        public async Task<User> GetUser(ulong userId, ulong guildId)
        {
            try
            {
                return await _users.FindAsync(user => user.UserId == userId && user.GuildId == guildId).Result.FirstAsync();
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
        
        public async Task<int> GetUserRank(User user)
        {
            // Count how many users have a higher score
            throw new NotImplementedException();
        }
        
        // Custom Game Stuff
        public async Task<List<User>> GetUsersWithoutTicket(ulong guildId)
        {
            return await _users.FindAsync(user => user.GuildId == guildId && user.TicketsRemaining == 0).Result.ToListAsync();
        }
    }
}