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

        [Command("dc")]
        [CheckForPermissions(SudoPermission.Me, GuildPermission.Any)]
        public async Task DivideChannels(CommandContext ctx)
        {
            await ctx.RespondAsync("starting");
            DiscordEmoji emoji = DiscordEmoji.FromName(ctx.Client, ":raised_hand:");
            await ctx.RespondAsync("got emoji");
            DiscordChannel mChannel = ctx.Guild.GetChannel(707341293717553183);
            await ctx.RespondAsync("got channel");
            DiscordMessage message = await mChannel.GetMessageAsync(716658998153052240);
            await ctx.RespondAsync("got message");
            int usersPerChannel = 100;
            int currentChannel = 0;
            await ctx.RespondAsync("got category");

            List<DiscordRole> roles = new List<DiscordRole>();
            roles.Add(ctx.Guild.GetRole(726747279746269284));
            roles.Add(ctx.Guild.GetRole(726747591605223464));
            roles.Add(ctx.Guild.GetRole(726748035123642449));
            roles.Add(ctx.Guild.GetRole(726748109211697203));
            roles.Add(ctx.Guild.GetRole(726748492214829107));
            roles.Add(ctx.Guild.GetRole(726748658128912424));


            var allUsers = await message.GetReactionsAsync(emoji, 999);
            int numChannels = (allUsers.Count / usersPerChannel) + 1;

            await ctx.RespondAsync("got reactions");

            await ctx.Channel.SendMessageAsync($"Sorting {allUsers.Count} members");

            var delayTime = 2;
            try {
                for (int c = 0; c < numChannels; c++) {
                    for (int i = 0; i < usersPerChannel ; i++) {
                        try {
                            var member = await ctx.Guild.GetMemberAsync(allUsers[(c+1)*i].Id);
                            await member.GrantRoleAsync(roles[c]);
                            await ctx.RespondAsync($"#{(c+1)*i} - Gruppe {c} - {allUsers[(c+1)*i].Id} - {allUsers[(c+1)*i].Username}");
                        } catch (Exception e) {
                            await ctx.RespondAsync($" #{(c+1)*i} - {allUsers[(c+1)*i].Id} - {e.Message}");
                        }
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