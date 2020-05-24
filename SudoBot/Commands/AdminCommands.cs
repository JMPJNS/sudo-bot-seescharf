using System;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using SudoBot.Attributes;
using SudoBot.Database;
using SudoBot.Models;

namespace SudoBot.Commands
{
    [Group("admin")]
    [Aliases("a")]
    [Description("Administrations Stuff")]
    public class AdminCommands : BaseCommandModule
    {
        [CheckForPermissions(SudoPermission.Me, GuildPermission.Any)]
        [Command("give-guild-permission")]
        [Aliases("ggp")]
        public async Task GiveGuildPermission(CommandContext ctx, string p)
        {
            Enum.TryParse(p, out GuildPermission perm);
            if (perm == GuildPermission.Any)
            {
                await ctx.Channel.SendMessageAsync("Ungültige Eingabe");
                return;
            }
            var guildConfig = await Guild.GetGuild(ctx.Guild.Id);
            if (!guildConfig.Permissions.Contains(perm))
            {
                await guildConfig.GivePermission(perm);
                await ctx.Channel.SendMessageAsync("Die Rechte wurden Vergeben!");
            }
        }
        
        // Mute
        // // Alle Rollen weg nehmen, und 639522863023521822 geben, bei unmuted wieder rollen geben
        
        [CheckForPermissions(SudoPermission.Me, GuildPermission.Any)]
        [Command("get-all-permissions")]
        [Aliases("gap")]
        public async Task GetAllPermissions(CommandContext ctx)
        {
            var perms = string.Join("\n", Enum.GetNames(typeof(GuildPermission)));
            await ctx.Channel.SendMessageAsync(perms);
        }

        [CheckForPermissions(SudoPermission.Admin, GuildPermission.Any)]
        [Description("Setze den Log Channel für diverse Bot Aktionen")]
        [Command("set-log-channel"), Aliases("slc")]
        public async Task SetLogChannel(CommandContext ctx, [Description("Der Channel für die Nachrichten")]DiscordChannel channel)
        {
            var guild = await Guild.GetGuild(ctx.Guild.Id);
            await guild.SetLocalLogChannel(channel.Id);
            await ctx.Channel.SendMessageAsync("Der Channel wurde Gesetzt");
        }
    }
}