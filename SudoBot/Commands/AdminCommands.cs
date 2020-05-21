using System;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using SudoBot.Attributes;
using SudoBot.Database;

namespace SudoBot.Commands
{
    public class AdminCommands : BaseCommandModule
    {
        [CheckForPermissions(SudoPermission.Me, GuildPermission.Any)]
        [Command("giveGuildPermission")]
        public async Task GiveGuildPermission(CommandContext ctx, string p)
        {
            Enum.TryParse(p, out GuildPermission perm);
            if (perm == GuildPermission.Any)
            {
                await ctx.Channel.SendMessageAsync("Ungültige Eingabe");
                return;
            }
            var guildConfig = await Mongo.Instance.GetGuild(ctx.Guild.Id);
            if (!guildConfig.Permissions.Contains(perm))
            {
                await guildConfig.GivePermission(perm);
                await ctx.Channel.SendMessageAsync("Die Rechte wurden Vergeben!");
            }
        }
        
        // Mute
        // // Alle Rollen weg nehmen, und 639522863023521822 geben, bei unmuted wieder rollen geben
        // Ban

        [CheckForPermissions(SudoPermission.Me, GuildPermission.Any)]
        [Command("getAllPermissions")]
        public async Task GetAllPermissions(CommandContext ctx)
        {
            var perms = string.Join("\n", Enum.GetNames(typeof(GuildPermission)));
            await ctx.Channel.SendMessageAsync(perms);
        }
    }
}