using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using SudoBot.Models;
using DSharpPlus.CommandsNext.Attributes;
using SudoBot.Database;

namespace SudoBot.Commands
{
    public class TestCommands : BaseCommandModule
    {
        [Command("test")]
        public async Task T(CommandContext ctx)
        {
        }
    }
}