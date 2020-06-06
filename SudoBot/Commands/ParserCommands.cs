using System;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using SudoBot.Attributes;
using SudoBot.Parser;

namespace SudoBot.Commands
{
    [Group("parser"), Aliases("p")]
    [Description("Commands für Webscraping")]
    public class ParserCommands: BaseCommandModule
    {
        [Command("hytale")]
        [CheckForPermissions(SudoPermission.Me, GuildPermission.Any)]
        public async Task ParseHytale(CommandContext ctx)
        {
            var hp = new HytaleParser();
            var res = await hp.ParseAsync(0, 1);

            var embed = new DiscordEmbedBuilder()
                .WithColor(DiscordColor.CornflowerBlue)
                .WithTitle(res.Posts?[0].Titel)
                .WithImageUrl(res.Posts?[0].ImgUrl)
                .WithDescription(res.Posts?[0].Details)
                .WithUrl(res.Posts?[0].PostUrl)
                .AddField(res.Posts?[0].Date, res.Posts?[0].Author);


            await ctx.Channel.SendMessageAsync(embed: embed.Build());
        }
    }
}