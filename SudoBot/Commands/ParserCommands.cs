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
        [CheckForPermissions(SudoPermission.Mod, GuildPermission.Any)]
        public async Task ParseHytale(CommandContext ctx)
        {
            var hp = new HytaleParser();
            var res = await hp.ParseAsync(0, 1);

            var embed = new DiscordEmbedBuilder()
                .WithColor(DiscordColor.CornflowerBlue)
                .WithTitle($"Hytale: {res.Posts?[0].Titel}")
                .WithImageUrl(res.Posts?[0].ImgUrl)
                .WithDescription(res.Posts?[0].Details)
                .WithUrl(res.Posts?[0].PostUrl)
                .AddField(res.Posts?[0].Date, res.Posts?[0].Author);


            await ctx.Channel.SendMessageAsync(embed: embed.Build());
        }
        
        [Command("youtube-channel"), Aliases("yc")]
        [CheckForPermissions(SudoPermission.Mod, GuildPermission.Any)]
        public async Task ParseYoutubeChannel(CommandContext ctx, string channelId)
        {
            var ycp = new YoutubeChannelParser();
            var res = await ycp.ParseAsync($"https://www.youtube.com/channel/{channelId}/videos");

            var embed = new DiscordEmbedBuilder()
                .WithColor(DiscordColor.IndianRed)
                .WithTitle($"Youtube: {res.Name}")
                .WithThumbnailUrl(res.ImgUrl)
                .WithDescription($"Subscriber: {res.SubCountString}")
                .WithUrl(res.Url);

            if (res.NoVideo == false)
            {
                embed.WithImageUrl(res.LatestVideoThumbnailUrl)
                    .AddField($"Letztes Video", $"[{res.LatestVideoTitle}] [{res.LatestVideoViewCount}]")
                    .AddField("Link", $"https://youtube.com{res.LatestVideoUrl}");
            }

            await ctx.Channel.SendMessageAsync(embed: embed.Build());
        }
    }
}