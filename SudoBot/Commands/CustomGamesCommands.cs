using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using SudoBot.Attributes;
using SudoBot.Database;
using SudoBot.Models;
using SudoBot.Handlers;

namespace SudoBot.Commands
{
    public class CustomGamesCommands: BaseCommandModule
    {
        [CheckForPermissions(SudoPermission.Admin, GuildPermission.CustomGames)]
        [Command("setCustomsRole")]
        public async Task SetCustomsRole(CommandContext ctx, DiscordRole role)
        {
            var guild = await Mongo.Instance.GetGuild(ctx.Guild.Id);
            await guild.SetCustomsRole(role.Id);
            await ctx.Channel.SendMessageAsync("Die Rolle wurde gesetzt");
        }

        [CheckForPermissions(SudoPermission.Mod, GuildPermission.CustomGames)]
        [Command("removeAllCustomsRole")]
        public async Task RemoveAllCustomsRole(CommandContext ctx)
        {
            var guild = await Mongo.Instance.GetGuild(ctx.Guild.Id);
            await ctx.Channel.SendMessageAsync("Die Rolle wird von allen entfernt. Wird ein paar Sekunden dauern.");
            await guild.RemoveAllCustomsRole(ctx);
            await ctx.Channel.SendMessageAsync("Die Rolle wurde von allen entfernt!");
        }
        
        [CheckForPermissions(SudoPermission.Mod, GuildPermission.CustomGames)]
        [Command("createCustoms")]
        public async Task CreateCustoms(CommandContext ctx, string title, string message, int maxMembers, DiscordEmoji tempJoinEmoji, bool useTicket = false)
        {
            //TODO add Timeout Stuff so it can't run forever!, Only Allowed Guilds can do forever
            var guild = await Mongo.Instance.GetGuild(ctx.Guild.Id);

            if (guild.CustomsRole == 0)
            {
                await ctx.Channel.SendMessageAsync("Definiere eine Custom Games Rolle mit: $setCustomsRole {@Rolle}");
                return;
            }

            DiscordEmoji joinEmoji;
            try
            {
                joinEmoji = DiscordEmoji.FromName(ctx.Client, $":{tempJoinEmoji.Name}:");
            }
            catch (Exception e)
            {
                joinEmoji = DiscordEmoji.FromName(ctx.Client, ":white_check_mark:");
            }
            var startEmoji = DiscordEmoji.FromName(ctx.Client, ":cyclone:");
            var cancleEmoji = DiscordEmoji.FromName(ctx.Client, ":no_entry_sign:");

            var embed = new DiscordEmbedBuilder()
                .WithTitle(title)
                .WithColor(DiscordColor.Blue)
                .WithDescription(message);

            // embed.AddField("Beigetreten", "0");
            embed.AddField("Reagiere auf diese Nachricht!", $"mit {joinEmoji}");
            
            List<ulong> joinedUsers = new List<ulong>();

            var sentMessage = await ctx.Channel.SendMessageAsync(embed:embed.Build());
            

            await sentMessage.CreateReactionAsync(joinEmoji);
            // await sentMessage.CreateReactionAsync(cancleEmoji);
            // await sentMessage.CreateReactionAsync(startEmoji);

            while (true)
            {
                try
                {
                    var reactionResult = await ctx.Client.GetInteractivity()
                        .WaitForReactionAsync(x => x.Message == sentMessage && !x.User.IsBot);
                    
                    if(reactionResult.TimedOut) continue;

                    if (reactionResult.Result.Emoji == joinEmoji)
                    {
                        if (joinedUsers.Contains(reactionResult.Result.User.Id))
                        {
                            // await ctx.Channel.SendMessageAsync($"{member.Mention} hat verlassen");

                            // var u = joinedUsers.Single(x => x == reactionResult.Result.User.Id);
                            //
                            // joinedUsers.Remove(u);
                        }
                        else
                        {
                            // if (useTicket && user.TicketsRemaining == 0)
                            // {
                            //     await ctx.Channel.SendMessageAsync($"{member.Mention} hat keine Tickets übrig!");
                            //     continue;
                            // }
                            // await ctx.Channel.SendMessageAsync($"{member.Mention} ist Beigetreten");
                            joinedUsers.Add(reactionResult.Result.User.Id);
                        }
                        
                        // await sentMessage.DeleteReactionAsync(reactionResult.Result.Emoji, reactionResult.Result.User);
                        
                        embed.Fields[0].Value = joinedUsers.Count.ToString();
                        // sentMessage.ModifyAsync(embed: embed.Build());
                        
                        // if (useTicket) await user.RemoveTicket();
                        continue;
                    }

                    if (reactionResult.Result.Emoji == cancleEmoji)
                    {
                        DiscordMember member = await sentMessage.Channel.Guild.GetMemberAsync(reactionResult.Result.User.Id);
                        if ((member.PermissionsIn(ctx.Channel) & Permissions.ManageMessages) == 0)
                        {
                            sentMessage.DeleteReactionAsync(reactionResult.Result.Emoji, reactionResult.Result.User);
                            continue;
                        }

                        await ctx.Channel.SendMessageAsync($"Das Custom Game wurde von {reactionResult.Result.User.Mention} Abgebrochen!");
                        return;
                    }

                    if (reactionResult.Result.Emoji == startEmoji)
                    {
                        DiscordMember member = await sentMessage.Channel.Guild.GetMemberAsync(reactionResult.Result.User.Id);
                        if ((member.PermissionsIn(ctx.Channel) & Permissions.ManageMessages) == 0)
                        {
                            sentMessage.DeleteReactionAsync(reactionResult.Result.Emoji, reactionResult.Result.User);
                            continue;
                        }
                        
                        var role = ctx.Guild.GetRole(guild.CustomsRole);
                        var rUsers = joinedUsers.Shuffle();

                        string users = "";

                        if (rUsers.Count < maxMembers) maxMembers = rUsers.Count;

                        for (int i = 0; i < maxMembers; i++)
                        {
                            try
                            {
                                var m = await sentMessage.Channel.Guild.GetMemberAsync(rUsers[i]);
                                users += $"{m.Mention}, ";
                                await m.GrantRoleAsync(role);
                            }
                            catch (Exception e)
                            {
                                await ctx.Channel.SendMessageAsync($"Es ist ein Fehler aufgetreten bei Member {rUsers[i]}");
                                if (e.Message == "Unauthorized: 403")
                                {
                                    await ctx.Channel.SendMessageAsync($"Der Bot Hat keine Berechtigung die Rolle {role.Mention} zu Vergeben!");
                                    return;
                                }
                            }
                            
                        }

                        users = users.Substring(0, users.Length - 2);
                        if (maxMembers > 1)
                        {
                            users += " haben die Rolle Gekriegt!";
                        } else if (maxMembers == 1)
                        {
                            users += " hat die Rolle Gekriegt!"; 
                        }
                        else
                        {
                            users += "Keiner hat die Rolle Gekriegt!"; 
                        }

                        await ctx.Channel.SendMessageAsync(users);
                        
                        return;
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