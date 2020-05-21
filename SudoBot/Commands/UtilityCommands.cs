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
    public class UtilityCommands : BaseCommandModule
    {
        [CheckForPermissions(SudoPermission.Mod, GuildPermission.Any)]
        [Command("say")]
        public async Task SayInChannel(CommandContext ctx, DiscordChannel channel, params string[] words)
        {
            Task.Run(async () =>
            {
                await Task.Delay(1000);
                await ctx.Message.DeleteAsync();
            });
            await channel.SendMessageAsync(ctx.RawArgumentString);
        }
        
        [CheckForPermissions(SudoPermission.Mod, GuildPermission.Any)]
        [Command("say")]
        public async Task Say(CommandContext ctx, params string[] words)
        {
            Task.Run(async () =>
            {
                await Task.Delay(1000);
                await ctx.Message.DeleteAsync();
            });
            await ctx.Channel.SendMessageAsync(ctx.RawArgumentString);
        }

        [Command("invite")]
        public async Task Invite(CommandContext ctx)
        {
            await ctx.Channel.SendMessageAsync(
                "https://discord.com/oauth2/authorize?client_id=705548602994458684&scope=bot&permissions=1544023122");
        }
        
        [CheckForPermissions(SudoPermission.Mod, GuildPermission.Any)]
        [Command("makeBig")]
        public async Task MakeBig(CommandContext ctx, DiscordEmoji e)
        {
            var embed = new DiscordEmbedBuilder()
                .WithImageUrl(e.Url);
            await ctx.Channel.SendMessageAsync(embed: embed.Build());
        }
        
        [Command("ping")]
        public async Task Ping(CommandContext ctx)
        {
            var message = await ctx.Channel.SendMessageAsync($"{ctx.Client.Ping.ToString()}ms");
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
        
        // Info Commands
        [Command("memberInfo")]
        public async Task MemberInfo(CommandContext ctx, DiscordMember member)
        {
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