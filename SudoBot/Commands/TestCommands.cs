using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using SudoBot.Attributes;
using SudoBot.Database;
using SudoBot.Models;
using SudoBot.Specifics;

namespace SudoBot.Commands
{
    public class TestCommands : BaseCommandModule
    {

        [Command("test")]
        [CheckForPermissions(SudoPermission.Any, GuildPermission.TestCommands)]
        [Description("test")]
        public async Task Test(CommandContext ctx)
        {
            var user = ctx.Client.CurrentUser;
            var member = await ctx.Guild.GetMemberAsync(user.Id);

            await ctx.Channel.SendMessageAsync(member.Nickname);
        }

        // [Command("RemoveAllNotVerified")]
        // [CheckForPermissions(SudoPermission.Me, GuildPermission.Any)]
        // public async Task ranv(CommandContext ctx)
        // {
        //     await ctx.Channel.SendMessageAsync("removing the role");
        //     await StanFunctions.RemoveAllNotVerified(ctx);
        //     await ctx.Channel.SendMessageAsync("done");
        // }

        [Command("create-role")]
        [CheckForPermissions(SudoPermission.Me, GuildPermission.Any)]
        public async Task CreateRole(CommandContext ctx) {
            var r = await ctx.Guild.CreateRoleAsync($"G", 0);
            await ctx.RespondAsync("done");
        }

        [Command("dc")]
        [CheckForPermissions(SudoPermission.Me, GuildPermission.Any)]
        public async Task DivideChannels(CommandContext ctx)
        {
            await ctx.RespondAsync("starting");
            DiscordEmoji emoji = DiscordEmoji.FromName(ctx.Client, ":raised_hand:");

            var role = ctx.Guild.GetRole(707343944228405339);
            await ctx.RespondAsync($"got Role ({role.Mention})");

            var allMembers = await ctx.Guild.GetAllMembersAsync();
            await ctx.RespondAsync($"got all members ({allMembers.Count()})");

            var verifiedMembers = allMembers.Where(user => user.Roles.Contains(role));
            await ctx.RespondAsync("got verified members");

            var category = ctx.Guild.GetChannel(725097053335715890);
            await ctx.RespondAsync("got category");

            List<DiscordRole> roles = new List<DiscordRole>();

            double maxPerRole = 100;

            int roleCount = (int)Math.Ceiling(verifiedMembers.Count() / maxPerRole);

            try {
                for (int i = 1; i<=roleCount; i++) {
                    var name = $"turnier-gruppe-{i}";
                    var foundRole = ctx.Guild.Roles.FirstOrDefault(x => x.Value.Name == name);

                    if (foundRole.Value != null) {
                        await foundRole.Value.DeleteAsync();
                    }

                    var newRole = await ctx.Guild.CreateRoleAsync(name, 0);

                    var foundChannel = ctx.Guild.Channels.FirstOrDefault(x=> x.Value.Name == name);

                    if (foundChannel.Value != null) {
                        await foundChannel.Value.AddOverwriteAsync(newRole, DSharpPlus.Permissions.AccessChannels);
                    } else {
                        var channel = await ctx.Guild.CreateTextChannelAsync(name, category);
                        await channel.AddOverwriteAsync(newRole, DSharpPlus.Permissions.AccessChannels);
                    }

                    roles.Add(newRole);
                }
            } catch (Exception e) {
                await ctx.RespondAsync(e.Message);
                return;
            } 

            await ctx.RespondAsync("Channel/Rollen Eingestellt");
            await ctx.Channel.SendMessageAsync($"sorting {verifiedMembers.Count()} members into {roleCount} roles");

            try {
                int currentRole = 0;
                int index = 0;
                foreach(var member in verifiedMembers) {
                    try {
                        var alreadyRoles = member.Roles.Intersect(roles).ToArray();
                        foreach(var r in alreadyRoles) {
                            await member.RevokeRoleAsync(r);
                        }

                        await member.GrantRoleAsync(roles[currentRole]);

                        await ctx.RespondAsync($"[{member.Mention}-{member.Id}] Gruppe {currentRole + 1}, Index {index}");

                        index++;
                        if (index == maxPerRole) {
                            index = 0;
                            currentRole++;
                        }
                    } catch (Exception e) {
                        await ctx.RespondAsync($"{member.Mention} {member.Id} {e.Message}");
                    }
                    
                }
            } catch (Exception e) {
                await ctx.RespondAsync(e.Message);
            }

            await ctx.RespondAsync("done");
        }

        [Command("leave-guild")]
        [CheckForPermissions(SudoPermission.Me, GuildPermission.TestCommands)]
        public async Task LeaveGuild(CommandContext ctx, ulong guildId)
        {
            var guild = await ctx.Client.GetGuildAsync(guildId);

            await ctx.Channel.SendMessageAsync($"Do you want to Leave guild {guild.Name} with {guild.MemberCount.ToString()} Members? Send yes to confirm");
            var interactivity = ctx.Client.GetInteractivity();
            var msg = interactivity.WaitForMessageAsync(m => m.Author == ctx.Member && m.Content == "yes", TimeSpan.FromMinutes(1));

            if (msg != null)
            {
                await guild.LeaveAsync();
                await ctx.Channel.SendMessageAsync("Left Guild");
            }
        }


        [CheckForPermissions(SudoPermission.Admin, GuildPermission.TestCommands)]
        [Command("e")]
        public async Task E(CommandContext ctx, DiscordChannel channel)
        {


            var embed = new DiscordEmbedBuilder()
                .WithTitle("Das IQ-System")
                .WithColor(DiscordColor.Aquamarine)
                .WithDescription(
                    $@"Wir haben ein System, mit dem ihr durch Aktivität und auch reine Anwesenheit auf dem Server IQ sammeln könnt.
 
Dieses IQ-Punkte stellen ein komplett neues Levelsystem da, mit dem ihr ab einer gewissen Anzahl an IQ Rollen erhaltet.
 
Spammen wird nicht belohnt und verstößt zudem noch gegen die {channel.Mention}, also haltet euch an diese!

Außerdem könnt ihr auch durch Teilnahme an Contests etc. IQ verdienen, es lohnt sich also, auch an diesen teilzunehmen. ");
            await ctx.Channel.SendMessageAsync(embed: embed.Build());

        }
    }
}