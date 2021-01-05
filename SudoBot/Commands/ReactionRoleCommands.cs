using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using SudoBot.Attributes;
using SudoBot.Models;

namespace SudoBot.Commands
{
    [Group("reaction")]
    public class ReactionRoleCommands: BaseCommandModule
    {
        [CheckForPermissions(SudoPermission.Mod, GuildPermission.Any)]
        [Description("Create a new Reaction Role")]
        [Command]
        public async Task SetupRole(CommandContext ctx, string title, string message, DiscordRole role, DiscordEmoji emoji)
        {
            var embed = new DiscordEmbedBuilder().WithTitle(title).WithDescription(message);
            
            embed.AddField(emoji.ToString(), role.Mention, true);
            var sentMessage = await ctx.RespondAsync(embed:embed.Build());
            await sentMessage.CreateReactionAsync(emoji);
            
            var reactionRole = new Guild.ReactionRole(sentMessage.Id, ctx.Channel.Id, emoji.Id, role.Id);
            var guild = await Guild.GetGuild(ctx.Guild.Id);
            await guild.AddReactionRole(reactionRole);
        }
    }
}