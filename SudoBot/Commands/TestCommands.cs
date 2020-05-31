using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
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
        [Cooldown(1, 20, CooldownBucketType.User)]
        [Description("test")]
        public async Task Test(CommandContext ctx)
        {
            await ctx.Channel.SendMessageAsync("asdf");
        }

        // [Command("RemoveAllNotVerified")]
        // [CheckForPermissions(SudoPermission.Me, GuildPermission.Any)]
        // public async Task ranv(CommandContext ctx)
        // {
        //     await ctx.Channel.SendMessageAsync("removing the role");
        //     await StanFunctions.RemoveAllNotVerified(ctx);
        //     await ctx.Channel.SendMessageAsync("done");
        // }

        [Command("divide-channels")]
        [CheckForPermissions(SudoPermission.Me, GuildPermission.Any)]
        public async Task DivideChannels(CommandContext ctx)
        {
            ulong group;
            DiscordChannel messageChannel = ctx.Guild.GetChannel(710985729655701577);
            DiscordEmoji emoji = DiscordEmoji.FromName(ctx.Client, ":Tofu:");
            DiscordMessage message = await messageChannel.GetMessageAsync(716321439728402543);
            int usersPerChannel = 1;
            int currentChannel = 0;
            int index = 0;
            
            foreach (var member in await message.GetReactionsAsync(emoji))
            {
                if (index % usersPerChannel == 0) currentChannel += 1;
                
                
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