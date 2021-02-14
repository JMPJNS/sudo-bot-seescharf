using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using SudoBot.Attributes;
using SudoBot.Handlers;
using SudoBot.Models;
using SudoBot.Specifics;

namespace SudoBot.Commands
{
    [Group("fun"), Aliases("f")]
    [Description("Commands für Random Fun Stuff")]
    public class FunCommands : BaseCommandModule
    {
        [Command("crabrave"), Description("Add Text to Crab Rave")]
        [CheckForPermissions(SudoPermission.Me, GuildPermission.Any)]
        public async Task CrabRave(CommandContext ctx, [RemainingText] string text)
        {
            var path = GetPathName();

            var cmd = $"ffmpeg -i /drive/jonas/files/crab.mkv -vf \"drawtext=text='{text}': fontsize=36: fontcolor=white: shadowcolor=black: shadowx=2: shadowy=2: x=(w-text_w)/2: y=(h-text_h)/2\" -y {path}output.mp4";
            
            var res = await Globals.RunCommand(cmd);
            await ctx.RespondWithFileAsync($"{path}output.mp4");
            
            string GetPathName()
            {
                var tempPath = Path.GetTempPath();
                bool isWindows = Environment.OSVersion.Platform == PlatformID.Win32NT;
                var path = tempPath + (isWindows ? "crabRave\\" : "crabRave/");

                if (Directory.Exists(path))
                {
                    return path;
                }

                Directory.CreateDirectory(path);
                return path;
            }
        }

        [Command, Aliases("schmuser"), Description("Get a Random AI Generated Cat Image")]
        public async Task Cat(CommandContext ctx)
        {
            await ctx.RespondAsync($"https://thiscatdoesnotexist.com/?{Globals.RandomString(5)}");
        }
        
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
        public async Task Subnet(CommandContext ctx, [Description("Die IP Adresse (Format Beispiel: 192.168.8.30/24)")] string ip, [Description("Die Anzahl der Subnets")] int subnetCount)
        {
            SubnetCalculator.DoTheThing(ip, subnetCount, ctx);
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

        [Command("hunger-games"), Description("Starte eine Hunger games Session")]
        [CheckForPermissions(SudoPermission.Mod, GuildPermission.HungerGames)]
        public async Task HungerGames(CommandContext ctx, [Description("Wie lange bis zum start")] int minutes = 5, 
            [Description("Wie viele Leute maximal mitspielen dürfen")] int maxPlayers = 512, 
            [Description("Ob spieler die zu wenig sind, durch bots ersetzt werden sollen")] Boolean useBots = false)
        {
            var emoji = DiscordEmoji.FromUnicode("🏟");
            
            var em = new DiscordEmbedBuilder()
                .WithTitle("Hunger Games")
                .WithImageUrl("https://media.vanityfair.com/photos/5e9f30a3bca9ee00084f4d4e/master/pass/shutterstock_editorial_5885659aw.jpg")
                .WithColor(DiscordColor.Goldenrod)
                .WithDescription($"Reagiere mit {emoji} zum beitreten");
            
            var m = await ctx.RespondAsync(embed: em.Build());

            await m.CreateReactionAsync(emoji);
            
            var sched = new Scheduled(new List<ScheduledType> {ScheduledType.HungerGames, ScheduledType.Minute}, $"{maxPlayers}:{useBots}", DateTime.Now.AddMinutes(minutes), ctx.Guild.Id, ctx.Channel.Id, ctx.User.Id, m.Id);

            if (minutes < 10)
            {
                await Task.Delay(TimeSpan.FromMinutes(minutes));
                await Scheduled.RunSchedule(ScheduledType.Minute);
            }
        }
    }
    
    public class GgtKgvCalculator
    {
        private readonly int _num1;
        private readonly int _num2;
        
        
        public GgtKgvCalculator(int num1, int num2)
        {
            if (num1 >= 100000 || num2 >= 100000)
            {
                throw new ArgumentOutOfRangeException();
            }
            
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