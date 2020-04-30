using System;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using SudoBot.DataInterfaces;

namespace SudoBot.Handlers
{
    public class MessageHandler
    {
        private UserHandler _userHandler = new UserHandler();
        public async Task HandleMessage(DiscordMessage message, DiscordUser author, DiscordGuild guild)
        {
            if (author.IsBot) return;
            
            DiscordMember member = await guild.GetMemberAsync(author.Id);
            User user = UserHandler.GetOrCreateUser(member);
            
            bool didCountMessage = _userHandler.AddCountedMessages(user, message);
            
        }
    }
}