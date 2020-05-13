using System;
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
    public class RankCommands: BaseCommandModule
    {
        [CheckForPermissions(SudoPermission.Any, GuildPermission.Ranking)]
        [Command("rank")]
        public async Task Rank(CommandContext ctx)
        {
            var user = await User.GetOrCreateUser(ctx.Member);
            await ctx.Channel.SendMessageAsync($"{ctx.Member.Mention} hat {user.CountedMessages.ToString()} gesendete Nachrichten, {user.SpecialPoints.ToString()} Spezial Punkte und damit {user.CalculatePoints().ToString()} IQ");
        }
        
        [Command("giveSP")]
        [CheckForPermissions(SudoPermission.Mod, GuildPermission.Ranking)]
        public async Task GiveSp(CommandContext ctx, DiscordMember member, int count)
        {
            var user = await User.GetOrCreateUser(member);
            await user.AddSpecialPoints(count);
            await ctx.Channel.SendMessageAsync($"{member.Mention} hat {count} IQ erhalten");
        }

        [Command("setRankingRole")]
        [CheckForPermissions(SudoPermission.Admin, GuildPermission.Ranking)]
        public async Task SetRankingRole(CommandContext ctx, int points, DiscordRole role)
        {
            var guild = await MongoCrud.Instance.GetGuild(ctx.Guild.Id);
            await guild.AddRankingRole(role, points);
            await ctx.Channel.SendMessageAsync($"Die Rolle {role.Name} ist mit {points.ToString()} IQ zu Erreichen!");
        }
    }
}