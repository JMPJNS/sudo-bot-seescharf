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
            DiscordEmoji emoji = DiscordEmoji.FromName(ctx.Client, ":Tofu:");
            DiscordMessage message = await ctx.Channel.GetMessageAsync(725085826660434030);
            int usersPerChannel = 1;
            int currentChannel = 0;
            int index = 0;

            List<DiscordChannel> channels = new List<DiscordChannel>();
            
            foreach (var user in await message.GetReactionsAsync(emoji))
            {
                if (index < usersPerChannel) {
                    if (index == 0) {
                        var category = ctx.Guild.GetChannel(725088157414064169);
                        var channel = await ctx.Guild.CreateTextChannelAsync($"Gruppe {currentChannel.ToString()}", category);
                        channels.Add(channel);
                    }
                } else {
                    index = 0;
                    currentChannel += 1;
                }

                var member = await ctx.Guild.GetMemberAsync(user.Id);

                await channels[currentChannel].AddOverwriteAsync(member, DSharpPlus.Permissions.SendMessages);
                await channels[currentChannel].AddOverwriteAsync(member, DSharpPlus.Permissions.ReadMessageHistory);
                await channels[currentChannel].AddOverwriteAsync(member, DSharpPlus.Permissions.AccessChannels);
                index++;               
                
            }


        }

        [Command("leave-guild")]
        [CheckForPermissions(SudoPermission.Me, GuildPermission.TestCommands)]
        public async Task LeaveGuild(CommandContext ctx, ulong guildId)
        {
            var guild = await ctx.Client.GetGuildAsync(guildId);

            await ctx.Channel.SendMessageAsync($"Do you want to Leave guild {guild.Name} with {guild.MemberCount.ToString()} Members? Send yes to confirm");
            var interactivity = ctx.Client.GetInteractivity();
            var msg = interactivity.WaitForMessageAsync(m=> m.Author == ctx.Member && m.Content == "yes", TimeSpan.FromMinutes(1));

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