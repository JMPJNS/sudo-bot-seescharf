using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Fizzler.Systems.HtmlAgilityPack;
using HtmlAgilityPack;
using ScrapySharp.Extensions;
using ScrapySharp.Network;
using SudoBot.Models;

namespace SudoBot.Parser
{
    public class YoutubeChannelParser
    {
        public async Task<YoutubeChannelParserResult> ParseAsync(string url)
        {
            ScrapingBrowser browser = new ScrapingBrowser();
            WebPage page = await browser.NavigateToPageAsync(new Uri(url));
            
            
            var res = new YoutubeChannelParserResult();
            // page.Html.InnerHtml.Substring(page.Html.InnerHtml.IndexOf("181K") - 200, page.Html.InnerHtml.IndexOf("181K") + 100)

            res.SubCountString = page.Html.QuerySelector(".yt-subscription-button-subscriber-count-branded-horizontal")
                .InnerText;

            res.Name = page.Html.QuerySelector("title").InnerText;
            res.Name = res.Name.Substring(0, res.Name.IndexOf("\n")).Trim();
            res.ImgUrl = page.Html.QuerySelector(".appbar-nav-avatar").GetAttributeValue("src", "src");
            res.Url = url;
            try
            {
                res.LatestVideoUrl = page.Html.QuerySelector("div.yt-lockup-content").QuerySelector("[href]")
                    .GetAttributeValue("href", "href");

                res.LatestVideoThumbnailUrl = page.Html.QuerySelectorAll("[data-ytimg]").ToImmutableList()
                    .Find(x => x.OuterHtml.Contains("ytimg.com")).GetAttributeValue("src", "src");

                res.LatestVideoTitle = page.Html.QuerySelector(".yt-lockup-title").InnerText;
                res.LatestVideoViewCount = page.Html.QuerySelector(".yt-lockup-meta-info").FirstChild.InnerText;
                res.NoVideo = false;
            }
            catch (Exception)
            {
                res.NoVideo = true;
            }

            return res;
        }
    }
    
    
    public class YoutubeChannelParserResult : ParserResult
    {
        public string ImgUrl;
        public string SubCountString;
        public string Name;
        public string Url;

        public bool NoVideo;
        
        public string LatestVideoUrl;
        public string LatestVideoThumbnailUrl;
        public string LatestVideoTitle;
        public string LatestVideoViewCount;
        
        public YoutubeChannelParserResult()
        {
            Parser = "YoutubeChannel";
            ParsedTime = DateTime.Now;
        }
    }
}