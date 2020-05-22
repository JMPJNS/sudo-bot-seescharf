using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using SudoBot.Attributes;
using SudoBot.Database;
using SudoBot.Models;
using SudoBot.Handlers;

namespace SudoBot.Commands
{
    [Group("rank")]
    public class RankCommands: BaseCommandModule
    {
        [CheckForPermissions(SudoPermission.Any, GuildPermission.Ranking)]
        [Command("rank")]
        public async Task Rank(CommandContext ctx)
        {
            var user = await User.GetOrCreateUser(ctx.Member);
            await ctx.Channel.SendMessageAsync($"{ctx.Member.Mention} hat {user.CountedMessages.ToString()} gesendete Nachrichten, {user.SpecialPoints.ToString()} Spezial Punkte und damit {user.CalculatePoints().ToString()} IQ");
        }
        
        [Command("givePoints")]
        [CheckForPermissions(SudoPermission.Mod, GuildPermission.Ranking)]
        public async Task GiveSp(CommandContext ctx, DiscordMember member, int count)
        {
            var user = await User.GetOrCreateUser(member);
            await user.AddSpecialPoints(count);
            await ctx.Channel.SendMessageAsync($"{member.Mention} hat {count.ToString()} IQ erhalten");
        }

        [Command("setRankingRole")]
        [CheckForPermissions(SudoPermission.Admin, GuildPermission.Ranking)]
        public async Task SetRankingRole(CommandContext ctx, int points, DiscordRole role)
        {
            var guild = await Guild.GetGuild(ctx.Guild.Id);
            await guild.AddRankingRole(role, points);
            await ctx.Channel.SendMessageAsync($"Die Rolle {role.Name} ist mit {points.ToString()} IQ zu Erreichen!");
        }
        
        [Command("removeRankingRole")]
        [CheckForPermissions(SudoPermission.Admin, GuildPermission.Ranking)]
        public async Task RemoveRankingRole(CommandContext ctx, DiscordRole role)
        {
            var guild = await Guild.GetGuild(ctx.Guild.Id);
            var success = await guild.RemoveRankingRole(role);
            if (success)
            {
                await ctx.Channel.SendMessageAsync($"Die Rolle {role.Name} wurde aus dem Ranking entfernt!");
            }
            else
            {
                await ctx.Channel.SendMessageAsync($"Die Rolle {role.Name} ist nicht im Ranking!");
            }
        }

        [Command("rankList")]
        [CheckForPermissions(SudoPermission.Any, GuildPermission.Ranking)]
        public async Task ListRankingRoles(CommandContext ctx)
        {
            var guild = await Guild.GetGuild(ctx.Guild.Id);
            var roles = guild.RankingRoles;

            if (roles.Count == 0)
            {
                throw new DataException("Es wurden noch keine Rollen festgelegt, siehe `$help setRankingRole`");
            } 
            
            var embed = new DiscordEmbedBuilder()
                .WithColor(DiscordColor.Aquamarine)
                .WithTitle("Rollen");
            
            foreach (var r in roles)
            {
                var drole = ctx.Guild.GetRole(r.Role);
                embed.AddField(drole.Mention, r.Points.ToString());
            }
        }
    }
}