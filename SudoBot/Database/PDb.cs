using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Dapper;
using DSharpPlus.Entities;
using Npgsql;
using SudoBot.DataInterfaces;

namespace SudoBot.Database
{
    public class PDb
    {
        public async Task Test(DiscordMember member)
        {
            using (IDbConnection connection = new NpgsqlConnection(Environment.GetEnvironmentVariable("DBSTRING")))
            {
                var u = await User.GetOrCreateUser(member);
                var dbRes = connection.QueryFirst<User>("SELECT * FROM \"Users\"");
                Console.WriteLine("test");
            }
        }

        public async Task<int> GetUserRank(User user)
        {
            // ROW_NUMBER is what u need 
            throw new NotImplementedException();
        }
    }
}