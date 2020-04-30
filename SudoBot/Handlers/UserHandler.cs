using System;
using System.Collections.Generic;
using System.Linq;
using DSharpPlus.Entities;
using SudoBot.DataInterfaces;
using DateTime = System.DateTime;

namespace SudoBot.Handlers
{
    public class UserHandler
    {
        public static List<User> Users = new List<User>();

        public bool AddCountedMessages(User user, DiscordMessage message)
        {
            if (user.Blocked) return false;
            
            TimeSpan minDelay = TimeSpan.FromMinutes(0.1);

            if (DateTimeOffset.Now - minDelay <= user.LastUpdated)
            {
                return false;
            }

            int countedMessageLength = 100;
            int count = (message.Content.Length + countedMessageLength)/countedMessageLength;

            user.LastUpdated = DateTime.Now;
            user.CountedMessages += count;
            
            return true;
        }

        public static User GetOrCreateUser(DiscordMember member)
        {
            User user = Users.FirstOrDefault(x => x.UserId == member.Id);

            if (user is null)
            {
                User fresh =  GetFreshUser(member);
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