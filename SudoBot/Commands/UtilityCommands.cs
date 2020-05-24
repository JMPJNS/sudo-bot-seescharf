using System;
using System.Globalization;
using System.Security;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using SudoBot.Attributes;
using SudoBot.Models;
using SudoBot.Handlers;

namespace SudoBot.Commands
{
    [Group("utility"), Aliases("u")]
    public class UtilityCommands : BaseCommandModule
    {
        [CheckForPermissions(SudoPermission.Mod, GuildPermission.Any)]
        [Description("Eine Nachricht als Bot User in einem Speziellen Channel Senden")]
        [Command("say")]
        public async Task SayInChannel(CommandContext ctx, [Description("Der Channel")]DiscordChannel channel, [Description("Die Nachricht")]params string[] words)
        {
            var sentMessage = await (ctx.Channel.SendMessageAsync("hört auf zu trollen"));
            
            await channel.SendMessageAsync(string.Join(" ",words));

            await Task.Delay(1000);
            await ctx.Message.DeleteAsync();
            await Task.Delay(1000);
            await sentMessage.DeleteAsync();

        }

        [CheckForPermissions(SudoPermission.Mod, GuildPermission.Any)]
        [Description("Ein Emoji vergrößern")]
        [Command("make-big"), Aliases("mb")]
        public async Task MakeBig(CommandContext ctx, [Description("Das Emoji zu vergrößern")]DiscordEmoji e)
        {
            var embed = new DiscordEmbedBuilder()
                .WithImageUrl(e.Url);
            await ctx.Channel.SendMessageAsync(embed: embed.Build());
        }
        
        [Command("ping")]
        [Description("Der Ping vom Bot zur Discord API")]
        public async Task Ping(CommandContext ctx)
        {
            var message = await ctx.Channel.SendMessageAsync($"{ctx.Client.Ping.ToString()}ms");
        }

        // Reminder Stuff

        [Command("reminder")]
        [Description("Eine Erinnerung zu einem Bestimmten Zeitpunkt Erstellen erstellen")]
        public async Task Reminder(CommandContext ctx, [Description("um")] string format , [Description("Zeitpunkt {beispiel: 12:00}")] DateTime time, [Description("Nachricht")] string message)
        {
            await this.ReminderCommand(ctx, format, time.ToUniversalTime(), message);
        }
        
        [Command("reminder")]
        [Description("Eine Erinnerung in x (s/m/h/d) erstellen")]
        public async Task Reminder(CommandContext ctx, [Description("in")] string format, [Description("Zeitspanne {beispiel: 12m}")] TimeSpan timespan, [Description("Nachricht")] string message)
        {
            await this.ReminderCommand(ctx, format, DateTime.UtcNow + timespan, message);
        }

        private async Task ReminderCommand(CommandContext ctx, string format, DateTime time, string message)
        {
            var tz = TimeZoneInfo.FindSystemTimeZoneById("Europe/Vienna");
            time = TimeZoneInfo.ConvertTimeFromUtc(time, tz);
            var thenTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz);
            var ts = time - thenTime;

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
        
        // Info Commands
        [Command("member-info"), Aliases("userInfo")]
        [Description("Information über einen Member")]
        public async Task MemberInfo(CommandContext ctx, [Description("Der Member (optional)")]DiscordMember member = null)
        {
            if (member == null) member = ctx.Member;
            string roles = "";
            foreach (var role in member.Roles)
            {
                roles += $"{role.Mention}, ";
            }

            if (roles.Length > 2)
            {
                roles = roles.Substring(0, roles.Length - 2);   
            }

            var embed = new DiscordEmbedBuilder()
                .WithDescription(member.Mention)
                .WithColor(member.Color)
                .AddField("Nickname", member.Nickname ?? member.Username, true)
                .AddField("Username", member.Username, true)
                .AddField("#", member.Discriminator, true)
                .AddField("ID", member.Id.ToString(), true)
                .AddField("Beigetreten", member.JoinedAt.ToString())
                .AddField("Rollen", roles.Length != 0 ? roles : "Keine")
                .WithThumbnailUrl(member.AvatarUrl);

            await ctx.Channel.SendMessageAsync(embed: embed.Build());
        }
    }
}