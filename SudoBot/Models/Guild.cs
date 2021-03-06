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
        public string Name { get; set; }
        public int MemberCount { get; set; }

        public int TicketCount { get; set; }
        
        public ulong CustomsRole { get; private set; }
        public ulong CustomsChannel { get; private set; }
        public ulong CustomsEmoji { get; private set; }
        public ulong CustomsMessage { get; private set; }
        
        public ulong LocalLogChannel { get; private set; }
        public ulong CommandChannel { get; private set; }

        public List<RankingRole> RankingRoles { get; private set; }
        public int RankingTimeMultiplier { get; private set; }
        public int RankingMultiplier { get; private set; }
        public string RankingPointName { get; private set; }
        
        public List<ReactionRole> ReactionRoles { get; private set; }
        
        public Translation.Lang Language { get; private set; }

        public List<GuildPermission> Permissions { get; set; }

        public Guild(ulong guildId)
        {
            GuildId = guildId;
            
            RankingRoles = new List<RankingRole>();
            ReactionRoles = new List<ReactionRole>();
            Permissions = new List<GuildPermission>();
            RankingTimeMultiplier = 10;
            RankingMultiplier = 1;
            RankingPointName = "XP";
            TicketCount = 1;
            Language = Translation.Lang.En;
        }
        
        public class RankingRole
        {
            public ulong Role;
            public int Points;
        }

        public class ReactionRole
        {
            public ulong MessageId;
            public ulong ChannelId;
            public ulong EmojiId;
            public ulong RoleId;

            public ReactionRole(ulong messageId, ulong channelId, ulong emojiId, ulong roleId)
            {
                MessageId = messageId;
                ChannelId = channelId;
                EmojiId = emojiId;
                RoleId = roleId;
            }
        }
        
        public async Task SaveGuild()
        {
            var cacheIndex = Globals.GuildCache.FindIndex(x => x.GuildId == GuildId);
            if (cacheIndex != -1) Globals.GuildCache[cacheIndex] = this;
            await Mongo.Instance.UpdateGuild(this);
        }

        public async Task GivePermission(GuildPermission perm)
        {
            Permissions.Add(perm);
            await SaveGuild();
        }

        public async Task AddReactionRole(ReactionRole role)
        {
            ReactionRoles ??= new List<ReactionRole>();
            ReactionRoles.Add(role);
            await SaveGuild();
        }
        
        public async Task RemoveReactionRole(ReactionRole role)
        {
            if (ReactionRoles == null) return;
            
            var r = ReactionRoles.FirstOrDefault(x => x == role);
            ReactionRoles.Remove(r);
            await SaveGuild();
        }

        public async Task SetRankingPointsName(string name)
        {
            RankingPointName = name;
            await SaveGuild();
        }

        public async Task SetLanguage(Translation.Lang lang)
        {
            Language = lang;
            await SaveGuild();
        }

        public async Task SetRankingTimeMultipier(int num)
        {
            RankingTimeMultiplier = num;
            await SaveGuild();
        }
        
        public async Task SetRankingMultipier(int num)
        {
            RankingMultiplier = num;
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

        public async Task SetLocalLogChannel(ulong channelId)
        {
            LocalLogChannel = channelId;
            await SaveGuild();
        }
        
        public async Task SetCommandChannel(ulong channelId)
        {
            CommandChannel = channelId;
            await SaveGuild();
        }

        public async Task SetCustoms(ulong message, ulong channel, ulong emoji)
        {
            CustomsChannel = channel;
            CustomsMessage = message;
            CustomsEmoji = emoji;

            await SaveGuild();
        }

        public async Task ResetTickets()
        {
            var users = await Mongo.Instance.GetUsersWithoutTicket(GuildId);
            foreach (var user in users)
            {
                await user.AddTickets(TicketCount);
            }
        }
        
        // Ranking Stuff

        public async Task AddRankingRole(DiscordRole role, int points)
        {
            if (RankingRoles == null) RankingRoles = new List<RankingRole>();
            
            var rr = new RankingRole();
            rr.Points = points;
            rr.Role = role.Id;
            RankingRoles.Add(rr);
            await SaveGuild();
        }

        public async Task<bool> RemoveRankingRole(DiscordRole role)
        {
            return await RemoveRankingRole(role.Id);
        }
        
        public async Task<bool> RemoveRankingRole(ulong roleId)
        {
            var rr = RankingRoles.FirstOrDefault(r => r.Role == roleId);
            if (rr == null)
            {
                return false;
            }
            else
            {
                RankingRoles.Remove(rr);
                await SaveGuild();
                return true;
            }
        }
        
        public static async Task<Guild> GetGuild(ulong guildId)
        {
            var cached = Globals.GuildCache.FirstOrDefault(x => x.GuildId == guildId);

            if (cached != null)
            {
                return cached;
            }
            
            var guild = await Mongo.Instance.GetGuild(guildId);
            if (guild != null) Globals.GuildCache.Add(guild);
            return guild;
        }
    }
}