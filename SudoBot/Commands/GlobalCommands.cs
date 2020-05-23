using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using SudoBot.Attributes;
using SudoBot.Models;

namespace SudoBot.Commands
{
    public class GlobalCommands : BaseCommandModule
    {
      
        [CheckForPermissions(SudoPermission.Any, GuildPermission.Ranking)]
        [Command("rank")]
        public async Task RankOther(CommandContext ctx, DiscordMember member = null)
        {
            if (member == null) member = ctx.Member;
            var user = await User.GetOrCreateUser(member);
            var guild = await Guild.GetGuild(user.GuildId);
            
            await user.UpdateRankRoles();
        
            var embed = new DiscordEmbedBuilder()
                .WithColor(member.Color)
                .WithThumbnailUrl(member.AvatarUrl)
                .WithTitle(member.Nickname ?? member.Username)
                .AddField("Bonus Punkte", user.SpecialPoints.ToString(), true)
                .AddField(guild.RankingPointName ?? "XP", user.CalculatePoints().ToString(), true)
                .AddField("Beigetreten", user.JoinDate.ToString("dd.MM.yyyy H:mm"), true);
            
            await ctx.Channel.SendMessageAsync(embed:embed.Build());
        }

        [CheckForPermissions(SudoPermission.Mod, GuildPermission.Any)]
        [Command("say")]
        public async Task Say(CommandContext ctx, params string[] words)
        {
            
        }
    }
}