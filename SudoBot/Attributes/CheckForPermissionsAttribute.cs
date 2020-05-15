using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using SudoBot.Database;

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
            if (ctx.Command.Name == "help") return Task.FromResult(true);
            if (ctx.Guild == null || ctx.Member == null) return Task.FromResult(false);

            var guildConfig = Mongo.Instance.GetGuild(ctx.Guild.Id).GetAwaiter().GetResult();

            if (guildConfig == null)
            {
                ctx.Channel.SendMessageAsync("ERROR GUILD NOT FOUND, CONTACT JMP#7777");
                ctx.Channel.SendMessageAsync($"MessageID: {ctx.Message.Id}, GuildID: {ctx.Guild.Id}, ChannelID: {ctx.Channel.Id}");
                return Task.FromResult(false);
            }

            // Guild Permission Check
            if (_guildPerm != GuildPermission.Any && !guildConfig.Permissions.Contains(GuildPermission.All))
            {
                if (!guildConfig.Permissions.Contains(_guildPerm))
                {
                    ctx.Channel.SendMessageAsync("Dieser Command ist für diesen Discord nicht Freigegeben, für mehr Infos kontaktiert JMP#7777");
                    return Task.FromResult(false);
                }
            }

            switch (_perm)
            {
                // Roles Check
                case SudoPermission.Any:
                    return Task.FromResult(true);
                case SudoPermission.Me when ctx.Member.Id == 272809112851578881:
                    return Task.FromResult(true);
                case SudoPermission.Admin when ctx.Member.Roles.Any(x => Globals.AdminRoles.Any(y => y == x.Name)):
                    return Task.FromResult(true);
                case SudoPermission.Mod when ctx.Member.Roles.Any(x => Globals.ModRoles.Any(y => y == x.Name)):
                    return Task.FromResult(true);
                default:
                    ctx.Channel.SendMessageAsync($"Du hast keine Berechtigung dazu {DiscordEmoji.FromName(ctx.Client, ":smirk:")}");
            
                    return Task.FromResult(false);
            }
        }
    }
}