using System.Collections.Generic;
using Google.Cloud.Firestore;
using SudoBot.Database;

namespace SudoBot.DataInterfaces
{
    [FirestoreData]
    public class BotConfig
    {
        [FirestoreProperty]
        public string Name { get; set; }
        
        [FirestoreProperty]
        public string Token { get; set; }
        [FirestoreProperty]
        public List<string> Prefixes { get; set; }
    }
}