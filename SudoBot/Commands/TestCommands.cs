using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using SudoBot.Attributes;

namespace SudoBot.Commands
{
    public class TestCommands : BaseCommandModule
    {
        [CheckForPermissions(SudoPermission.Any, GuildPermission.TestCommands)]
        [Command("test")]
        public async Task T(CommandContext ctx)
        {
        }
    }
}