using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using SudoBot.Attributes;

namespace SudoBot.Commands
{
    [Group("fun"), Aliases("f")]
    [Description("Commands für Random Fun Stuff")]
    public class FunCommands : BaseCommandModule
    {
        [Command("add"), Description("Addiert 2 Zahlen"), Cooldown(5, 10, CooldownBucketType.User)]
        [CheckForPermissions(SudoPermission.Any, GuildPermission.Any)]
        public async Task Add(CommandContext ctx, int uno, int due)
        {
            var message = await ctx.Channel.SendMessageAsync(uno.ToString() + "+" + due.ToString() + "=" + (uno+due).ToString());
        }
        
        [Command("subtract"), Description("Subtrahiert 2 Zahlen"), Cooldown(5, 10, CooldownBucketType.User)]
        [CheckForPermissions(SudoPermission.Any, GuildPermission.Any)]
        public async Task Subtract(CommandContext ctx, int uno, int due)
        {
            var message = await ctx.Channel.SendMessageAsync(uno.ToString() + "-" + due.ToString() + "=" + (uno-due).ToString());
        }

        [Command("number"), Cooldown(3, 30, CooldownBucketType.Channel)]
        [CheckForPermissions(SudoPermission.Any, GuildPermission.Any)]
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
        
        [Command("random"), Description("Random zahl"), Cooldown(5, 10, CooldownBucketType.User)]
        [CheckForPermissions(SudoPermission.Any, GuildPermission.Any)]
        public async Task R(CommandContext ctx, int minimum, int maximum)
        {
            Random rnd = new Random();
            int random = rnd.Next(minimum, maximum+1);
            await ctx.RespondAsync(random.ToString());
        }
        
        [Command("multiply"), Description("Multipliziert 2 Zahlen"), Cooldown(5, 10, CooldownBucketType.User)]
        [CheckForPermissions(SudoPermission.Any, GuildPermission.Any)]
        public async Task Multiply(CommandContext ctx, int uno, int due)
        {
            var message = await ctx.Channel.SendMessageAsync(uno.ToString() + "*" + due.ToString() + "=" + (uno*due).ToString());
        }
        
        [Command("divide"), Description("Dividiert 2 Zahlen"), Cooldown(5, 10, CooldownBucketType.User)]
        [CheckForPermissions(SudoPermission.Any, GuildPermission.Any)]
        public async Task Divide(CommandContext ctx, int uno, int due)
        {
            var message = await ctx.Channel.SendMessageAsync(uno.ToString() + "/" + due.ToString() + "=" + ((float)uno/(float)due).ToString());
        }
    }
}