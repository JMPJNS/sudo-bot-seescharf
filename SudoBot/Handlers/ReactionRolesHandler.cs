using System;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using SudoBot.Models;

namespace SudoBot.Handlers
{
    public class ReactionRolesHandler
    {
        public async Task HandleReactionAdded(MessageReactionAddEventArgs args)
        {
            if (args.User.IsBot) return;
            
            DiscordMember member = await args.Guild.GetMemberAsync(args.User.Id);
            var guild = await Guild.GetGuild(args.Guild.Id);

            foreach (var role in guild.ReactionRoles)
            {
                if (args.Message.Id == role.MessageId && args.Channel.Id == role.ChannelId &&
                    args.Emoji.Id == role.EmojiId)
                {
                    try
                    {
                        var drole = args.Guild.GetRole(role.RoleId);
                        await member.GrantRoleAsync(drole);
                    }
                    catch (Exception e)
                    {
                        await args.Message.RespondAsync(e.Message);
                        return;
                    }
                }
            }
        }
        
        public async Task HandleReactionRemoved(MessageReactionRemoveEventArgs args)
        {
            if (args.User.IsBot) return;
            
            DiscordMember member = await args.Guild.GetMemberAsync(args.User.Id);
            var guild = await Guild.GetGuild(args.Guild.Id);

            foreach (var role in guild.ReactionRoles)
            {
                if (args.Message.Id == role.MessageId && args.Channel.Id == role.ChannelId &&
                    args.Emoji.Id == role.EmojiId)
                {
                    try
                    {
                        var drole = args.Guild.GetRole(role.RoleId);
                        await member.RevokeRoleAsync(drole);
                    }
                    catch (Exception e)
                    {
                        await args.Message.RespondAsync(e.Message);
                        return;
                    }
                }
            }
        }
    }
}