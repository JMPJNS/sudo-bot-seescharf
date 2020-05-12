using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using SudoBot.Database;

namespace SudoBot.Models
{
    public class Guild
    {
        [BsonId]
        public ObjectId Id { get; set; }
        public ulong GuildId { get; set; }

        public int TicketCount { get; set; }
        
        public ulong CustomsRole { get; private set; }
        
        private List<RankingRole> RankingRoles { get; set; }

        public List<GuildPermission> Permissions { get; set; }

        public Guild(ulong guildId)
        {
            GuildId = guildId;
            
            RankingRoles = new List<RankingRole>();
            Permissions = new List<GuildPermission>();

            TicketCount = 1;
        }
        
        private class RankingRole
        {
            public ulong Role;
            public int Points;
        }
        
        
        private async Task SaveGuild()
        {
            await MongoCrud.Instance.UpdateGuild(this);
        }

        public async Task GivePermission(GuildPermission perm)
        {
            Permissions.Add(perm);
            await SaveGuild();
        }
        
        // Custom Games stuff

        public async Task RemoveAllCustomsRole(CommandContext ctx)
        {
            if (CustomsRole == 0) return;
            
            var role = ctx.Guild.GetRole(CustomsRole);
            var allMembers = await ctx.Guild.GetAllMembersAsync();
            var customsMembers = allMembers.Where(user => user.Roles.Contains(role));
            
            foreach (DiscordMember member in customsMembers)
            {
                await member.RevokeRoleAsync(role);
            }
        }

        public async Task SetCustomsRole(ulong roleId)
        {
            CustomsRole = roleId;
            await SaveGuild();
        }

        public async Task ResetTickets()
        {
            var users = await MongoCrud.Instance.GetUsersWithoutTicket(GuildId);
            foreach (var user in users)
            {
                await user.AddTickets(TicketCount);
            }
        }
        
        // Ranking Stuff

        public async Task AddRankingRole(DiscordRole role, int points)
        {
            var rr = new RankingRole();
            rr.Points = points;
            rr.Role = role.Id;
            RankingRoles.Add(rr);
            await SaveGuild();
        }
    }
}