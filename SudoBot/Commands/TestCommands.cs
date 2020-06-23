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
            DiscordChannel mChannel = ctx.Guild.GetChannel(707341293717553183);
            DiscordMessage message = await mChannel.GetMessageAsync(716658998153052240);
            int usersPerChannel = 100;
            int currentChannel = 0;
            int index = 0;
            var category = ctx.Guild.GetChannel(725097053335715890);

            List<DiscordChannel> channels = new List<DiscordChannel>();

            var allUsers = await message.GetReactionsAsync(emoji, 999);

            await ctx.Channel.SendMessageAsync($"Sorting {allUsers.Count} members");

            foreach (var user in allUsers)
            {
                try {
                    if (index < usersPerChannel)
                    {
                        if (index == 0)
                        {
                            var channel = await ctx.Guild.CreateTextChannelAsync($"Gruppe {currentChannel.ToString()}", category);
                            channels.Add(channel);
                        }
                    }
                    else
                    {
                        index = 0;
                        currentChannel += 1;
                    }

                    try {
                        var member = await ctx.Guild.GetMemberAsync(user.Id);
                        try {
                            await channels[currentChannel].AddOverwriteAsync(member, DSharpPlus.Permissions.AccessChannels);
                        } catch (Exception ec) {
                            await ctx.Channel.SendMessageAsync($"Channel Not Found, {currentChannel}, All Channels {channels.ToString()} {channels.Count}");
                            continue;
                        }
                    } catch (Exception em) {
                        await ctx.Channel.SendMessageAsync($"Member Not Found, {user.Id} {user.Username}");
                        continue;
                    }

                    index++;

                } catch (Exception e) {
                    await ctx.Channel.SendMessageAsync($"Index {index}");
                    await ctx.Channel.SendMessageAsync($"CurrChannel {currentChannel}");
                    await ctx.Channel.SendMessageAsync(e.Message);
                    return;
                }

            }

            await ctx.RespondAsync("done");
            await ctx.Channel.SendMessageAsync($"Index {index}");
            await ctx.Channel.SendMessageAsync($"CurrChannel {currentChannel}");
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