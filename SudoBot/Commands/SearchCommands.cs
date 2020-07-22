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
using Newtonsoft.Json.Linq;
using SudoBot.Attributes;
using SudoBot.Parser;

namespace SudoBot.Commands
{
    [Group("search"), Aliases("s")]
    [Description("Commands zum Sachen im Internet Suchen")]
    public class SearchCommands: BaseCommandModule
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
        [Cooldown(1, 20, CooldownBucketType.User)]
        [CheckForPermissions(SudoPermission.Any, GuildPermission.Any)]
        public async Task Search(CommandContext ctx, [RemainingText]string searchTerm)
        {
            // Edit Search Engine: https://cse.google.com/cse/setup/basic?cx=005934734475344700205%3Abkvzqh68i3s
            var last = searchTerm.Last().ToString();
            int resCount = 1;
            try
            {
                resCount = Convert.ToInt32(last);
            }
            catch (Exception e) { }

            var trimmedTerm = searchTerm.Remove(searchTerm.Length - 1).Trim();

            if (trimmedTerm == "hytale" || searchTerm == "hytale")
            {
                await ParseHytale(ctx);
                resCount--;
            }

            var apiKey = Environment.GetEnvironmentVariable("GOOGLE_API_KEY");
            List<SearchResult> results = new List<SearchResult>();
            var anilistRes = await MakeSearch("005934734475344700205:wjw658x0xpw", apiKey, searchTerm);
            if (anilistRes != null) results.Add(anilistRes);


            if (results.Count < resCount)
            {
                var urbanRes = await MakeSearch("005934734475344700205:bnbbnplwmik", apiKey, searchTerm);
                if (urbanRes != null) results.Add(urbanRes);
            }

            // if (results.Count < resCount)
            // {
            //     var youtubeRes = await MakeSearch("005934734475344700205:kki-evoqkn0", apiKey, searchTerm);
            //     if (youtubeRes != null) results.Add(youtubeRes);
            // }
            //
            // if (results.Count < resCount)
            // {
            //     var wikiRes = await MakeSearch("005934734475344700205:o3fub-xeqjc", apiKey, searchTerm);
            //     if (wikiRes != null) results.Add(wikiRes);
            // }
            //
            // if (results.Count < resCount)
            // {
            //     var githubRes = await MakeSearch("005934734475344700205:vdr_xefxnpa", apiKey, searchTerm);
            //     if (githubRes != null) results.Add(githubRes);
            // }

            if (results.Count < resCount)
            {
                var everythingRes = await MakeSearch("005934734475344700205:bkvzqh68i3s", apiKey, searchTerm);
                if (everythingRes != null) results.Add(everythingRes);
            }

            if (resCount != 0 && results.Count == 0)
            {
                await ctx.Channel.SendMessageAsync("Keine Ergebnisse Gefunden");
                return;
            }

            foreach (var res in results)
            {
                var embed = new DiscordEmbedBuilder();
                embed.WithTitle(res.Title);
                embed.WithUrl(res.Url);
                if (res.ImageUrl != null) embed.WithThumbnailUrl(res.ImageUrl);
                if (res.Snippet != null) embed.WithDescription(res.Snippet);
                await ctx.Channel.SendMessageAsync(embed: embed.Build());
            }
        }
        
        private async Task ParseHytale(CommandContext ctx)
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

        private async Task<SearchResult> MakeSearch(string engine, string apiKey, string searchTerm)
        {
            var url = $"https://www.googleapis.com/customsearch/v1?key={apiKey}&cx={engine}&q={HttpUtility.UrlEncode(searchTerm)}";

            var request = await WebRequest.Create(url).GetResponseAsync();
            
            using (Stream data = request.GetResponseStream())
            {
                StreamReader reader = new StreamReader(data);
                var res = await reader.ReadToEndAsync();
                var items = JObject.Parse(res)["items"];
                if (items != null && items[0] != null)
                {
                    var item = items[0];
                    SearchResult sr = new SearchResult();
                    sr.Title = item["title"].ToString();
                    sr.Url = item["link"].ToString();
                    sr.Snippet = item["snippet"].ToString();

                    if (item["pagemap"] != null && item["pagemap"]["cse_thumbnail"] != null &&
                        item["pagemap"]["cse_thumbnail"][0] != null)
                    {
                        sr.ImageUrl = item["pagemap"]["cse_thumbnail"][0]["src"].ToString();
                    }

                    request.Close();
                    return sr;
                }
            }
            request.Close();
            
            return null;
        }

        private class SearchResult
        {
            public string Title;
            public string Url;
            public string ImageUrl;
            public string Snippet;
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