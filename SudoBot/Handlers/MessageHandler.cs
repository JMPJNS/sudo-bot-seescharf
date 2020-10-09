using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using SudoBot.Models;

namespace SudoBot.Handlers
{
    public class MessageHandler
    {
        public async Task HandleMessage(MessageCreateEventArgs args)
        {
            if (args.Author.IsBot) return;
            
            DiscordMember member = await args.Guild.GetMemberAsync(args.Author.Id);
            User user = await User.GetOrCreateUser(member);

            if (user.UserName != member.Username || user.Discriminator != member.Discriminator)
            {
                await user.UpdateUser(member);
            }

            var ignoredChannels = new List<ulong>() { 279942475152359425, 709596534768926761 };

            if (!ignoredChannels.Contains(args.Message.Channel.Id))
            {
                await user.AddCountedMessages(args.Message);
            }

        }
    }
}