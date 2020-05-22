using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using SudoBot.Attributes;

namespace SudoBot.Commands
{
    [Group("test")]
    public class TestCommands : BaseCommandModule
    {
        [CheckForPermissions(SudoPermission.Admin, GuildPermission.TestCommands)]
        [Command("test")]
        public async Task T(CommandContext ctx, DiscordChannel channel)
        {


            var embed = new DiscordEmbedBuilder()
                .WithTitle("Das IQ-System")
                .WithColor(DiscordColor.Aquamarine)
                .WithDescription(
                    $@"Wir haben ein System, mit dem ihr durch Aktivität und auch reine Anwesenheit auf dem Server IQ sammeln könnt.
 
Dieses IQ-Punkte stellen ein komplett neues Levelsystem da, mit dem ihr ab einer gewissen Anzahl an IQ Rollen erhaltet.
 
Spammen wird nicht belohnt und verstößt zudem noch gegen die {channel.Mention}, also haltet euch an diese!

Außerdem könnt ihr auch durch Teilnahme an Contests etc. IQ verdienen, es lohnt sich also, auch an diesen teilzunehmen. ");
            await ctx.Channel.SendMessageAsync(embed: embed.Build());

        }
    }
}