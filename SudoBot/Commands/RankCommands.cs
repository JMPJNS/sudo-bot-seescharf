using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using SudoBot.Attributes;
using SudoBot.Database;
using SudoBot.Models;
using SudoBot.Handlers;

namespace SudoBot.Commands
{
    [Group("rank"), Aliases("r", "ranking")]
    [Description("Leveling System")]
    public class RankCommands: BaseCommandModule
    {
        
        [CheckForPermissions(SudoPermission.Any, GuildPermission.Any)]
        [Description("Information über den Aktuellen Rang (30s Cooldown)")]
        [GroupCommand]
        public async Task Rank(CommandContext ctx, [Description("Anderer User (optional)")]DiscordMember member = null)
        {
            if (member == null) member = ctx.Member;
            var user = await User.GetOrCreateUser(member);
            var guild = await Guild.GetGuild(user.GuildId);

            if (guild.CommandChannel != 0 && guild.CommandChannel != ctx.Channel.Id)
            {
                var channel = ctx.Guild.GetChannel(guild.CommandChannel);
                var sentMessage = await ctx.Channel.SendMessageAsync($"In diesem Channel nicht erlaubt, bitte in {channel.Mention} verwenden!");
                Task.Run(() =>
                {
                    Task.Delay(5000).GetAwaiter().GetResult();
                    sentMessage.DeleteAsync();
                    ctx.Message.DeleteAsync();
                });
                return;
            }

            // if (guild.LocalLogChannel == 0)
            // {
            //     await ctx.Channel.SendMessageAsync("Bitte setze einen Log Channel für fehler, $a set-log-channel #channel");
            // }

            await user.UpdateRankRoles();
            var rank = await user.GetRank();
            var currNext = await user.GetCurrentAndNextRole();

            var embed = new DiscordEmbedBuilder()
                .WithColor(member.Color)
                .WithThumbnail(member.AvatarUrl)
                .WithTitle(member.Nickname ?? member.Username)
                .AddField("Rank", $"#{rank.ToString()}", true)
                .AddField(guild.RankingPointName ?? "XP", user.CalculatePoints().ToString(), true)
                .AddField($"Bonus {guild.RankingPointName ?? "XP"}", user.SpecialPoints.ToString(), true);

            if (currNext.Current != null)
            {
                embed.AddField("Aktuell", $"{currNext.Current.Mention}", true);
            }
            if (currNext.Remaining != 0)
            {
                embed.AddField("Verbleibend", $"{currNext.Remaining.ToString()} {guild.RankingPointName ?? "XP"}", true);
            }
            if (currNext.Next != null)
            {
                embed.AddField("Als Nächstes", $"{currNext.Next.Mention}", true);
            }

            embed.AddField("Beigetreten", user.JoinDate.ToString("dd.MM.yyyy H:mm"))
                .AddField(guild.RankingTimeMultiplier == 0
                        ? $"```{guild.RankingPointName} kriegt man durch Nachrichten schreiben!```"
                        : $"```{guild.RankingPointName} kriegt man durch Nachrichten schreiben!\nAußerdem erhälst du jeden Tag {guild.RankingTimeMultiplier.ToString()} {guild.RankingPointName}, rückwirkend seit du dem Discord Beigetreten bist!```",
                    "`$r list` um alle Ränge anzuzeigen.");
            
            await ctx.Channel.SendMessageAsync(embed:embed.Build());
        }

        [Command("leaderboard")]
        [CheckForPermissions(SudoPermission.Any, GuildPermission.Any)]
        [Description("Das Globale Leaderboard anzeigen.")]
        public async Task Leaderboard(CommandContext ctx)
        {
            var skip = 0;
            var user = await User.GetOrCreateUser(ctx.Member);

            var lb = await Mongo.Instance.GetLeaderboard(skip, ctx.Guild.Id);
            await SendLeaderboard(ctx, lb, user);
        }
        [Command("leaderboard")]
        [CheckForPermissions(SudoPermission.Any, GuildPermission.Any)]
        [Description("Das Leaderboard anzeigen.")]
        public async Task LeaderboardOther(CommandContext ctx, DiscordMember member = null)
        {
            var user = await User.GetOrCreateUser(member);
            var skip = (int)await user.GetRank();

            var lb = await Mongo.Instance.GetLeaderboard(skip, ctx.Guild.Id);
            await SendLeaderboard(ctx, lb, user);
        }

        private async Task SendLeaderboard(CommandContext ctx, List<User> lb, User wanted)
        {
            var embed = new DiscordEmbedBuilder()
                .WithTitle("Leaderboard")
                .WithColor(DiscordColor.Chartreuse);
            var guild = await Guild.GetGuild(ctx.Guild.Id);
            var wantedRank = await wanted.GetRank();
            foreach (var u in lb)
            {
                var rank = await u.GetRank();
                if (rank == wantedRank)
                {
                    embed.AddField($"> #{rank.ToString()} {u.UserName}", $"{u.Points.ToString()} {guild.RankingPointName}");
                }
                else
                {
                    embed.AddField($"#{rank.ToString()} {u.UserName}", $"{u.Points.ToString()} {guild.RankingPointName}");
                }
                
            }

            await ctx.Channel.SendMessageAsync(embed: embed.Build());
        }
        
        [Command("give-points"), Aliases("give")]
        [CheckForPermissions(SudoPermission.Mod, GuildPermission.Any)]
        [Description("Einem User Bonus Punkte Geben")]
        public async Task GiveSp(CommandContext ctx, [Description("Der User der die Punkte Erhalten Soll")]DiscordMember member, [Description("Anzahl der Punkte")]int count)
        {
            var user = await User.GetOrCreateUser(member);
            var guild = await Guild.GetGuild(user.GuildId);
            await user.AddSpecialPoints(count);
            await ctx.Channel.SendMessageAsync($"{member.Mention} hat {count.ToString()} {guild.RankingPointName ?? "XP"} erhalten");
        }
        
        [Command("remove-points"), Aliases("remove")]
        [CheckForPermissions(SudoPermission.Mod, GuildPermission.Any)]
        [Description("Einem User Bonus Punkte Entfernen")]
        public async Task RemoveSp(CommandContext ctx, [Description("Der User dem die Punkte abgezogen werden")]DiscordMember member, [Description("Anzahl der Punkte")]int count)
        {
            var user = await User.GetOrCreateUser(member);
            var guild = await Guild.GetGuild(user.GuildId);
            await user.AddSpecialPoints(-count);
            await ctx.Channel.SendMessageAsync($"{member.Mention} hat {count.ToString()} {guild.RankingPointName ?? "XP"} verloren");
        }

        [Command("set-role")]
        [Aliases("sr")]
        [Description("Eine Rolle fürs Ranking System Festlegen")]
        [CheckForPermissions(SudoPermission.Admin, GuildPermission.Any)]
        public async Task SetRankingRole(CommandContext ctx, [Description("Die Rolle die zu erhalten ist")]DiscordRole role, [Description("Anzahl der Punkte bei der man die Rolle erhält")]int points)
        {
            var guild = await Guild.GetGuild(ctx.Guild.Id);
            await guild.AddRankingRole(role, points);
            await ctx.Channel.SendMessageAsync($"Die Rolle {role.Name} ist mit {points.ToString()} IQ zu Erreichen!");
        }
        
        [Command("remove-role")]
        [Aliases("rr")]
        [Description("Eine Rolle aus dem Ranking System Entfernen")]
        [CheckForPermissions(SudoPermission.Admin, GuildPermission.Any)]
        public async Task RemoveRankingRole(CommandContext ctx, [Description("Die zu entfernende Rolle")]DiscordRole role)
        {
            var guild = await Guild.GetGuild(ctx.Guild.Id);
            var success = await guild.RemoveRankingRole(role);
            if (success)
            {
                await ctx.Channel.SendMessageAsync($"Die Rolle {role.Name} wurde aus dem Ranking entfernt!");
            }
            else
            {
                await ctx.Channel.SendMessageAsync($"Die Rolle {role.Name} ist nicht im Ranking!");
            }
        }

        [Command("set-name")]
        [Description("Den Namen der Punkte setzen")]
        [CheckForPermissions(SudoPermission.Admin, GuildPermission.Any)]
        public async Task SetRankingName(CommandContext ctx, [Description("Der Name den die Punkte haben sollen (Default: XP)")]string name)
        {
            var guild = await Guild.GetGuild(ctx.Guild.Id);
            var oldname = guild.RankingPointName;
            await guild.SetRankingPointsName(name);
            await ctx.Channel.SendMessageAsync($"Der Name wurde von {oldname} auf {guild.RankingPointName} geändert!");
        }
        
        [Command("set-time-multiplier")]
        [Description("Setzen wie viel ein Tag als Nachrichten zählt (Default: 10 Nachrichten Pro Tag seit Join Date)")]
        [CheckForPermissions(SudoPermission.Admin, GuildPermission.Any)]
        public async Task SetTimeMultiplier(CommandContext ctx, [Description("Der Multiplikator")]int ammount)
        {
            if (ammount < 0)
            {
                await ctx.Channel.SendMessageAsync("Muss ein Positiver Wert sein!");
                return;
            }
            var guild = await Guild.GetGuild(ctx.Guild.Id);
            await guild.SetRankingTimeMultipier(ammount);
            await ctx.Channel.SendMessageAsync($"Der Zeit Multiplikator wurde auf {guild.RankingTimeMultiplier.ToString()} gesetzt!");
        }

        [Command("set-multiplier")]
        [Description("Lege einen XP Multiplier fest (default 1), um beispilesweise Double XP Weekends zu machen")]
        [CheckForPermissions(SudoPermission.Admin, GuildPermission.Any)]
        public async Task SetMultiplier(CommandContext ctx, [Description("Der Multiplikator")] int ammount)
        {
            if (ammount < 0)
            {
                await ctx.Channel.SendMessageAsync("Muss ein Positiver Wert sein!");
                return;
            }
            var guild = await Guild.GetGuild(ctx.Guild.Id);
            await guild.SetRankingMultipier(ammount);
            await ctx.Channel.SendMessageAsync($"Der Multiplikator wurde auf {guild.RankingMultiplier.ToString()} gesetzt!");

        }

        [Command("list")]
        [Description("Auflistung aller Rollen im Ranking System")]
        [CheckForPermissions(SudoPermission.Any, GuildPermission.Any)]
        public async Task ListRankingRoles(CommandContext ctx)
        {
            try
            {
                var guild = await Guild.GetGuild(ctx.Guild.Id);
                var roles = guild.RankingRoles.OrderBy(x => x.Points).ToList();

                if (roles == null || roles.Count == 0)
                {
                    await ctx.Channel.SendMessageAsync(
                        "Es wurden noch keine Rollen festgelegt, siehe `$help rank set-role`");
                    return;
                }

                int embedCount = 1 + roles.Count / 24;
                
                List<DiscordEmbedBuilder> embeds = new List<DiscordEmbedBuilder>();

                for (var i = 0; i < embedCount; i++)
                {
                    var currRoles = roles.Skip(i * 24).Take(24);
                    DiscordEmbedBuilder embed;
                    if (i == 0)
                    {
                        embed = new DiscordEmbedBuilder()
                            .WithColor(DiscordColor.Aquamarine)
                            .WithTitle("Rollen");
                    }
                    else
                    {
                        embed = new DiscordEmbedBuilder()
                            .WithColor(DiscordColor.Aquamarine);
                    }
                    
                
                    foreach (var r in currRoles)
                    {
                        var drole = ctx.Guild.GetRole(r.Role);
                        embed.AddField($"{r.Points.ToString()} {guild.RankingPointName ?? "XP"}", drole.Mention, true);
                    }
                    embeds.Add(embed);
                }

                foreach (var embed in embeds)
                {
                    await ctx.Channel.SendMessageAsync(embed: embed.Build());
                }
            }
            catch (Exception e)
            {
                await ctx.RespondAsync($"Es ist ein fehler aufgetreten: {e.Message}");
            }
        }
    }
}