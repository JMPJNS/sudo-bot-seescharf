using System;
using System.Collections.Generic;
using System.Linq;
using DSharpPlus.Entities;
using OllekBot.DataInterfaces;

namespace OllekBot.Handlers
{
    public class UserHandler
    {
        public static List<User> Users = new List<User>();

        public bool AddCountedMessage(User user)
        {
            // Pro 100 Zeichen eine extra message, also nachricht von 0-100 = 1 message, 100-200 = 2 messages
            // 1 Minute Cooldown (Mit command einstellbar)
            return true;
        }

        public static User GetOrCreateUser(DiscordMember member)
        {
            if (Users.Count == 0)
            {
                var fresh =  GetFreshUser(member);
                Users.Add(fresh);
                return fresh;
            }
            
            var user = Users.Find(x => x.UserId == member.Id);
            
            
            if (user is null)
            {
                var fresh =  GetFreshUser(member);
                Users.Add(fresh);
                return fresh;
            }
            
            return user;
        }

        private static User GetFreshUser(DiscordMember member)
        {
            return new User(
                member.Id,
                member.Guild.Id,
                member.JoinedAt,
                0,
                0,
                false
            );
        }
    }
}