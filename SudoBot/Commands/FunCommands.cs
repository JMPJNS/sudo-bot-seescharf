using System.IO;
using System.Net;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

namespace SudoBot.Commands
{
    public class FunCommands : BaseCommandModule
    {
        [Command("add")]
        public async Task Add(CommandContext ctx, int uno, int due)
        {
            var message = await ctx.Channel.SendMessageAsync(uno.ToString() + "+" + due.ToString() + "=" + (uno+due).ToString());
        }
        
        [Command("subtract")]
        public async Task Subtract(CommandContext ctx, int uno, int due)
        {
            var message = await ctx.Channel.SendMessageAsync(uno.ToString() + "-" + due.ToString() + "=" + (uno-due).ToString());
        }

        [Command("number")]
        [Description("Gibt Information über eine Zahl")]
        public async Task Number(CommandContext ctx, int number)
        {
            string url = $"http://numbersapi.com/{number.ToString()}";
            var request = await WebRequest.Create(url).GetResponseAsync();

            using (Stream data = request.GetResponseStream())
            {
                StreamReader reader = new StreamReader(data);
                var ninfo = await reader.ReadToEndAsync();
                var embed = new DiscordEmbedBuilder()
                    .WithTitle(number.ToString())
                    .WithDescription(ninfo);
                await ctx.Channel.SendMessageAsync(embed: embed.Build());
            }
            request.Close();
        }
        
        [Command("multiply")]
        public async Task Multiply(CommandContext ctx, int uno, int due)
        {
            var message = await ctx.Channel.SendMessageAsync(uno.ToString() + "*" + due.ToString() + "=" + (uno*due).ToString());
        }
        
        [Command("divide")]
        public async Task Divide(CommandContext ctx, int uno, int due)
        {
            var message = await ctx.Channel.SendMessageAsync(uno.ToString() + "/" + due.ToString() + "=" + ((float)uno/(float)due).ToString());
        }
    }
}