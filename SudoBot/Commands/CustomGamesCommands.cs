using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using SudoBot.Attributes;
using SudoBot.Database;
using SudoBot.Models;
using SudoBot.Handlers;

namespace SudoBot.Commands
{
    [Group("customs")]
    [Description("Custom Games Commands")]
    public class CustomGamesCommands: BaseCommandModule
    {
        [CheckForPermissions(SudoPermission.Admin, GuildPermission.Any)]
        [Description("Setze die Custom Games Rolle")]
        [Command("set")]
        public async Task SetCustomsRole(CommandContext ctx, DiscordRole role)
        {
            var guild = await Guild.GetGuild(ctx.Guild.Id);
            await guild.SetCustomsRole(role.Id);
            await ctx.Channel.SendMessageAsync("Die Rolle wurde gesetzt");
        }

        [CheckForPermissions(SudoPermission.Mod, GuildPermission.Any)]
        [Cooldown(1, 240, CooldownBucketType.Guild)]
        [Description("Entferne alle aus der Custom Games Rolle")]
        [Command("end")]
        public async Task RemoveAllCustomsRole(CommandContext ctx)
        {
            var guild = await Guild.GetGuild(ctx.Guild.Id);
            await ctx.Channel.SendMessageAsync("Die Rolle wird von allen entfernt. Wird ein paar Sekunden dauern.");
            await guild.RemoveAllCustomsRole(ctx);
            await ctx.Channel.SendMessageAsync("Die Rolle wurde von allen entfernt!");
        }
        
        [CheckForPermissions(SudoPermission.Mod, GuildPermission.Any)]
        [Description("Erstelle ein Custom Game")]
        [Command("create")]
        public async Task CreateCustoms(CommandContext ctx, string title, string message, DiscordEmoji tempJoinEmoji)
        {
            var guild = await Guild.GetGuild(ctx.Guild.Id);

            if (guild.CustomsRole == 0)
            {
                await ctx.Channel.SendMessageAsync("Definiere eine Custom Games Rolle mit: $custom set {@Rolle}");
                return;
            }

            DiscordEmoji joinEmoji;
            try
            {
                joinEmoji = DiscordEmoji.FromName(ctx.Client, $":{tempJoinEmoji.Name}:");
            }
            catch (Exception e)
            {
                joinEmoji = DiscordEmoji.FromName(ctx.Client, ":white_check_mark:");
            }

            var embed = new DiscordEmbedBuilder()
                .WithTitle(title)
                .WithColor(DiscordColor.Blue)
                .WithDescription(message);

            embed.AddField("Reagiere auf diese Nachricht!", $"mit {joinEmoji}");

            var sentMessage = await ctx.Channel.SendMessageAsync(embed:embed.Build());
            
            await guild.SetCustoms(sentMessage.Id, ctx.Channel.Id, joinEmoji.Id);

            await sentMessage.CreateReactionAsync(joinEmoji);
            await ctx.Channel.SendMessageAsync("Starten mit `$customs start {anzahl}`");
        }
        
        [CheckForPermissions(SudoPermission.Mod, GuildPermission.Any)]
        [Description("Starte das Custom Game")]
        [Command("start")]
        public async Task GiveCustoms(CommandContext ctx, int maxMembers)
        {
            var guild = await Guild.GetGuild(ctx.Guild.Id);
            
            if (guild.CustomsRole == 0)
            {
                await ctx.Channel.SendMessageAsync("Definiere eine Custom Games Rolle mit: $custom set {@Rolle}");
                return;
            }
            if (guild.CustomsMessage == 0)
            {
                await ctx.Channel.SendMessageAsync("Derzeit kein laufendes Custom Game, Starte eins mit mit: `$customs create {Titel} {Nachricht} {Emoji}`");
                return;
            }
            var role = ctx.Guild.GetRole(guild.CustomsRole);
            var channel = ctx.Guild.GetChannel(guild.CustomsChannel);
            var message = await channel.GetMessageAsync(guild.CustomsMessage);

            DiscordEmoji emoji;
            try
            {
                emoji = await ctx.Guild.GetEmojiAsync(guild.CustomsEmoji);
            }
            catch
            {
                if (guild.CustomsEmoji != 0)
                {
                    await ctx.Channel.SendMessageAsync($"Es ist ein Fehler Aufgetreten, Emoji {guild.CustomsEmoji} exestiert nicht. Kontaktiere JMP#7777");
                    return;
                }
                emoji = DiscordEmoji.FromName(ctx.Client, ":white_check_mark:");
            }
            
            var joinedUsers = await message.GetReactionsAsync(emoji);
            
            IList<DiscordUser> sUsers = new List<DiscordUser>();


            foreach (var u in joinedUsers)
            {
                if (!u.IsBot)
                {
                    sUsers.Add(u);
                }
            }
            
            var rUsers = sUsers.ToList().Shuffle();

            string users = "";

            if (rUsers.Count < maxMembers) maxMembers = rUsers.Count;

            for (int i = 0; i < maxMembers; i++)
            {
                try
                {
                    var m = await message.Channel.Guild.GetMemberAsync(rUsers[i].Id);
                    users += $"{m.Mention}, ";
                    await m.GrantRoleAsync(role);
                }
                catch (Exception e)
                {
                    await ctx.Channel.SendMessageAsync($"Es ist ein Fehler aufgetreten bei Member {rUsers[i].Mention}");
                    if (e.Message == "Unauthorized: 403")
                    {
                        await ctx.Channel.SendMessageAsync($"Der Bot Hat keine Berechtigung die Rolle {role.Mention} zu Vergeben!");
                        return;
                    }
                }
                
            }

            if (users.Length > 2) users = users.Substring(0, users.Length - 2);
            if (maxMembers > 1)
            {
                users += " haben die Rolle Gekriegt!";
            } else if (maxMembers == 1)
            {
                users += " hat die Rolle Gekriegt!"; 
            }
            else
            {
                users += "Keiner hat die Rolle Gekriegt!"; 
            }

            await ctx.Channel.SendMessageAsync(users);
            }
    }
}