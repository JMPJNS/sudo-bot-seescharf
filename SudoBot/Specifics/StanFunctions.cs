using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

namespace SudoBot.Specifics
{
    public class StanFunctions
    {
        public static async Task RemoveNotVerified(GuildMemberUpdateEventArgs args)
        {
            var verifiziert = args.Guild.GetRole(707343944228405339);
            var nicht = args.Guild.GetRole(707302992310960189);

            if (args.RolesAfter.Contains(verifiziert) && args.RolesAfter.Contains(nicht))
            {
                await args.Member.RevokeRoleAsync(nicht);
                Console.WriteLine("Removed Role");
            }
        }

        public static async Task RemoveAllNotVerified(CommandContext ctx)
        {
            var verifiziert = ctx.Guild.GetRole(707343944228405339);
            var nicht = ctx.Guild.GetRole(707302992310960189);
            
            var allMembers = await ctx.Guild.GetAllMembersAsync();
            var customsMembers = allMembers.Where(user => (user.Roles.Contains(verifiziert) && user.Roles.Contains(nicht)));

            int i = 0;
            
            foreach (DiscordMember member in customsMembers)
            {
                i += 1;
                if (i % 100 == 0)
                {
                    Console.WriteLine(i);
                }
                await member.RevokeRoleAsync(nicht);
            }
        }

    }
}