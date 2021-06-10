using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fizzler.Systems.HtmlAgilityPack;
using HtmlAgilityPack;
using SudoBot.Models;

namespace SudoBot.Parser
{
    public class HytaleParser
    {
        private string baseUrl = "https://hytale.com";
        private string url = "https://hytale.com/news";
        

        public HytaleParserResult Parse(int startIndex = 0, int limit = 5)
        {
            var web = new HtmlWeb();
            var html = web.Load(url);
            var document = html.DocumentNode;

            var posts = document.QuerySelectorAll(".postWrapper").ToList();

            limit = posts.Count < limit ? posts.Count : limit;
            
            var pr = new HytaleParserResult();

            for (var i = startIndex; i < limit; i++)
            {
                try
                {
                    var post = new HytalePost();
                    post.ImgUrl = posts[i].QuerySelector(".post__image__frame").FirstChild
                        .GetAttributeValue("src", "src");
                    post.Titel = posts[i].QuerySelector(".post__details__heading").InnerText.Trim();
                    post.Details = posts[i].QuerySelector(".post__details__body").InnerText.Trim();
                    post.Date = posts[i].QuerySelector(".post__details__meta__date").InnerText.Trim();
                    post.Author = posts[i].QuerySelector(".post__details__meta__author").InnerText.Trim();
                    post.PostUrl = $"{baseUrl}{posts[i].FirstChild.GetAttributeValue("href", "href")}";

                    pr.Posts.Add(post);
                }
                catch (Exception e)
                {
                    
                    Console.WriteLine($"Hytale Parser: {e}");
                }
                
            }

            return pr;
        }
    }

    public class HytalePost
    {
        public string ImgUrl;
        public string Titel;
        public string Details;
        public string Date;
        public string Author;
        public string PostUrl;
    }

    public class HytaleParserResult : ParserResult
    {
        public List<HytalePost> Posts;

        public HytaleParserResult()
        {
            Parser = "Hytale";
            ParsedTime = DateTime.Now;
            Posts = new List<HytalePost>();
        }
    }
}