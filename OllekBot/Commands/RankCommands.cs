using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using OllekBot.DataInterfaces;
using OllekBot.Handlers;

namespace OllekBot.Commands
{
    public class RankCommands: BaseCommandModule
    {
        [Command("rank")]
        public async Task Rank(CommandContext ctx)
        {
            var user = UserHandler.GetOrCreateUser(ctx.Member);
            await ctx.Channel.SendMessageAsync($"{ctx.Member.Mention} hat {user.CalculatePoints().ToString()} Punkte");
        }
        
        [Command("giveSP")]
        public async Task GiveSp(CommandContext ctx, DiscordMember member, int count)
        {
            var user = UserHandler.GetOrCreateUser(member);
            user.CountedMessages += count;
            await ctx.Channel.SendMessageAsync($"{member.Mention} hat {user.CountedMessages.ToString()}xp erhalten");
        }
    }
}