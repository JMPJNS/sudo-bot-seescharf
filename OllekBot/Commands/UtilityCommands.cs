using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using OllekBot.DataInterfaces;
using OllekBot.Handlers;

namespace OllekBot.Commands
{
    public class UtilityCommands : BaseCommandModule
    {
        [Command("say")]
        [RequireRoles(RoleCheckMode.Any, new []{"Admins", "Mods"})]
        public async Task Ping(CommandContext ctx, params string[] words)
        {
            var message = await ctx.Channel.SendMessageAsync(ctx.RawArgumentString);
        }
        
        [Command("listUsers")]
        public async Task ListUsers(CommandContext ctx)
        {
            foreach (User user in UserHandler.Users)
            {
                DiscordGuild currentGuild = await ctx.Client.GetGuildAsync(user.GuildId);
                if (ctx.Guild == currentGuild)
                {
                    var message = await ctx.Channel.SendMessageAsync($"{await currentGuild.GetMemberAsync(user.UserId)}");
                }
            }
        }

        [Command("reminder")]
        public async Task Reminder(CommandContext ctx, string format, DateTime time, string message)
        {
            await this.ReminderCommand(ctx, format, time, message);
        }
        
        [Command("reminder")]
        public async Task Reminder(CommandContext ctx, string format, TimeSpan time, string message)
        {
            await this.ReminderCommand(ctx, format, DateTime.Now + time, message);
        }

        private async Task ReminderCommand(CommandContext ctx, string format, DateTime time, string message)
        {
            var ts = time - DateTime.Now;
            var thenTime = DateTime.Now;
            
            if (format == "um" || format == "in")
            {
                await ctx.Channel.SendMessageAsync($"Ich werde dich um {time.ToString("h:mm:ss", DateTimeFormatInfo.InvariantInfo)} erinnern");
                await Task.Delay(ts);
                await ctx.Channel.SendMessageAsync($"{ctx.User.Mention} [{thenTime.ToString("h:mm:ss", DateTimeFormatInfo.InvariantInfo)}], {message}");
            } else
            {
                await ctx.Channel.SendMessageAsync("Falsch Verwendet! $reminder {um/in} {uhrzeit/zeitspanne} {nachricht}");
                return;
            }
        }
    }
}