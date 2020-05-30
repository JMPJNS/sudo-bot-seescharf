using System.Threading.Tasks;
using DSharpPlus.EventArgs;
using SudoBot.Database;
using SudoBot.Models;
using SudoBot.Specifics;

namespace SudoBot.Handlers
{
    public class MemberUpdateHandler
    {
        public async Task HandleRoleChange(GuildMemberUpdateEventArgs args)
        {
            var guild = await Guild.GetGuild(args.Guild.Id);

            await StanFunctions.RemoveNotVerified(args);

        }
    }
}