using System;
using System.Drawing;
using System.IO;
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
    public class FileCommands : BaseCommandModule
    {
        [CheckForPermissions(SudoPermission.Any, GuildPermission.Any)]
        [Command("upload")]
        public async Task UploadImage(CommandContext ctx)
        {
            try
            {
                var at = ctx.Message.Attachments[0];
                var uri = new Uri(at.Url);
                
                var uriWithoutQuery = uri.GetLeftPart(UriPartial.Path);
                var fileExtension = Path.GetExtension(uriWithoutQuery);
                
                var path = Path.Combine("/drive/jonas/files", $"{ctx.Message.Id.ToString()}{fileExtension}");

                var httpClient = new HttpClient();

                var img = await httpClient.GetByteArrayAsync(uri);

                await File.WriteAllBytesAsync(path, img);

                await ctx.Channel.SendMessageAsync($"https://files.jmp.blue/images/{ctx.Message.Id.ToString()}{fileExtension}");
            }
            catch
            {
                await ctx.Channel.SendMessageAsync("Fehler!, Eine Bild muss mitgesendet werden");
            }
        }
    }
}