using System.Threading.Tasks;
using DSharpPlus.EventArgs;
using SudoBot.Database;

namespace SudoBot.Handlers
{
    public class MemberUpdateHandler
    {
        public async Task HandleRoleChange(GuildMemberUpdateEventArgs args)
        {
            var guild = Mongo.Instance.GetGuild(args.Guild.Id);

        }
    }
}