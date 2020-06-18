using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using SudoBot.Database;
using SudoBot.Models;

namespace SudoBot.Attributes
{
    public class CheckForPermissionsAttribute : CheckBaseAttribute
    {
        private SudoPermission _perm;
        private GuildPermission _guildPerm;
        public CheckForPermissionsAttribute(SudoPermission perm, GuildPermission guildPerm)
        {
            _perm = perm;
            _guildPerm = guildPerm;
        }
        public override Task<bool> ExecuteCheckAsync(CommandContext ctx, bool help)
        {
            if (ctx.Member.Id == Globals.MyId) return Task.FromResult(true);
            
            bool isHelp = ctx.Command.Name == "help";
            if (ctx.Guild == null || ctx.Member == null) return Task.FromResult(false);

            var guildConfig = Guild.GetGuild(ctx.Guild.Id).GetAwaiter().GetResult();

            if (guildConfig == null)
            {
                ctx.Channel.SendMessageAsync("ERROR GUILD NOT FOUND, CONTACT JMP#7777");
                ctx.Channel.SendMessageAsync($"MessageID: {ctx.Message.Id.ToString()}, GuildID: {ctx.Guild.Id.ToString()}, ChannelID: {ctx.Channel.Id.ToString()}");
                return Task.FromResult(false);
            }

            // Guild Permission Check
            if (_guildPerm != GuildPermission.Any && !guildConfig.Permissions.Contains(GuildPermission.All))
            {
                if (!guildConfig.Permissions.Contains(_guildPerm))
                {
                    if (!isHelp) ctx.Channel.SendMessageAsync("Dieser Command ist für diesen Discord nicht Freigegeben, für mehr Infos kontaktiert JMP#7777");
                    return Task.FromResult(false);
                }
            }

            switch (_perm)
            {
                // Roles Check
                case SudoPermission.Any:
                    return Task.FromResult(true);
                case SudoPermission.Me when ctx.Member.Id == Globals.MyId:
                    return Task.FromResult(true);
                case SudoPermission.Admin when ctx.Member.Roles.Any(x => Globals.AdminRoles.Any(y => y == x.Name)) || (ctx.Member.PermissionsIn(ctx.Channel) & Permissions.Administrator) != 0:
                    return Task.FromResult(true);
                case SudoPermission.Mod when ctx.Member.Roles.Any(x => Globals.ModRoles.Any(y => y == x.Name)) || (ctx.Member.PermissionsIn(ctx.Channel) & Permissions.ManageMessages) != 0:
                    return Task.FromResult(true);
                default:
                    if (!isHelp) ctx.Channel.SendMessageAsync($"Du hast keine Berechtigung dazu {DiscordEmoji.FromName(ctx.Client, ":smirk:")}");
            
                    return Task.FromResult(false);
            }
        }
    }
}