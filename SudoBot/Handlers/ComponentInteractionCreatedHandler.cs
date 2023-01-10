using System;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using SudoBot.Commands;
using SudoBot.Models;

namespace SudoBot.Handlers;

public class ComponentInteractionCreatedHandler
{
    public async Task HandleInteraction(DiscordClient client, ComponentInteractionCreateEventArgs a)
    {
        var member = await a.Guild.GetMemberAsync(a.User.Id);
        if (member == null) return;
        var guild = await Guild.GetGuild(member.Guild.Id);
        var rankCommands = new RankCommands();

        if (a.Id == "rank_level")
        {
            var builder = await rankCommands.GetInfoMessageBuilder(member, false);
            await a.Interaction.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder(builder).AsEphemeral());
        }
        
        if (a.Id == "rank_leaderboard")
        {
            var builder = await rankCommands.GetLeaderboardMessageBuilder(member, withButtons: false);
            await a.Interaction.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder(builder).AsEphemeral());
        }
        
        if (a.Id == "rank_list")
        {
            var embeds = await rankCommands.GetListEmbedBuilders(member);
            foreach (var embed in embeds)
            {
                await a.Interaction.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder(new DiscordMessageBuilder().WithEmbed(embed)).AsEphemeral());
            }
        }
        
        if (a.Id == "rank_info")
        {
            var embed = new DiscordEmbedBuilder()
                .AddField("info", guild.RankingTimeMultiplier == 0
                    ? $"```{guild.RankingPointName} kriegt man durch Nachrichten schreiben!```"
                    : $"```{guild.RankingPointName} kriegt man durch Nachrichten schreiben!\nAußerdem erhälst du jeden Tag {guild.RankingTimeMultiplier.ToString()} {guild.RankingPointName}, rückwirkend seit du dem Discord Beigetreten bist!```");
            var builder = new DiscordMessageBuilder().WithEmbed(embed);
            await a.Interaction.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder(builder).AsEphemeral());
        }
    }
}