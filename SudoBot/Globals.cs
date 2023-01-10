using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using SudoBot.Models;
using SudoBot.Parser;

namespace SudoBot
{
    public class Globals
    {
        public static DiscordClient Client;
        public static ulong MyId = 272809112851578881;
        public static string CdnKey = Environment.GetEnvironmentVariable("CDN_API_KEY") ?? "SuperSecret";
        public static readonly List<Guild> GuildCache = new();
        
        public static bool isProduction = Environment.GetEnvironmentVariable("PROD") == "true";

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
                RequestUri = new Uri(Environment.GetEnvironmentVariable("CDN_URL") ?? "https://cdn.jmp.blue"),
                Method = HttpMethod.Post,
                Content = formData
            };
            request.Headers.Add("x-api-key", CdnKey);

            var response = await client.SendAsync(request);
            var resUrl = await response.Content.ReadAsStringAsync();
            return resUrl;
        }

        public static async Task<JObject> HttpJsonRequest(string url)
        {
            var client = new HttpClient();
            var req = await client.GetAsync(url);
            if (req.StatusCode != HttpStatusCode.OK)
            {
                return null;
            }

            var res = await req.Content.ReadAsStringAsync();
            return JObject.Parse(res);
        }
        
        private static Random random = new Random();
        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
        
        public static async Task<string> HttpRequest(string url)
        {
            var client = new HttpClient();
            var req = await client.GetAsync(url);
            if (req.StatusCode != HttpStatusCode.OK)
            {
                return null;
            }

            return await req.Content.ReadAsStringAsync();
        }
        
        public static async Task<string> RunCommand(string cmd, int waitTime = 200)
        {
            var escapedArgs = cmd.Replace("\"", "\\\"");
            
            Console.Write($"Executing Command: /bin/sh -c \"{escapedArgs}\"");

            bool isWindows = Environment.OSVersion.Platform == PlatformID.Win32NT;

            var process = new Process()
            {
                StartInfo = new ProcessStartInfo()
                {
                    FileName = isWindows ? "powershell.exe" : "/bin/bash",
                    Arguments = isWindows ? $"\"{escapedArgs}\"" : $"-c \"{escapedArgs}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = false
                }
            };
            process.Start();
            int tries = 0;
            string outputError = "";
            string output = "";
            while (!process.HasExited)
            {
                // Console.WriteLine($"Delaying, Command: {cmd}");
                
                var currentError = await process.StandardError.ReadToEndAsync();
                var current = await process.StandardOutput.ReadToEndAsync();
                
                output += current;
                outputError += currentError;
                
                // Console.WriteLine($"Output: {current}, Error: {currentError}");
                await Task.Delay(waitTime);
                tries += 1;
                if (tries > 60)
                {
                    return "command ran into timeout after 60 seconds";
                }
            }
            
            
            
            output += await process.StandardOutput.ReadToEndAsync();
            outputError += await process.StandardError.ReadToEndAsync();

            if (output == "" && outputError != "")
            {
                return outputError;
            }
            return output;
        }
        
        public static DiscordChannel LogChannel
        {
            get
            {
                var guild = Client.GetGuildAsync(1062385168406560789).GetAwaiter().GetResult();
                if (isProduction)
                {
                    return guild.GetChannel(1062386968333385778);
                }
                else
                {
                    return guild.GetChannel(1062387017226387518);
                }
            }
        }

        public static DiscordUser BotUser
        {
            get
            {
                if (Environment.GetEnvironmentVariable("DBNAME") == "SudoBot")
                {
                    return Client.GetUserAsync(705548602994458684).GetAwaiter().GetResult();;
                }
                else
                {
                    return Client.GetUserAsync(705549474575024240).GetAwaiter().GetResult();;
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