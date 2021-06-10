using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mime;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using SudoBot.Attributes;

namespace SudoBot.Commands
{
    [Group("file"), Hidden()]
    public class FileCommands : BaseCommandModule
    {
        [CheckForPermissions(SudoPermission.Me, GuildPermission.Any)]
        [Command("upload")]
        public async Task UploadFile(CommandContext ctx)
        {
            try
            {
                var at = ctx.Message.Attachments[0];
                var uri = new Uri(at.Url);
                
                var uriWithoutQuery = uri.GetLeftPart(UriPartial.Path);
                var fileExtension = Path.GetExtension(uriWithoutQuery);
                var mimeType = MimeTypes.GetMimeType($"test{fileExtension}");
                var httpClient = new HttpClient();
                var img = await httpClient.GetByteArrayAsync(uri);
                var imgStream = new MemoryStream(img);

                var fileUrl = await Globals.UploadToCdn($"{ctx.Message.Id.ToString()}{fileExtension}", mimeType, imgStream);
                await ctx.Channel.SendMessageAsync("Url: " + fileUrl);
            }
            catch (Exception)
            {
                await ctx.Channel.SendMessageAsync("Fehler!, Eine Bild muss mitgesendet werden");
            }
        }

        [CheckForPermissions(SudoPermission.Me, GuildPermission.Any), Description("Download Youtube Video"),
         Command("youtube-dl")]
        public async Task YoutubeDl(CommandContext ctx, String url, Boolean audioOnly = false, int start = 0, int length = 0)
        {
            var extension = audioOnly ? ".m4a" : ".mp4";
            
            var fileName = ctx.Message.Id.ToString() + extension;
            var filePath = Path.GetTempPath() + fileName;
            var format = audioOnly ? "bestaudio[ext=m4a]" : "mp4/bestvideo+bestaudio";

            String cmd;
            
            if (start != 0 || length != 0)
            {
                cmd =
                    $"ffmpeg -y -ss {start} -t {length} -i $(youtube-dl -f '{format}' --get-url {url}) {filePath}";
            }
            else
            {
                cmd = $"ffmpeg -i $(youtube-dl -f '{format}' --get-url {url}) {filePath}";
            }
            
            var res = await Globals.RunCommand(cmd);
                
            var file = File.Open(filePath, FileMode.Open);
            var mimeType = MimeTypes.GetMimeType($"test{extension}");
            var resUrl = await Globals.UploadToCdn(fileName, mimeType, file);
            file.Close();
            File.Delete(filePath);

            await ctx.RespondAsync("Url: " + resUrl);
        }
        
        // [CheckForPermissions(SudoPermission.Me, GuildPermission.Any)]
        // [Command("upload")]
        // public async Task UploadText(CommandContext ctx, params string[] args)
        // {
        //     try
        //     {
        //         var path = Path.Combine("/drive/jonas/files", $"{ctx.Message.Id.ToString()}.txt");
        //
        //         args.Join()
        //
        //         await File.WriteAllBytesAsync(path, img);
        //
        //         await ctx.Channel.SendMessageAsync($"https://files.jmp.blue/images/{ctx.Message.Id.ToString()}{fileExtension}");
        //     }
        //     catch
        //     {
        //         await ctx.Channel.SendMessageAsync("Fehler!, Eine Bild muss mitgesendet werden");
        //     }
        // }
        
        
    }
}