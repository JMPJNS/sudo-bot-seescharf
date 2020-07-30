using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Newtonsoft.Json;
using SudoBot.Attributes;
using SudoBot.Parser;

namespace SudoBot.Commands
{
    [Group("parser"), Aliases("p"), Hidden]
    [Description("Commands für Webscraping")]
    public class ParserCommands: BaseCommandModule
    {
        [Command("youtube-channel"), Aliases("yc")]
        [CheckForPermissions(SudoPermission.Mod, GuildPermission.Any)]
        public async Task ParseYoutubeChannel(CommandContext ctx, string channelId)
        {
            var ycp = new YoutubeChannelParser();
            var res = await ycp.ParseAsync($"https://www.youtube.com/channel/{channelId}/videos");

            var embed = new DiscordEmbedBuilder()
                .WithColor(DiscordColor.IndianRed)
                .WithTitle($"Youtube: {res.Name}")
                .WithThumbnail(res.ImgUrl)
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

        [Command("covid19"), Hidden]
        [CheckForPermissions(SudoPermission.Any, GuildPermission.Any)]
        public async Task Covid19(CommandContext ctx, string country)
        {
            var url = $"https://api.covid19api.com/live/country/{country}";
            
            var request = await WebRequest.Create(url).GetResponseAsync();

            using (Stream data = request.GetResponseStream())
            {
                StreamReader reader = new StreamReader(data);
                var ninfo = await reader.ReadToEndAsync();
                var json = JsonConvert.DeserializeObject<List<CovidData>>(ninfo);
            
                if (json == null)
                {
                    await ctx.RespondAsync($"{country} nicht gefunden.");
                    return;
                }

                var el = json.Last();

                var embed = new DiscordEmbedBuilder()
                    .WithTitle($"Aktuelle Fälle in {country}")
                    .WithDescription(ninfo);
                await ctx.Channel.SendMessageAsync(embed: embed.Build());
            }
            request.Close();
        }
    }

    public class CovidData
    {
        public string Country;
        public string CountryCode;
        public string Province;
        public string City;
        public string CityCode;
        public string Lat;
        public string Lon;
        public int Confirmed;
        public int Deaths;
        public int Recovered;
        public int Active;
        public DateTime Date;
    }
}