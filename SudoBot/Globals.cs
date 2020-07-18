using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using SudoBot.Models;
using SudoBot.Parser;

namespace SudoBot
{
    public class Globals
    {
        public static readonly string[] ModRoles = {"SudoAdmin", "SudoMod", "Admins", "Mods", "✨ | Server-Techniker👑", "️✨│Senior Moderatoren", "🔧│Bot Developer✨", "✨│Helferleine", "✨│Moderatoren"};
        public static readonly string[] AdminRoles = {"SudoAdmin", "Admins", "✨ | Server-Techniker👑", "️✨│Senior Moderatoren", "🔧│Bot Developer✨"};
        public static DebugLogger Logger;
        public static DiscordClient Client;
        public static ulong MyId = 272809112851578881;
        public static string CdnKey;
        public static readonly List<Guild> GuildCache = new List<Guild>();

        public static List<YoutubeVideoGifParser> YoutubeVideoGifParsers = new List<YoutubeVideoGifParser>();

        public static async Task<string> UploadToCdn(string filename, string contentType, Stream file)
        {
            HttpContent content = new StreamContent(file);
            content.Headers.Add("Content-Type", contentType);
            var client = new HttpClient();
            var formData = new MultipartFormDataContent();
            formData.Add(content, filename, filename);
            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri("https://cdn.jmp.blue"),
                Method = HttpMethod.Post,
                Content = formData
            };
            request.Headers.Add("x-api-key", CdnKey);

            var response = await client.SendAsync(request);
            var resUrl = await response.Content.ReadAsStringAsync();
            return resUrl;
        }
        
        public static DiscordChannel LogChannel
        {
            get
            {
                if (Environment.GetEnvironmentVariable("DBNAME") == "SudoBot")
                {
                    var guild = Client.GetGuildAsync(468835109844418575).GetAwaiter().GetResult();
                    return guild.GetChannel(712253992243036281);
                }
                else
                {
                    var guild = Client.GetGuildAsync(468835109844418575).GetAwaiter().GetResult();
                    return guild.GetChannel(712254249148350495);
                }
            }
        }
    }
}

public static class IListExtensions
{
    public static IList<T> Shuffle<T>(this IList<T> list)
    {
        var rng = new Random();
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }

        return list;
    }
}