using System;
using System.Collections.Generic;
using System.Data;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using SudoBot.Models;
using Tag = SudoBot.Models.Tag;

namespace SudoBot.Database
{
    public class Mongo
    {
        public static Mongo Instance { get; } = new Mongo();
        
        private IMongoDatabase _db;
        private IMongoCollection<User> _users;
        private IMongoCollection<Tag> _tags;
        private IMongoCollection<Guild> _guilds;
        private IMongoCollection<Scheduled> _scheduled;
        private IMongoCollection<ParserResult> _parserResults;
        private IMongoCollection<SudoList> _lists;
        private Mongo()
        {
            try
            {
                var client = new MongoClient(Environment.GetEnvironmentVariable("DBSTRING"));
                _db = client.GetDatabase(Environment.GetEnvironmentVariable("DBNAME"));
                
                _users = _db.GetCollection<User>("Users");
                _tags = _db.GetCollection<Tag>("Tags");
                _guilds = _db.GetCollection<Guild>("Guilds");
                _parserResults = _db.GetCollection<ParserResult>("ParserResults");
                _scheduled = _db.GetCollection<Scheduled>("Scheduled");
                _lists = _db.GetCollection<SudoList>("Lists");

                try
                {
                    Globals.Client.Logger.LogInformation("Connecting to Mongo");
                    Globals.Client.Logger.LogInformation("me: " + _users.FindAsync(user => user.UserId == Globals.MyId)
                        .Result.First().UserName);
                }
                catch (Exception e)
                {
                    if (!(e is InvalidOperationException))
                    {
                        Globals.Client.Logger.LogCritical("Error occured while connecting to mongo");
                        Globals.Client.Logger.LogCritical(e.Message);
                        return;
                    }
                }
                
                Globals.Client.Logger.LogInformation("Connected to Mongo");
            }
            catch (Exception e)
            {
                Globals.Client.Logger.Log(LogLevel.Error,  $"Exception occured While connecting to Database: {e.GetType()}: {e.Message}", DateTime.Now);
            }
        }
        
        // Scheduled Stuff
        
        public async Task<Scheduled> GetScheduled(MongoDB.Bson.ObjectId scheduledId)
        {
            try
            {
                return await _scheduled.FindAsync(scheduled => scheduled._id == scheduledId).Result.FirstAsync();
            }
            catch (Exception e)
            {
                return null;
            }
        }
        
        public async Task InsertScheduled(Scheduled scheduled)
        {
            await _scheduled.InsertOneAsync(scheduled);
        }
        
        public async Task UpdateScheduled(Scheduled scheduled)
        {
            await _scheduled.ReplaceOneAsync(
                s => scheduled._id == s._id,
                scheduled);
        }

        public async Task<List<Scheduled>> GetDueScheduled()
        {
            try
            {
                return await _scheduled.FindAsync(scheduled => scheduled.Active && scheduled.ScheduledOn < DateTime.UtcNow).Result.ToListAsync();
            }
            catch (Exception e)
            {
                return null;
            }
        }
        
        // List Stuff

        public async Task<List<SudoList>> GetAllLists(ulong userId)
        {
            try
            {
                return await _lists.FindAsync(list => list.UserId == userId).Result.ToListAsync();
            }
            catch (Exception e)
            {
                return null;
            }
        }
        
        public async Task<long> GetListCount(ulong userId)
        {
            try
            {
                return _lists.CountDocumentsAsync(list => list.UserId == userId).Result;
            }
            catch (Exception e)
            {
                return 0;
            }
        }
        public async Task<SudoList> GetList(ulong userId, string listName)
        {
            try
            {
                return await _lists.FindAsync(list => userId == list.UserId && list.Name == listName).Result.FirstAsync();
            }
            catch (Exception e)
            {
                return null;
            }
        }
        
        public async Task DeleteList(SudoList list)
        {
            await _tags.DeleteOneAsync(l => l.Id == list.Id);
        }
        public async Task InsertList(SudoList list)
        {
            await _lists.InsertOneAsync(list);
        }
        
        public async Task UpdateList(SudoList list)
        {
            await _lists.ReplaceOneAsync(
                l => list.Name == l.Name,
                list);
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
        
        // Parser Stuff
        
        public async Task InsertParserResult(ParserResult res)
        {
            await _parserResults.InsertOneAsync(res);
        }

        public async Task GetLatestParserResult(String parser)
        {
            await _parserResults.Find(r => r.Parser == parser).SortByDescending(r => r.ParsedTime).FirstAsync();
        }
        
        // Tag Stuff
        public async Task InsertTag(Tag tag)
        {
            await _tags.InsertOneAsync(tag);
        }

        public async Task DeleteTag(Tag tag)
        {
            await _tags.DeleteOneAsync(t => t.Id == tag.Id);
        }

        public async Task<Tag> GetTag(String name, TagType type)
        {
            try
            {
                return await _tags.FindAsync(tag => (tag.Type == type && tag.Name == name)).Result.FirstAsync();
            }
            catch (Exception e)
            {
                return null;
            }
        }
        
        public async Task<List<Tag>> FindSimilarTags(String name, TagType type)
        {
            var filter = Builders<Tag>.Filter.Regex("Name", new BsonRegularExpression(name, "i"));
            try
            {
                return _tags.Find(filter).Limit(5).ToList();
            }
            catch (Exception e)
            {
                return null;
            }
        }
        
        public async Task UpdateTag(Tag tag)
        {
            await _tags.ReplaceOneAsync(
                t => tag.Id == t.Id,
                tag);
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
        
        public async Task<long> GetUserCount()
        {
            return await _users.CountDocumentsAsync(x => true);
        }

        public async Task<List<User>> GetLeaderboard(int position, ulong guildId)
        {
            int skip;
            if (position > 5)
            {
                skip = position - 5;
            }
            else
            {
                skip = 0;
            }
            try
            {
                return await _users.Find(u => u.GuildId == guildId)
                    .SortByDescending(x => x.Points)
                    .Skip(skip)
                    .Limit(10)
                    .ToListAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
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
        
        public async Task<long> GetUserRank(User user)
        {
            if (user.Points == 0) user.CalculatePoints();
            return await _users.CountDocumentsAsync(u => u.GuildId == user.GuildId && u.Points > user.Points);
        }

        public async Task<List<User>> GetGuildUsers(ulong guildId)
        {
            return await _users.FindAsync(user => user.GuildId == guildId).Result.ToListAsync();
        }
        
        // Custom Game Stuff
        public async Task<List<User>> GetUsersWithoutTicket(ulong guildId)
        {
            return await _users.FindAsync(user => user.GuildId == guildId && user.TicketsRemaining == 0).Result.ToListAsync();
        }
    }
}