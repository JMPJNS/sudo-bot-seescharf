using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using SudoBot.Database;

namespace SudoBot.Models
{
    public class Tag
    {
        [BsonId] public ObjectId Id { get; private set; }
        
        public String Name { get; set; }
        public String Content { get; set; }
        
        public TagType Type { get; set; }
        
        public ulong UserCreated { get; set; }
        public ulong ChannelCreated { get; set; }
        public ulong GuildCreated { get; set; }
        
        public DateTime DateCreated { get; set; }
        
        
        public async Task UpdateTag()
        {
            await Mongo.Instance.UpdateTag(this);
        }
        
        public async Task CreateTag()
        {
            await Mongo.Instance.InsertTag(this);
        }

        public async Task DeleteTag()
        {
            await Mongo.Instance.DeleteTag(this);
        }

        public static async Task<List<Tag>> FindSimilarTags(String name, TagType type = TagType.Guild)
        {
            return await Mongo.Instance.FindSimilarTags(name, type);
        }

        public static async Task<Tag> GetTag(String name, TagType type = TagType.Guild)
        {
            return await Mongo.Instance.GetTag(name, type);
        }

    }
    
    public enum TagType
    {
        User,
        Channel,
        Guild,
        Global
    }
}