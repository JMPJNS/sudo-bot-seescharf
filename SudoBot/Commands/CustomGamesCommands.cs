using System;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using SudoBot.DataInterfaces;
using SudoBot.Handlers;

namespace SudoBot.Commands
{
    public class CustomGamesCommands: BaseCommandModule
    {
        [Command("elf")]
        public async Task Elf(CommandContext ctx)
        {
            var sentMessage = await ctx.Channel.SendMessageAsync($"asdf");

            var xEmoji = DiscordEmoji.FromName(ctx.Client, ":x:");
            var bEmoji = DiscordEmoji.FromName(ctx.Client, ":b:");
            var boneEmoji = DiscordEmoji.FromName(ctx.Client, ":bone:");

            int xCount = 0;
            int bCount = 0;
            int boneCount = 0;

            await sentMessage.CreateReactionAsync(xEmoji);
            await sentMessage.CreateReactionAsync(bEmoji);
            await sentMessage.CreateReactionAsync(boneEmoji);

            while (true)
            {
                try
                {
                    var reactionResult = await ctx.Client.GetInteractivity()
                        .WaitForReactionAsync(x => x.Message == sentMessage && !x.User.IsBot);
                    
                    if(reactionResult.TimedOut) continue;

                    if (reactionResult.Result.Emoji == xEmoji)
                    {
                        xCount++;
                        await ctx.Channel.SendMessageAsync($"{reactionResult.Result.Emoji}: {xCount.ToString()}");
                        continue;
                    }

                    if (reactionResult.Result.Emoji == bEmoji)
                    {
                        bCount++;
                        await ctx.Channel.SendMessageAsync($"{reactionResult.Result.Emoji}: {bCount.ToString()}");
                        continue;
                    }

                    if (reactionResult.Result.Emoji == boneEmoji)
                    {
                        boneCount++;
                        await ctx.Channel.SendMessageAsync($"{reactionResult.Result.Emoji}: {boneCount.ToString()}");
                        continue;
                    }

                    await ctx.Channel.SendMessageAsync($"Deleting Reaction: {reactionResult.Result.Emoji}");
                    await sentMessage.DeleteReactionsEmojiAsync(reactionResult.Result.Emoji);
                }
                catch (Exception e)
                {
                    ctx.Client.DebugLogger.LogMessage(LogLevel.Critical, "SudoBot", "Error in Reaction Collector", DateTime.Now, e);
                }
                
            }
        }
    }
}