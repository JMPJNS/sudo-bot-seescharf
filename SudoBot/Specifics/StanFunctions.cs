using System;
using System.Collections.Generic;
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

        public static async Task RemoveAllColorRoles(CommandContext ctx)
        {
            var roles = new List<DiscordRole>();
            
            roles.Add(ctx.Guild.GetRole(799777797873729556));
            roles.Add(ctx.Guild.GetRole(799777129339945010));
            roles.Add(ctx.Guild.GetRole(799777533377773579));
            roles.Add(ctx.Guild.GetRole(799777027505913907));
            roles.Add(ctx.Guild.GetRole(799776726552936459));
            roles.Add(ctx.Guild.GetRole(799776726552936459));
            roles.Add(ctx.Guild.GetRole(799776301505839105));

            int i = 0;

            var allMembers = await ctx.Guild.GetAllMembersAsync();

            foreach (var m in allMembers)
            {
                i++;
                foreach (var r in roles)
                {
                    if (m.Roles.Contains(r))
                    {
                        await ctx.RespondAsync($"Removed ${m.Mention} [{i}/{allMembers.Count}]");
                        await m.RevokeRoleAsync(r);
                    }
                }
            }

            await ctx.RespondAsync("Done");
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