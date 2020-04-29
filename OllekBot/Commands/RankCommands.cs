using System;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using OllekBot.Database;
using OllekBot.Handlers;

namespace OllekBot.Commands
{
    public class RankCommands: BaseCommandModule
    {
        [Command("rank")]
        public async Task Rank(CommandContext ctx)
        {
            var user = UserHandler.GetOrCreateUser(ctx.Member);
            await ctx.Channel.SendMessageAsync($"{ctx.Member.Mention} hat {user.CalculatePoints().ToString()} IQ");
        }
        
        [Command("giveSP")]
        [RequireRoles(RoleCheckMode.Any, new []{"Admins", "Mods"})]
        public async Task GiveSp(CommandContext ctx, DiscordMember member, int count)
        {
            var user = UserHandler.GetOrCreateUser(member);
            user.CountedMessages += count;
            await ctx.Channel.SendMessageAsync($"{member.Mention} hat {user.CountedMessages.ToString()} IQ erhalten");
        }
        
        [Command("tweo")]
        public async Task Tweo(CommandContext ctx)
        {
            Firebase fb = Firebase.Instance;
            Firebase fb2 = Firebase.Instance;

            fb.weird = 12;
            await fb.DoTest();
            await fb2.DoTest();
            
            Console.WriteLine(fb2.weird.ToString());
        }
        
    }
}