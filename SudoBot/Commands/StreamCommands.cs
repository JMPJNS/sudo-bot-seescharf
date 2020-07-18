using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Net;
using SudoBot.Attributes;
using SudoBot.Parser;

namespace SudoBot.Commands
{
    public class StreamCommands: BaseCommandModule
    {
        // get length with youtube-dl
        // -ss = starting timestamp, -t = end timestamp
        // get short gif, play back, in the meantime get next gif...
        // execute command https://www.codeproject.com/Articles/25983/How-to-Execute-a-Command-in-C
        // ffmpeg -ss 30 -t 3 -i $(youtube-dl -f 18 --get-url https://www.youtube.com/watch?v=07d2dXHYb94) -segment_time 00:00:15 -vf "fps=10,scale=320:-1:flags=lanczos,split[s0][s1];[s0]palettegen[p];[s1][p]paletteuse" -loop 0 output%03d.gif

        [Command("yt")]
        [CheckForPermissions(SudoPermission.Me, GuildPermission.Any)]
        public async Task YoutubeGifStream(CommandContext ctx, string url)
        {
            await ctx.Message.DeleteAsync();
            YoutubeVideoGifParser parser = new YoutubeVideoGifParser();
            Globals.YoutubeVideoGifParsers.Add(parser);
            await parser.ParseVideo(ctx, url);
            await ctx.RespondAsync("done");
        }

        [Command("stop-yt")]
        [CheckForPermissions(SudoPermission.Me, GuildPermission.Any)]
        public async Task StopYoutubeGifStream(CommandContext ctx, DiscordUser user = null)
        {
            if (user != null)
            {
                var found = Globals.YoutubeVideoGifParsers.FirstOrDefault(x => x.InitiatedBy == user.Id);
                if (found != null)
                {
                    found.Active = false;
                    await ctx.RespondAsync($"Stopped 1 streams");
                    return;
                }
            }
            else
            {
                int sCount = 0;
                foreach (var parser in Globals.YoutubeVideoGifParsers)
                {
                    parser.Active = false;
                    sCount++;
                }

                await ctx.RespondAsync($"Stopped {sCount} streams");
                return;
            }

            await ctx.RespondAsync("No Active Streams found");
        }
    }
}