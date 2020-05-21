using System;
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
    public class RankCommands: BaseCommandModule
    {
        [CheckForPermissions(SudoPermission.Any, GuildPermission.Ranking)]
        [Command("rank")]
        public async Task Rank(CommandContext ctx)
        {
            var user = await User.GetOrCreateUser(ctx.Member);
            await ctx.Channel.SendMessageAsync($"{ctx.Member.Mention} hat {user.CountedMessages.ToString()} gesendete Nachrichten, {user.SpecialPoints.ToString()} Spezial Punkte und damit {user.CalculatePoints().ToString()} IQ");
        }
        
        [Command("giveSP")]
        [CheckForPermissions(SudoPermission.Mod, GuildPermission.Ranking)]
        public async Task GiveSp(CommandContext ctx, DiscordMember member, int count)
        {
            var user = await User.GetOrCreateUser(member);
            await user.AddSpecialPoints(count);
            await ctx.Channel.SendMessageAsync($"{member.Mention} hat {count} IQ erhalten");
        }

        [Command("setRankingRole")]
        [CheckForPermissions(SudoPermission.Admin, GuildPermission.Ranking)]
        public async Task SetRankingRole(CommandContext ctx, int points, DiscordRole role)
        {
            var guild = await Mongo.Instance.GetGuild(ctx.Guild.Id);
            await guild.AddRankingRole(role, points);
            await ctx.Channel.SendMessageAsync($"Die Rolle {role.Name} ist mit {points.ToString()} IQ zu Erreichen!");
        }

        [Command("sPerms")]
        [CheckForPermissions(SudoPermission.Me, GuildPermission.Ranking)]
        public async Task SPerms(CommandContext ctx)
        {
            await ctx.Channel.SendMessageAsync("Speichert Alle User!");

            try
            {
                var allMembers = await ctx.Guild.GetAllMembersAsync();
                var l80 = ctx.Guild.GetRole(702171347832406017);
                var l70 = ctx.Guild.GetRole(702158378935386112);
                var l60 = ctx.Guild.GetRole(702158324321484810);
                var l50 = ctx.Guild.GetRole(702158249041985586);
                var l40 = ctx.Guild.GetRole(702158169346015272);
                var l30 = ctx.Guild.GetRole(702158120461533325);
                var l20 = ctx.Guild.GetRole(702158058994008235);
                var l15 = ctx.Guild.GetRole(702157997530677368);
                var l10 = ctx.Guild.GetRole(702157810334433290);
                var l5 = ctx.Guild.GetRole(702157869130448988);
                var l1 = ctx.Guild.GetRole(702159345588109334);

                foreach (var member in allMembers)
                {
                    var user = await User.GetOrCreateUser(member);
                    if (member.Roles.Contains(l80))
                    {
                        await user.SetHighestOldLevel(80);
                    } else if (member.Roles.Contains(l70))
                    {
                        await user.SetHighestOldLevel(70);
                    } else if (member.Roles.Contains(l60))
                    {
                        await user.SetHighestOldLevel(60);
                    } else if (member.Roles.Contains(l50))
                    {
                        await user.SetHighestOldLevel(50);
                    } else if (member.Roles.Contains(l40))
                    {
                        await user.SetHighestOldLevel(40);
                    } else if (member.Roles.Contains(l30))
                    {
                        await user.SetHighestOldLevel(30);
                    } else if (member.Roles.Contains(l20))
                    {
                        await user.SetHighestOldLevel(20);
                    } else if (member.Roles.Contains(l15))
                    {
                        await user.SetHighestOldLevel(15);
                    } else if (member.Roles.Contains(l10))
                    {
                        await user.SetHighestOldLevel(10);
                    } else if (member.Roles.Contains(l5))
                    {
                        await user.SetHighestOldLevel(5);
                    } else if (member.Roles.Contains(l1))
                    {
                        await user.SetHighestOldLevel(1);
                    }
                }

                await ctx.Channel.SendMessageAsync("Fertig!");
            }
            catch (Exception e)
            {
                await ctx.Channel.SendMessageAsync($"Fehler!, {e.Message} {e.StackTrace}");
            }
        }
    }
}