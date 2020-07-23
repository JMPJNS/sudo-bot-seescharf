using System;
using MongoDB.Bson.Serialization.Attributes;

namespace SudoBot.Models
{
    [BsonKnownTypes(typeof(ListItem), typeof(AnilistItem))]
    public class ListItem
    {
        public string Name { get; private set; }
        public ulong UserId { get; private set; }
        public DateTime CreatedOn { get; private set; }

        public ListItem(string name, ulong userId)
        {
            Name = name;
            UserId = userId;
            CreatedOn = DateTime.Now;
        }

        public virtual string GetTypeName()
        {
            return "Generic";
        }
    }
}