using System;
using System.Threading.Tasks;
using Google.Cloud.Firestore;

namespace SudoBot.Database
{
    public class Firebase
    {
        public static Firebase Instance { get; } = new Firebase();

        private FirestoreDb _db;
        private CollectionReference _users;
        private CollectionReference _emojis;
        private CollectionReference _customGames;
        private CollectionReference _test;

        public int weird;
        
        private Firebase()
        {
            _db = FirestoreDb.Create("ollekbot");
            
            _users = _db.Collection("Users");
            _emojis = _db.Collection("Emojis");
            _customGames = _db.Collection("CustomGames");
            _test = _db.Collection("Test");
        }

        public async Task DoTest()
        {
            Query query = _test.WhereEqualTo("Uno", "ASDF");
            QuerySnapshot querySnapshot = await query.GetSnapshotAsync();
            Console.WriteLine(querySnapshot.Documents[0].GetValue<string>("Uno"));
        }
    }
}