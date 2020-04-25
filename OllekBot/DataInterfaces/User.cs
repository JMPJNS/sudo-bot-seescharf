using System;
using DSharpPlus.Entities;

namespace OllekBot.DataInterfaces
{
    public class User
    {
        public ulong UserId;
        public ulong GuildId;

        public bool Blocked;
        
        public DateTimeOffset LastUpdated;
        public DateTimeOffset JoinDate;

        public int SpecialPoints;
        
        // Not Real Messages
        public int CountedMessages;
        
        public int CalculatePoints()
        {
            int messages = this.CountedMessages;
            int points = this.SpecialPoints;

            int days = (int)(DateTime.Now - this.JoinDate).TotalDays;

            return messages + points + days*50;
        }

        public User(ulong userId, ulong guildId, DateTimeOffset joinDate, int countedMessages, int specialPoints, bool blocked)
        {
            this.UserId = userId;
            this.GuildId = guildId;
            this.JoinDate = joinDate;

            this.Blocked = blocked;

            this.CountedMessages = countedMessages;
            this.SpecialPoints = specialPoints;
            
            this.LastUpdated = new DateTimeOffset();
        }
    }
}