using System;
using System.Globalization;
using System.Security;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using SudoBot.DataInterfaces;
using SudoBot.Handlers;

namespace SudoBot.Commands
{
    public class UtilityCommands : BaseCommandModule
    {
        [Command("say")]
        [RequirePermissions(Permissions.ManageMessages)]
        public async Task Say(CommandContext ctx, params string[] words)
        {
            Task.Run(async () =>
            {
                await Task.Delay(1000);
                await ctx.Message.DeleteAsync();
            });
            await ctx.Channel.SendMessageAsync(ctx.RawArgumentString);
        }

        // Reminder Stuff

        [Command("reminder")]
        [Description("Eine Erinnerung erstellen")]
        public async Task Reminder(CommandContext ctx, [Description("um")] string format , [Description("Zeitpunkt {beispiel: 12:00}")] DateTime time, [Description("Nachricht")] string message)
        {
            await this.ReminderCommand(ctx, format, time, message);
        }
        
        [Command("reminder")]
        [Description("Eine Erinnerung erstellen")]
        public async Task Reminder(CommandContext ctx, [Description("in")] string format, [Description("Zeitspanne {beispiel: 12m}")] TimeSpan timespan, [Description("Nachricht")] string message)
        {
            await this.ReminderCommand(ctx, format, DateTime.Now + timespan, message);
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