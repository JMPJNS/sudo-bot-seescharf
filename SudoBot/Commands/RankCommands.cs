using System;
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
    [Group("ranking"), Aliases("r")]
    public class RankCommands: BaseCommandModule
    {
        [Command("givePoints")]
        [CheckForPermissions(SudoPermission.Mod, GuildPermission.Ranking)]
        public async Task GiveSp(CommandContext ctx, DiscordMember member, int count)
        {
            var user = await User.GetOrCreateUser(member);
            await user.AddSpecialPoints(count);
            await ctx.Channel.SendMessageAsync($"{member.Mention} hat {count.ToString()} IQ erhalten");
        }

        [Command("setRole")]
        [CheckForPermissions(SudoPermission.Admin, GuildPermission.Ranking)]
        public async Task SetRankingRole(CommandContext ctx, int points, DiscordRole role)
        {
            var guild = await Guild.GetGuild(ctx.Guild.Id);
            await guild.AddRankingRole(role, points);
            await ctx.Channel.SendMessageAsync($"Die Rolle {role.Name} ist mit {points.ToString()} IQ zu Erreichen!");
        }
        
        [Command("removeRole")]
        [CheckForPermissions(SudoPermission.Admin, GuildPermission.Ranking)]
        public async Task RemoveRankingRole(CommandContext ctx, DiscordRole role)
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

        [Command("setName")]
        [CheckForPermissions(SudoPermission.Admin, GuildPermission.Ranking)]
        public async Task SetRankingName(CommandContext ctx, string name)
        {
            var guild = await Guild.GetGuild(ctx.Guild.Id);
            var oldname = guild.RankingPointName;
            await guild.SetRankingPointsName(name);
            await ctx.Channel.SendMessageAsync($"Der Name wurde von {oldname} auf {guild.RankingPointName} geändert!");
        }
        
        [Command("setTimeMultiplier")]
        [CheckForPermissions(SudoPermission.Admin, GuildPermission.Ranking)]
        public async Task SetTimeMultiplier(CommandContext ctx, int ammount)
        {
            var guild = await Guild.GetGuild(ctx.Guild.Id);
            await guild.SetRankingTimeMultipier(ammount);
            await ctx.Channel.SendMessageAsync($"Der Zeit Multiplikator wurde auf {guild.RankingTimeMultiplier.ToString()} gesetzt!");
        }

        [Command("list")]
        [Description("Auflistung alle Rollen im Ranking System")]
        [CheckForPermissions(SudoPermission.Any, GuildPermission.Ranking)]
        public async Task ListRankingRoles(CommandContext ctx)
        {
            var guild = await Guild.GetGuild(ctx.Guild.Id);
            var roles = guild.RankingRoles.OrderBy(x => x.Points).ToList();

            if (roles == null || roles.Count == 0)
            {
                await ctx.Channel.SendMessageAsync("Es wurden noch keine Rollen festgelegt, siehe `$help ranking setRole`");
                return;
            } 
            
            var embed = new DiscordEmbedBuilder()
                .WithColor(DiscordColor.Aquamarine)
                .WithTitle("Rollen");
            
            foreach (var r in roles)
            {
                var drole = ctx.Guild.GetRole(r.Role);
                embed.AddField(drole.Name, r.Points.ToString(), true);
            }

            await ctx.Channel.SendMessageAsync(embed: embed.Build());
        }
    }
}