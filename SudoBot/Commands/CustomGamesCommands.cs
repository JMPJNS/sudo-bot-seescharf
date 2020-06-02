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
        public async Task SetCustomsRole(CommandContext ctx, [Description("Die Custom Games Rolle")]DiscordRole role)
        {
            var guild = await Guild.GetGuild(ctx.Guild.Id);
            await guild.SetCustomsRole(role.Id);
            await ctx.Channel.SendMessageAsync("Die Rolle wurde gesetzt");
        }

        [CheckForPermissions(SudoPermission.Mod, GuildPermission.CustomGames)]
        [Cooldown(1, 240, CooldownBucketType.Guild)]
        [Description("Entferne alle aus der Custom Games Rolle (4 Minuten Cooldown)")]
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
        public async Task CreateCustoms(CommandContext ctx, [Description("Titel der Custom Games")]string title, [Description("Eine Beschreibung dazu")]string message, [Description("Das Emote mit dem Reagiert werden soll")]DiscordEmoji tempJoinEmoji)
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
        [Description("Einem Member Tickets für die Custom Games geben / nehmen")]
        [Command("add-ticket")]
        public async Task AddTicket(CommandContext ctx, DiscordMember member, [Description("Negativ um Tickets zu nehmen")]int count = 1)
        {
            var user = await User.GetOrCreateUser(member);

            await user.AddTickets(count);
            await ctx.Channel.SendMessageAsync($"{member.Mention} hat {count.ToString()} Tickets erhalten!");
        }
        
        [CheckForPermissions(SudoPermission.Mod, GuildPermission.Any)]
        [Description("Die Ticket Anzahl wieder auf den Standard wert setzen (1)")]
        [Command("reset-tickets")]
        public async Task ResetTickets(CommandContext ctx)
        {
            var guild = await Guild.GetGuild(ctx.Guild.Id);

            await guild.ResetTickets();
            await ctx.Channel.SendMessageAsync("Die Tickets wurden zurückgesetzt!");
        }
        
        [CheckForPermissions(SudoPermission.Mod, GuildPermission.Any)]
        [Description("Starte das Custom Game (Lost die Rolle unter den Leuten die Reagiert haben aus)")]
        [Command("start")]
        public async Task GiveCustoms(CommandContext ctx, [Description("Wie viele Leute die Custom Games Rolle erhalten sollen")]int maxMembers, [Description("Ob beitreten ein Ticket verwenden soll.\nTickets können mit `$customs add-ticket [@User] [(optional) Anzahl]` vergeben werden\n mit `$customs reset-tickets` alle tickets auf den Standard wert (1) setzen.")]bool useTickets = false)
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

            await ctx.Channel.SendMessageAsync($"Die Rolle wird unter {maxMembers.ToString()} Leuten Ausgelost!");

            for (int i = 0; i < maxMembers; i++)
            {
                try
                {
                    var m = await message.Channel.Guild.GetMemberAsync(rUsers[i].Id);
                    var user = await User.GetOrCreateUser(m);
                    
                    if (user.TicketsRemaining > 0 || useTickets == false) {
                        if (useTickets) await user.RemoveTicket();
                        
                        users += $"{m.Mention}, ";
                        await m.GrantRoleAsync(role);
                    }
                    else
                    {
                        if (maxMembers < rUsers.Count)
                        {
                            maxMembers++;
                        }
                    }
                    
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
            } else if (maxMembers == 1 && users.Length > 2)
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