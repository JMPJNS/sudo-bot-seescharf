using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using SudoBot.Models;
using SudoBot.Handlers;

namespace SudoBot.Commands
{
    public class CustomGamesCommands: BaseCommandModule
    {
        [Command("createCustoms")]
        public async Task CreateCustoms(CommandContext ctx, string title, string message, int maxMembers) 
        {
            var embed = new DiscordEmbedBuilder();
            embed.Title = title;
            embed.Color = DiscordColor.Blue;
            embed.Description = message;

            var usersIds = new List<ulong>();
            
            var sentMessage = await ctx.Channel.SendMessageAsync(embed:embed);
            
            var joinEmoji = DiscordEmoji.FromName(ctx.Client, ":white_check_mark:");
            var startEmoji = DiscordEmoji.FromName(ctx.Client, ":cyclone:");
            var cancleEmoji = DiscordEmoji.FromName(ctx.Client, ":no_entry_sign:");

            await sentMessage.CreateReactionAsync(joinEmoji);
            await sentMessage.CreateReactionAsync(cancleEmoji);
            await sentMessage.CreateReactionAsync(startEmoji);

            while (true)
            {
                try
                {
                    var reactionResult = await ctx.Client.GetInteractivity()
                        .WaitForReactionAsync(x => x.Message == sentMessage && !x.User.IsBot);
                    
                    if(reactionResult.TimedOut) continue;
                    DiscordMember member = await sentMessage.Channel.Guild.GetMemberAsync(reactionResult.Result.User.Id);

                    if (reactionResult.Result.Emoji == joinEmoji)
                    {
                        await ctx.Channel.SendMessageAsync($"{reactionResult.Result.Emoji}");
                        User user = await User.GetOrCreateUser(member);

                        if (usersIds.Contains(user.UserId) || user.TicketsRemaining == 0)
                        {
                            await sentMessage.DeleteReactionAsync(reactionResult.Result.Emoji, reactionResult.Result.User);
                            continue;
                        }
                        
                        usersIds.Add(user.UserId);

                        await user.RemoveTicket();
                        await sentMessage.DeleteReactionAsync(reactionResult.Result.Emoji, reactionResult.Result.User);
                        continue;
                    }

                    if ((member.Guild.Permissions & Permissions.ManageMessages) != 0 && reactionResult.Result.Emoji == cancleEmoji)
                    {
                        await ctx.Channel.SendMessageAsync($"{reactionResult.Result.Emoji}");
                        await ctx.Channel.SendMessageAsync($"Das Custom Game wurde von {reactionResult.Result.User.Mention} Abgebrochen!");
                        return;
                    }

                    if ((member.Guild.Permissions & Permissions.ManageMessages) != 0 && reactionResult.Result.Emoji == startEmoji)
                    {
                        await ctx.Channel.SendMessageAsync($"{reactionResult.Result.Emoji}");

                        var rUsers = usersIds.Shuffle();

                        if (rUsers.Count < maxMembers) maxMembers = rUsers.Count;

                        for (int i = 0; i < maxMembers; i++)
                        {
                            var m = await sentMessage.Channel.Guild.GetMemberAsync(rUsers[i]);
                            // TODO get customgames role from CustomGames Mongo, add Role to User
                        }
                        
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