using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using OllekBot.DataInterfaces;
using OllekBot.Handlers;

namespace OllekBot.Commands
{
    public class CustomGamesCommands: BaseCommandModule
    {
        [Command("elf")]
        public async Task Elf(CommandContext ctx)
        {
            var sentMessage = await ctx.Channel.SendMessageAsync($"Bruv");
            // die zeile drunter einfach in while (true) loop packen und auf bestimmter reaction einfach breaken, easy message collector
            var reactionResult = await ctx.Client.GetInteractivity().WaitForReactionAsync(x =>
            {
                return x.Emoji == DiscordEmoji.FromName(ctx.Client, ":x:") && x.Message == sentMessage;
            });
            await ctx.Channel.SendMessageAsync($"{reactionResult.Result}");
        }
    }
}