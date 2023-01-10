using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using Microsoft.Extensions.Logging;
using SudoBot.Attributes;
using SudoBot.Database;
using SudoBot.Models;
using SudoBot.Handlers;

namespace SudoBot.Commands
{
    [SlashCommandGroup("rank", "Leveling system commands")]
    [GuildOnly]
    public class RankCommands : ApplicationCommandModule
    {
        public Translation Translator { private get; set; }

        [SlashCommand("info", "Check a users Level")]
        [DescriptionLocalization(Localization.German, "Das Level eines Users abfragen")]
        public async Task InfoAsync(InteractionContext ctx, [Option("user", "user")] DiscordUser discordUser = null)
        {
            var member = discordUser != null ? await ctx.Guild.GetMemberAsync(discordUser.Id) : ctx.Member;
            var user = await User.GetOrCreateUser(member);
            var guild = await Guild.GetGuild(user.GuildId);

            if (ctx.Member.Id != Globals.MyId && guild.CommandChannel != 0 && guild.CommandChannel != ctx.Channel.Id)
            {
                var channel = ctx.Guild.GetChannel(guild.CommandChannel);
                await ctx.CreateResponseAsync(
                    Translator.Translate("RANKING_CHANNEL_NOT_ALLOWED", Translation.Lang.De,
                        new List<string> { channel.Mention }), true);
                return;
            }

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
                embed.AddField("Verbleibend", $"{currNext.Remaining.ToString()} {guild.RankingPointName ?? "XP"}",
                    true);
            }

            if (currNext.Next != null)
            {
                embed.AddField("Als Nächstes", $"{currNext.Next.Mention}", true);
            }

            embed.AddField("Beigetreten", user.JoinDate.ToString("dd.MM.yyyy H:mm"))
                .AddField(guild.RankingTimeMultiplier == 0
                        ? $"```{guild.RankingPointName} kriegt man durch Nachrichten schreiben!```"
                        : $"```{guild.RankingPointName} kriegt man durch Nachrichten schreiben!\nAußerdem erhälst du jeden Tag {guild.RankingTimeMultiplier.ToString()} {guild.RankingPointName}, rückwirkend seit du dem Discord Beigetreten bist!```",
                    "`/rank list` um alle Ränge anzuzeigen.");

            await ctx.CreateResponseAsync(embed);
        }

        [SlashCommand("leaderboard", "Get The Leveling Leaderboard")]
        [DescriptionLocalization(Localization.German, "Das Leaderboard abfragen")]
        public async Task Leaderboard(InteractionContext ctx,
            [Option("user", "Other Member (optional)")]
            DiscordUser? discordUser = null)
        {
            var member = discordUser != null ? await ctx.Guild.GetMemberAsync(discordUser.Id) : ctx.Member;
            var skip = 0;
            var user = await User.GetOrCreateUser(member);

            var lb = await Mongo.Instance.GetLeaderboard(skip, ctx.Guild.Id);

            var embed = new DiscordEmbedBuilder()
                .WithTitle("Leaderboard")
                .WithColor(DiscordColor.Chartreuse);
            var guild = await Guild.GetGuild(ctx.Guild.Id);
            var wantedRank = await user.GetRank();
            foreach (var u in lb)
            {
                var rank = await u.GetRank();
                if (rank == wantedRank)
                {
                    embed.AddField($"> #{rank.ToString()} {u.UserName}",
                        $"{u.Points.ToString()} {guild.RankingPointName}");
                }
                else
                {
                    embed.AddField($"#{rank.ToString()} {u.UserName}",
                        $"{u.Points.ToString()} {guild.RankingPointName}");
                }
            }

            await ctx.CreateResponseAsync(embed);
        }

        [SlashCommand("list", "Auflistung aller Rollen im Ranking System")]
        [DescriptionLocalization(Localization.German, "Auflistung aller Rollen im Ranking System")]
        public async Task ListRankingRoles(InteractionContext ctx)
        {
            try
            {
                var guild = await Guild.GetGuild(ctx.Guild.Id);
                var roles = guild.RankingRoles.OrderBy(x => x.Points).ToList();

                if (roles == null || roles.Count == 0)
                {
                    await ctx.CreateResponseAsync(
                        "Es wurden noch keine Rollen festgelegt, siehe `rank add-ranking-role`", true);
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
                        if (drole == null)
                        {
                            await guild.RemoveRankingRole(r.Role);
                            continue;
                        }

                        embed.AddField($"{r.Points.ToString()} {guild.RankingPointName ?? "XP"}", drole.Mention, true);
                    }

                    embeds.Add(embed);
                }

                foreach (var embed in embeds)
                {
                    await ctx.CreateResponseAsync(embed, true);
                }
            }
            catch (Exception e)
            {
                await ctx.CreateResponseAsync($"Es ist ein fehler aufgetreten: {e.Message}", true);
            }
        }
    }

    [SlashCommandGroup("rank-config", "Configure the Ranking system")]
    [SlashCommandPermissions(Permissions.ManageRoles)]
    public class SubGroup : ApplicationCommandModule
    {
        public Translation Translator { private get; set; }

        [SlashCommand("rank-all", "Recalculate the level of every guild member")]
        [SlashCommandPermissions(Permissions.Administrator)]
        [SlashRequirePermissions(Permissions.ManageRoles)]
        [DescriptionLocalization(Localization.German, "Das Level aller member neu berechnen")]
        public async Task RankAll(InteractionContext ctx)
        {
            await ctx.DeferAsync();
            var users = await Mongo.Instance.GetGuildUsers(ctx.Guild.Id);

            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Die Rollen werden neu berechnet"));

            int i = 0;

            foreach (var user in users)
            {
                try
                {
                    var res = await user.UpdateRankRoles();
                    if (res)
                    {
                        i++;
                        await ctx.EditResponseAsync(
                            new DiscordWebhookBuilder().WithContent($"Updated {user.UserId}"));
                    }
                }
                catch (Exception e)
                {
                    await ctx.EditResponseAsync(
                        new DiscordWebhookBuilder().WithContent($"Error: {e.Message}, [{user.UserId}]"));
                }
            }

            await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent($"Done, {i} user angepasst"));
        }

        [SlashCommand("give-points", "Add Bonus points to a user")]
        [DescriptionLocalization(Localization.German, "Einem User Bonus Punkte Geben")]
        public async Task GiveSp(
            InteractionContext ctx,
            [Option("user", "The user you want to give points")]
            [DescriptionLocalization(Localization.German, "Der Member der die Punkte Erhalten Soll")]
            DiscordUser discordUser,
            [Option("count", "How many points")] [DescriptionLocalization(Localization.German, "Wie viele Punkte")]
            long count
        )
        {
            var member = await ctx.Guild.GetMemberAsync(discordUser.Id);
            var user = await User.GetOrCreateUser(member);
            var guild = await Guild.GetGuild(user.GuildId);
            await user.AddSpecialPoints(Convert.ToInt32(count));
            await ctx.CreateResponseAsync(
                $"{member.Mention} hat {count.ToString()} {guild.RankingPointName ?? "XP"} erhalten", true);
        }

        [SlashCommand("remove-points", "Subtract bonus points from a user")]
        [DescriptionLocalization(Localization.German, "Einem User Bonus Punkte Entziehen")]
        public async Task RemoveSp(
            InteractionContext ctx,
            [Option("user", "The member you want to subtract points")]
            [DescriptionLocalization(Localization.German, "Der Member dem die Punkte Abgezogen werden sollen")]
            DiscordUser discordUser,
            [Option("count", "How many points")] [DescriptionLocalization(Localization.German, "Wie viele Punkte")]
            long count
        )
        {
            var member = await ctx.Guild.GetMemberAsync(discordUser.Id);
            var user = await User.GetOrCreateUser(member);
            var guild = await Guild.GetGuild(user.GuildId);
            await user.AddSpecialPoints(-Convert.ToInt32(count));
            await ctx.CreateResponseAsync(
                $"{member.Mention} hat {count.ToString()} {guild.RankingPointName ?? "XP"} erhalten", true);
        }

        [SlashCommand("add-ranking-role", "Add a role to the ranking system")]
        [DescriptionLocalization(Localization.German, "Eine Rolle zum Ranglisten System hinzufügen")]
        public async Task SetRankingRole(
            InteractionContext ctx,
            [Option("role", "The role you want to add to the system")]
            [DescriptionLocalization(Localization.German, "Die Rolle die vergeben werden soll")]
            DiscordRole role,
            [Option("points", "With how many points one should reach this role")]
            [DescriptionLocalization(Localization.German, "Mit wie vielen Punkten diese zu erreichen ist")]
            long points
        )
        {
            var guild = await Guild.GetGuild(ctx.Guild.Id);
            try
            {
                var bu = ctx.Client.CurrentUser;
                var bm = await ctx.Guild.GetMemberAsync(bu.Id);

                await bm.GrantRoleAsync(role);
                await bm.RevokeRoleAsync(role);

                await guild.AddRankingRole(role, Convert.ToInt32(points));
                await ctx.CreateResponseAsync(
                    $"Die Operation war erfolgreich! Die Rolle {role.Name} ist mit {points.ToString()} {guild.RankingPointName} zu Erreichen!",
                    true);
            }
            catch (UnauthorizedException)
            {
                await ctx.CreateResponseAsync(Translator.Translate("RANKING_BOT_NO_PERMISSION", guild.Language),
                    true);
            }
        }

        [SlashCommand("remove-ranking-role", "Remove a role from the ranking system")]
        [DescriptionLocalization(Localization.German, "Eine Rolle zum Ranglisten System hinzufügen")]
        public async Task RemoveRankingRole(
            InteractionContext ctx,
            [Option("role", "The role you want to add to the system")]
            [DescriptionLocalization(Localization.German, "Die Rolle die vergeben werden soll")]
            DiscordRole role
        )
        {
            var guild = await Guild.GetGuild(ctx.Guild.Id);
            await guild.RemoveRankingRole(role);
            await ctx.CreateResponseAsync($"Die Rolle wurde aus dem Ranking System entfernt!", true);
        }

        [SlashCommand("set-points-name", "Set how you want leveling points to be called")]
        [DescriptionLocalization(Localization.German, "Wie sollen die Ranking Punkte genannt werden")]
        [SlashCommandPermissions(Permissions.Administrator)]
        [SlashRequirePermissions(Permissions.Administrator)]
        public async Task SetPointsName(
            InteractionContext ctx,
            [Option("name", "How you want leveling points to be called")]
            [DescriptionLocalization(Localization.German, "Wie sollen die Ranking Punkte genannt werden")]
            string name
        )
        {
            var guild = await Guild.GetGuild(ctx.Guild.Id);
            var oldname = guild.RankingPointName;
            await guild.SetRankingPointsName(name);
            await ctx.CreateResponseAsync($"Der Name wurde von {oldname} auf {guild.RankingPointName} geändert!");
        }

        [SlashCommand("set-time-multiplier", "Set a multiplier for how many points over time someone should get")]
        [DescriptionLocalization(Localization.German,
            "Multiplikator setzen, wie viele punkte man über Zeit erhalten soll")]
        [SlashCommandPermissions(Permissions.Administrator)]
        [SlashRequirePermissions(Permissions.Administrator)]
        public async Task SetTimeMultiplier(
            InteractionContext ctx,
            [Option("multiplier", "multiplier, how many points over time someone should get")]
            [DescriptionLocalization(Localization.German,
                "Multiplikator, wie viele punkte man über Zeit erhalten soll")]
            long multiplier
        )
        {
            if (multiplier < 0)
            {
                await ctx.CreateResponseAsync("Muss ein Positiver Wert sein!", true);
                return;
            }

            var guild = await Guild.GetGuild(ctx.Guild.Id);
            await guild.SetRankingTimeMultipier(Convert.ToInt32(multiplier));
            await ctx.CreateResponseAsync(
                $"Der Zeit Multiplikator wurde auf {guild.RankingTimeMultiplier.ToString()} gesetzt!", true);
        }

        [SlashCommand("set-multiplier", "Set a multiplier for how many points someone should get")]
        [DescriptionLocalization(Localization.German,
            "Multiplikator setzen, wie viele punkte man über Zeit erhalten soll")]
        [SlashCommandPermissions(Permissions.Administrator)]
        [SlashRequirePermissions(Permissions.Administrator)]
        public async Task SetMultiplier(
            InteractionContext ctx,
            [Option("multiplier", "multiplier, how many points someone should get")]
            [DescriptionLocalization(Localization.German, "Multiplikator, wie viele Punkte man erhalten soll")]
            long multiplier
        )
        {
            if (multiplier < 0)
            {
                await ctx.CreateResponseAsync("Muss ein Positiver Wert sein!", true);
                return;
            }

            var guild = await Guild.GetGuild(ctx.Guild.Id);
            await guild.SetRankingMultipier(Convert.ToInt32(multiplier));
            await ctx.CreateResponseAsync(
                $"Der Multiplikator wurde auf {guild.RankingMultiplier.ToString()} gesetzt!", true);
        }
    }

    [Group("rank"), Aliases("r", "ranking")]
    [Description("Leveling System")]
    public class LegacyRankCommands : BaseCommandModule
    {
        public Translation Translator { private get; set; }
        
        [CheckForPermissions(SudoPermission.Any, GuildPermission.Any)]
        [Description("Information über den Aktuellen Rang (30s Cooldown)")]
        [GroupCommand]
        public async Task Rank(CommandContext ctx, [Description("Anderer User (optional)")]DiscordMember? member = null)
        {
            member ??= ctx.Member;
            
            var embed = new DiscordEmbedBuilder()
                .WithColor(member.Color)
                .WithThumbnail(member.AvatarUrl)
                .WithTitle(member.Nickname ?? member.Username);

            embed.WithDescription(
                "Das Ranking System ist jetzt als Slash Command verfügbar!, siehe `/rank`"
                );
            
            await ctx.Channel.SendMessageAsync(embed:embed.Build());
        }
    }
}