using System;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using SudoBot.Attributes;
using SudoBot.Database;

namespace SudoBot.Commands
{
    [Group("test")]
    public class TestCommands : BaseCommandModule
    {
        [CheckForPermissions(SudoPermission.Me, GuildPermission.Any)]
        [Command("test")]
        public async Task Test(CommandContext ctx)
        {
            var users = await Mongo.Instance.GetAllUsersWithPrevRole();
            foreach (var user in users)
            {
                int addMessages = 0;
                if (user.HighestOldLevel == 1)
                {
                    addMessages = 1;
                } else if (user.HighestOldLevel == 5)
                {
                    addMessages = 50;
                } else if (user.HighestOldLevel == 10)
                {
                    addMessages = 150;
                } else if (user.HighestOldLevel == 15)
                {
                    addMessages = 500;
                } else if (user.HighestOldLevel == 20)
                {
                    addMessages = 700;
                } else if (user.HighestOldLevel == 30)
                {
                    addMessages = 1000;
                } else if (user.HighestOldLevel == 40)
                {
                    addMessages = 1200;
                } else if (user.HighestOldLevel == 50)
                {
                    addMessages = 1400;
                } else if (user.HighestOldLevel == 60)
                {
                    addMessages = 1600;
                } else if (user.HighestOldLevel == 70)
                {
                    addMessages = 3000;
                } else if (user.HighestOldLevel == 80)
                {
                    addMessages = 5000;
                }
                else
                {
                    addMessages = 0;
                }

                await user.AddCountedMessagesByHand(addMessages);
                Console.WriteLine("Added");
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