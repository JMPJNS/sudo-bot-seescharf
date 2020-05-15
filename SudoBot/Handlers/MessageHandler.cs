using System;
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
            
            bool didCountMessage = await user.AddCountedMessages(args.Message);

        }
    }
}