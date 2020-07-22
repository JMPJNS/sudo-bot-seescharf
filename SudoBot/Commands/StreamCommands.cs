using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Net;
using Newtonsoft.Json;
using SudoBot.Attributes;
using SudoBot.Parser;

namespace SudoBot.Commands
{
    [Group("search"), Aliases("s")]
    [Description("Commands zum Sachen im Internet Suchen")]
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

        [GroupCommand]
        [CheckForPermissions(SudoPermission.Any, GuildPermission.Any)]
        public async Task Search(CommandContext ctx, [RemainingText]string searchTerm)
        {
            // Edit Search Engine: https://cse.google.com/cse/setup/basic?cx=005934734475344700205%3Abkvzqh68i3s
            var apiKey = Environment.GetEnvironmentVariable("GOOGLE_API_KEY");
            var anilistRes = await MakeSearch("005934734475344700205:wjw658x0xpw", apiKey, searchTerm);
            
            
            var urbanRes = await MakeSearch("005934734475344700205:bnbbnplwmik", apiKey, searchTerm);
            var wikipediaRes = await MakeSearch("005934734475344700205:o3fub-xeqjc", apiKey, searchTerm);
            var githubRes = await MakeSearch("005934734475344700205:vdr_xefxnpa", apiKey, searchTerm);
            var everythingRes = await MakeSearch("005934734475344700205:bkvzqh68i3s", apiKey, searchTerm);
        }

        private async Task<string> MakeSearch(string engine, string apiKey, string searchTerm)
        {
            var url = $"https://www.googleapis.com/customsearch/v1?key={apiKey}&cx={engine}&q={HttpUtility.UrlEncode(searchTerm)}";

            return "test";
        }

        private class SearchResult
        {
            public string Title;
            public string Url;
            public string Image;
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
                    Globals.YoutubeVideoGifParsers.Remove(found);
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
                    Globals.YoutubeVideoGifParsers.Remove(parser);
                    sCount++;
                }

                await ctx.RespondAsync($"Stopped {sCount} streams");
                return;
            }

            await ctx.RespondAsync("No Active Streams found");
        }
    }
}