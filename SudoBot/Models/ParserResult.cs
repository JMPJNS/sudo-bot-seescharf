using System;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using SudoBot.Database;

namespace SudoBot.Models
{
    public class ParserResult
    {
        [BsonId]
        public ObjectId Id { get; set; }
        public DateTime ParsedTime { get; set; }
        public String Parser { get; set; }
        
        public async Task Insert()
        {
            await Mongo.Instance.InsertParserResult(this);
        }
    }
}