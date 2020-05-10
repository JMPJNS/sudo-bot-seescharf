using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using SudoBot.Database;
using SudoBot.Models;
using SudoBot.Handlers;

namespace SudoBot.Commands
{
    public class CustomGamesCommands: BaseCommandModule
    {
        [RequireRoles(RoleCheckMode.Any, new []{"SudoBotAdmin", "SudoBotMod", "Admins", "Mods"})]
        [Command("setCustomsRole")]
        public async Task SetCustomsRole(CommandContext ctx, DiscordRole role)
        {
            var guild = await MongoCrud.Instance.GetGuild(ctx.Guild.Id);
            await guild.SetCustomsRole(role.Id);
        }
        
        [RequireRoles(RoleCheckMode.Any, new []{"SudoBotAdmin", "SudoBotMod", "Admins", "Mods"})]
        [Command("createCustoms")]
        public async Task CreateCustoms(CommandContext ctx, string title, string message, int maxMembers, bool useTicket = false)
        {
            var guild = await MongoCrud.Instance.GetGuild(ctx.Guild.Id);

            if (guild == null) {Console.WriteLine("ERROR GUILD NOT FOUND"); return;}
            if (guild.CustomsRole == 0)
            {
                await ctx.Channel.SendMessageAsync("Definiere eine Custom Games Rolle mit: $setCustomsRole {@Rolle}");
                return;
            }
            
            await guild.RemoveAllCustomsRole(ctx);

            var embed = new DiscordEmbedBuilder();
            embed.Title = title;
            embed.Color = DiscordColor.Blue;
            embed.Description = message;

            //TODO this probably breaks
            // embed.Fields[0].Name = "Beigetreten";
            // embed.Fields[0].Value = 0.ToString();
            
            List<ulong> joinedUsers = new List<ulong>();

            var sentMessage = await ctx.Channel.SendMessageAsync(embed:embed.Build());
            
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
                        User user = await User.GetOrCreateUser(member);
                        

                        if (joinedUsers.Contains(user.UserId))
                        {
                            await ctx.Channel.SendMessageAsync($"{member.Mention} hat verlassen");

                            var u = joinedUsers.Single(x => x == user.UserId);
                            
                            joinedUsers.Remove(u);
                        }
                        else
                        {
                            if (user.TicketsRemaining == 0)
                            {
                                await ctx.Channel.SendMessageAsync($"{member.Mention} hat keine Tickets übrig!");
                                continue;
                            }
                            await ctx.Channel.SendMessageAsync($"{member.Mention} ist Beigetreten");
                            joinedUsers.Add(user.UserId);
                        }
                        
                        await sentMessage.DeleteReactionAsync(reactionResult.Result.Emoji, reactionResult.Result.User);
                        
                        // embed.Fields[0].Value = joinedUsers.Count.ToString();
                        // await sentMessage.ModifyAsync(embed: embed.Build());
                        
                        await user.RemoveTicket();
                        continue;
                    }

                    if (reactionResult.Result.Emoji == cancleEmoji)
                    {
                        if ((member.PermissionsIn(ctx.Channel) & Permissions.ManageMessages) == 0)
                        {
                            await sentMessage.DeleteReactionAsync(reactionResult.Result.Emoji, reactionResult.Result.User);
                            continue;
                        }
                        
                        await ctx.Channel.SendMessageAsync($"Das Custom Game wurde von {reactionResult.Result.User.Mention} Abgebrochen!");
                        return;
                    }

                    if (reactionResult.Result.Emoji == startEmoji)
                    {
                        if ((member.PermissionsIn(ctx.Channel) & Permissions.ManageMessages) == 0)
                        {
                            await sentMessage.DeleteReactionAsync(reactionResult.Result.Emoji, reactionResult.Result.User);
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