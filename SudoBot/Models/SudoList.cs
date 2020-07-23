using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using SudoBot.Database;

namespace SudoBot.Models
{
    public class SudoList
    {
        public static int MaxItems = 1000;
        
        [BsonId]
        public ObjectId Id { get; set; }

        public string Name { get; private set; }
        public ulong UserId { get; private set; }
        public DateTime CreatedOn { get; private set; }
        public List<ListItem> Items { get; private set; }

        public SudoList(string name, ulong userId)
        {
            Name = name;
            UserId = userId;
            CreatedOn = DateTime.Now;
            Items = new List<ListItem>();

            CreateList().GetAwaiter().GetResult();
        }
        
        public async Task SaveList()
        {
            await Mongo.Instance.UpdateList(this);
        }
        
        private async Task CreateList()
        {
            var maxLists = 25;
            var foundList = await GetList(UserId, Name);
            
            var listCount = await Mongo.Instance.GetListCount(UserId);
            
            if (listCount >= maxLists) throw new OverflowException("Maximale Listen Anzahl erreicht");

            if (foundList != null)
            {
                throw new DuplicateNameException("List already exists");
            }
            await Mongo.Instance.InsertList(this);
        }

        public static async Task<List<SudoList>> GetAllLists(ulong userId)
        {
            return await Mongo.Instance.GetAllLists(userId);
        }

        public static async Task<SudoList> GetList(ulong userId, string name)
        {
            return await Mongo.Instance.GetList(userId, name);
        }

        public async Task InsertItem(ListItem item)
        {
            if (Items.Count < MaxItems)
            {
                if (Items.Any(i => i.Name == item.Name))
                {
                    throw new DuplicateNameException("Item Exestiert Bereits");
                }
                Items.Add(item);
            }
            else
            {
                throw new OverflowException("Liste ist voll");
            }

            await SaveList();
        }
    }
}