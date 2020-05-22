using System.Threading.Tasks;
using DSharpPlus.EventArgs;
using SudoBot.Database;
using SudoBot.Models;

namespace SudoBot.Handlers
{
    public class MemberUpdateHandler
    {
        public async Task HandleRoleChange(GuildMemberUpdateEventArgs args)
        {
            var guild = Guild.GetGuild(args.Guild.Id);

        }
    }
}