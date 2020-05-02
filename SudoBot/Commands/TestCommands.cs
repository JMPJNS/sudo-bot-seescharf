using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using SudoBot.DataInterfaces;
using DSharpPlus.CommandsNext.Attributes;
using SudoBot.Database;

namespace SudoBot.Commands
{
    public class TestCommands : BaseCommandModule
    {
        [Command("test")]
        public async Task T(CommandContext ctx)
        {
            PDb tDB = new PDb();
            await tDB.Test(ctx.Member);
        }
    }
}