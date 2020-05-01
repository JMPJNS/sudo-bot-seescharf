using System;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using SudoBot.Database;
using SudoBot.DataInterfaces;
using SudoBot.Handlers;

namespace SudoBot.Commands
{
    public class RankCommands: BaseCommandModule
    {
        [Command("rank")]
        public async Task Rank(CommandContext ctx)
        {
            var user = await User.GetOrCreateUser(ctx.Member);
            await ctx.Channel.SendMessageAsync($"{ctx.Member.Mention} hat {user.CountedMessages.ToString()} gesendete Nachrichten, {user.SpecialPoints.ToString()} Spezial Punkte und damit {user.CalculatePoints().ToString()} IQ");
        }
        
        [Command("giveSP")]
        [RequireRoles(RoleCheckMode.Any, new []{"SudoBotAdmin", "SudoBotMod", "Admins", "Mods"})]
        public async Task GiveSp(CommandContext ctx, DiscordMember member, int count)
        {
            var user = await User.GetOrCreateUser(member);
            user.AddSpecialPoints(count);
            await ctx.Channel.SendMessageAsync($"{member.Mention} hat {user.CountedMessages.ToString()} IQ erhalten");
        }
    }
}