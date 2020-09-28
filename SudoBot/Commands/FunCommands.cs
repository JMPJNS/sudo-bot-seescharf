using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using SudoBot.Attributes;
using SudoBot.Handlers;

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

        [Command("subnet"), Description("Subnetmask und Broadcast Adresse zu einer IP Adresse und Prefix länge")]
        public async Task Subnet(CommandContext ctx, string ip, int prefixLength)
        {
            SubnetCalculator.DoTheThing(ip, prefixLength, ctx);
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
        
        [Command("ggt"), Description("GGT von 2 Zahlen Finden"), Cooldown(5, 10, CooldownBucketType.User)]
        [CheckForPermissions(SudoPermission.Any, GuildPermission.Any)]
        public async Task Ggt(CommandContext ctx, [Description("Erste Zahl")]int uno, [Description("Zweite Zahl")]int due)
        {
            var calc = new GgtKgvCalculator(uno, due);
            await ctx.Channel.SendMessageAsync($"GGT: {calc.GetGgt()}");
        }
        
        [Command("kgv"), Description("KGV von 2 Zahlen Finden"), Cooldown(5, 10, CooldownBucketType.User)]
        [CheckForPermissions(SudoPermission.Any, GuildPermission.Any)]
        public async Task Kgv(CommandContext ctx, [Description("Erste Zahl")]int uno, [Description("Zweite Zahl")]int due)
        {
            var calc = new GgtKgvCalculator(uno, due);
            await ctx.Channel.SendMessageAsync($"KGV: {calc.GetKgv()}");
        }
    }
    
    public class GgtKgvCalculator
    {
        private readonly int _num1;
        private readonly int _num2;
        
        
        public GgtKgvCalculator(int num1, int num2)
        {
            _num1 = num1;
            _num2 = num2;
        }

        public int GetGgt()
        {
            int num = _num1;
            int rest = _num2;

            // Divide by rest until ggt is found
            while (rest != 0)
            {
                int t = rest;
                rest = num % rest;
                num = t;
            }
            
            return num;
        }

        public int GetKgv()
        {
            return (_num1 * _num2) / GetGgt();
        }
    }
}