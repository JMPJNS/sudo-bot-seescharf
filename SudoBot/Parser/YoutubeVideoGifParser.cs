using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;

namespace SudoBot.Parser
{
    public class YoutubeVideoGifParser
    {
        public Dictionary<int, string> Urls = new Dictionary<int, string>();
        public bool Active = true;
        public ulong InitiatedBy;
            
        // make is active bool and stop function to stop from outside
        public async Task ParseVideo(CommandContext ctx, string video = null)
        {
            InitiatedBy = ctx.User.Id;
            video ??= "https://www.youtube.com/watch?v=07d2dXHYb94";
            var res = await RunCommand($"youtube-dl --get-duration {video}");

            await ctx.RespondAsync($"length: {res}");

            var splitLength = 15;

            var times = res.Replace("\n", "").Trim().Split(":").Reverse().ToArray();
            int seconds = 0;
            
            for (int i = 0; i < times.Length; i++)
            {
                var t = Int32.Parse(times[i]);
                var m = Enumerable.Repeat(60, i)
                    .Aggregate(1, (a, b) => a * b);

                seconds += t * m;
            }

            var splitCount = seconds / splitLength;
            Console.Write($"video length: {res}");
            
            var dl = DownloadSplits(video, splitLength, splitCount, ctx.User.Id.ToString());
            var show = ShowSplits(ctx, video, splitLength, splitCount);
            await Task.WhenAll(dl, show);
        }

        private async Task ShowSplits(CommandContext ctx, string video, int splitLength, int splitCount)
        {
            
            var message = await ctx.Channel.SendMessageAsync($"Sending Video {splitCount * splitLength} seconds, split 0 / {splitCount}");
            var path = GetPathName();
            
            for (var curr = 0; curr < splitCount; curr++)
            {
                if (!Active)
                {
                    await ctx.Channel.SendMessageAsync("Some error occured or execution was stopped");
                    return;
                }
                
                Console.WriteLine(
                    $"Displaying {curr * splitLength} - {curr * splitLength + splitLength} (total {splitCount * splitLength}) as segment {curr}");
                var filePath = path + $"output{curr}.gif";
                DiscordMessage currentMessage = null;
                

                var checkIterations = 0;
                
                if (!Urls.ContainsKey(curr))
                {
                    currentMessage = await ctx.Channel.SendMessageAsync($"waiting for segment {curr}");
                }

                while (!Urls.ContainsKey(curr))
                {
                    if (!Active)
                    {
                        await ctx.Channel.SendMessageAsync("Some error occured or execution was stopped");
                        return;
                    }
                    Console.WriteLine($"Waiting for {curr}");
                    await Task.Delay(200);
                    checkIterations += 1;

                    if (checkIterations > 100) Active = false;
                }

                var url = Urls[curr];

                if (currentMessage != null)
                {
                    await currentMessage.DeleteAsync();
                }
                
                await message.ModifyAsync($"Sending Video {splitCount * splitLength} seconds, split {curr} / {splitCount} {url}");
                
                await Task.Delay(splitLength * 1000);
            }
        }
        
        private async Task DownloadSplits(string url, int splitLength, int splitCount, string baseName = "output")
        {
            var path = GetPathName();

            for (var curr = 0; curr < splitCount; curr++)
            {
                if (!Active)
                {
                    Console.WriteLine("Some error occured or execution was stopped");
                    return;
                }
                
                Console.WriteLine($"Downloading Seconds {curr*splitLength} - {curr*splitLength+splitLength} (total {splitCount*splitLength}) as segment {curr}");
                var filePath = path+$"{baseName}{curr}.gif";
            
                var cmd =
                    $"ffmpeg -y -ss {curr*splitLength} -t {splitLength} -i $(youtube-dl -f 18 --get-url {url}) ";
                cmd +=
                    $"-vf \"fps=10,scale=320:-1:flags=lanczos,split[s0][s1];[s0]palettegen[p];[s1][p]paletteuse\" -loop 0 {filePath}";

                var res = await RunCommand(cmd);
                
                var file = File.Open(filePath, FileMode.Open);

                var resUrl = await Globals.UploadToCdn($"{baseName}{curr}.gif", "image/gif", file);
                
                file.Close();
                File.Delete(filePath);
                Urls[curr] = resUrl;
            }
            Console.WriteLine("done");
        }

        private string GetPathName()
        {
            var tempPath = Path.GetTempPath();
            bool isWindows = Environment.OSVersion.Platform == PlatformID.Win32NT;
            var path = tempPath + (isWindows ? "gifStuff\\" : "gifStuff/");

            if (Directory.Exists(path))
            {
                return path;
            }

            Directory.CreateDirectory(path);
            return path;
        }
        
        private async Task<string> RunCommand(string cmd, int waitTime = 1000)
        {
            var escapedArgs = cmd.Replace("\"", "\\\"");

            bool isWindows = Environment.OSVersion.Platform == PlatformID.Win32NT;

            var process = new Process()
            {
                StartInfo = new ProcessStartInfo()
                {
                    FileName = isWindows ? "powershell.exe" : "/bin/sh",
                    Arguments = isWindows ? $"\"{escapedArgs}\"" : $"-c \"{escapedArgs}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = false
                }
            };
            process.Start();
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
            }
            
            
            
            output += await process.StandardOutput.ReadToEndAsync();
            outputError += await process.StandardError.ReadToEndAsync();

            if (output == "" && outputError != "")
            {
                return outputError;
            }
            return output;
        }
    }
}