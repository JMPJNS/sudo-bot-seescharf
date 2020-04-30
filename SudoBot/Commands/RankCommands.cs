using System;
using System.Threading.Tasks;
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
            var user = UserHandler.GetOrCreateUser(ctx.Member);
            await ctx.Channel.SendMessageAsync($"{ctx.Member.Mention} hat {user.CountedMessages.ToString()} gesendete Nachrichten, {user.SpecialPoints.ToString()} Spezial Punkte und damit {user.CalculatePoints().ToString()} IQ");
        }
        
        [Command("giveSP")]
        [RequireRoles(RoleCheckMode.Any, new []{"Admins", "Mods"})]
        public async Task GiveSp(CommandContext ctx, DiscordMember member, int count)
        {
            var user = UserHandler.GetOrCreateUser(member);
            user.CountedMessages += count;
            await ctx.Channel.SendMessageAsync($"{member.Mention} hat {user.CountedMessages.ToString()} IQ erhalten");
        }
        
        [Command("listUsers")]
        public async Task ListUsers(CommandContext ctx)
        {
            foreach (User user in UserHandler.Users)
            {
                DiscordGuild currentGuild = await ctx.Client.GetGuildAsync(user.GuildId);
                if (ctx.Guild == currentGuild)
                {
                    var message = await ctx.Channel.SendMessageAsync($"{(await currentGuild.GetMemberAsync(user.UserId)).DisplayName}: {user.CalculatePoints().ToString()}IQ");
                }
            }
        }
        
    }
}