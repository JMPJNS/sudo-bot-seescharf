using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using SudoBot.Attributes;

namespace SudoBot.Commands
{
    [Group("apec")]
    [Description("Apec Stuff")]
    public class ApecCommands: BaseCommandModule
    {
        private ulong _guildId = 468835109844418575;
        private ulong _channelId = 710985729655701577;
        private string _charset = "0123456789ABCDEF";

        [CheckForPermissions(SudoPermission.Mod, GuildPermission.Any)]
        [Description("Setze die Custom Games Rolle")]
        [Command("termin")]
        public async Task AddTermin(CommandContext ctx, string name, string datum, string link)
        {
            var guild = await ctx.Client.GetGuildAsync(_guildId);
            var channel = guild.GetChannel(_channelId);

            var color = "";

            foreach (var x in name.ToUpper())
            {
                if (_charset.Contains(x))
                {
                    color += x;
                    continue;
                }

                color += "0";
            }

            if (color.Length > 6)
            {
                color = color.Take(6).ToString();
            }

            if (color.Length < 6)
            {
                color = color.PadLeft(6, 'F');
            }
            
            var embed = new DiscordEmbedBuilder()
                .WithColor(new DiscordColor(color));

            embed.AddField("Name", name);
            embed.AddField("Datum", datum);
            embed.AddField("Link", link);

            await channel.SendMessageAsync(embed: embed.Build());
        }
    }
}