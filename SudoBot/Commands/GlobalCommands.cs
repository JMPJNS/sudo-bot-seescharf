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
        public async Task Rank(CommandContext ctx)
        {
            var user = await User.GetOrCreateUser(ctx.Member);
            var guild = await Guild.GetGuild(user.GuildId);

            await user.UpdateRankRoles();

            var embed = new DiscordEmbedBuilder()
                .WithColor(ctx.Member.Color)
                .WithThumbnailUrl(ctx.Member.AvatarUrl)
                .WithTitle(ctx.Member.Nickname ?? ctx.Member.Username)
                .AddField("Bonus Punkte", user.SpecialPoints.ToString(), true)
                .AddField(guild.RankingPointName ?? "XP", user.CalculatePoints().ToString(), true);
            
            await ctx.Channel.SendMessageAsync(embed:embed.Build());
        }
        
        [CheckForPermissions(SudoPermission.Any, GuildPermission.Ranking)]
        [Command("rank")]
        public async Task RankOther(CommandContext ctx, DiscordMember member)
        {
            var user = await User.GetOrCreateUser(member);
            var guild = await Guild.GetGuild(user.GuildId);
            
            await user.UpdateRankRoles();
        
            var embed = new DiscordEmbedBuilder()
                .WithColor(member.Color)
                .WithThumbnailUrl(member.AvatarUrl)
                .WithTitle(member.Nickname ?? member.Username)
                .AddField("Bonus Punkte", user.SpecialPoints.ToString(), true)
                .AddField(guild.RankingPointName ?? "XP", user.CalculatePoints().ToString(), true);
            
            await ctx.Channel.SendMessageAsync(embed:embed.Build());
        }
    }
}