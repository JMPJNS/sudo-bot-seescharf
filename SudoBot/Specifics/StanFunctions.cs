using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.EventArgs;

namespace SudoBot.Specifics
{
    public class StanFunctions
    {
        public static async Task RemoveVerified(GuildMemberUpdateEventArgs args)
        {
            var verifiziert = args.Guild.GetRole(707343944228405339);
            var nicht = args.Guild.GetRole(707302992310960189);

            if (args.RolesAfter.Contains(verifiziert) && args.RolesAfter.Contains(nicht))
            {
                await args.Member.RevokeRoleAsync(nicht);
            }
        }
    }
}