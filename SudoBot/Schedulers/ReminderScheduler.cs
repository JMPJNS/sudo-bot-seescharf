using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SudoBot.Models;

namespace SudoBot.Schedulers
{
    public class ReminderScheduler
    {
        public static async Task Execute(Scheduled stuff)
        {
            List<string> requiredKeys = new List<string>{"GuildId", "ChannelId", "MessageId", "MemberId", "Message"};

            foreach (var key in requiredKeys)
            {
                if (!stuff.Arguments.ContainsKey(key))
                {
                    Console.WriteLine($"Invalid HungerGames Schedule: {stuff.Id}");
                    return;
                }
            }

            ulong guildId = UInt64.Parse(stuff.Arguments["GuildId"]);
            ulong channelId = UInt64.Parse(stuff.Arguments["ChannelId"]);
            ulong messageId = UInt64.Parse(stuff.Arguments["MessageId"]);
            ulong memberId = UInt64.Parse(stuff.Arguments["MemberId"]);
            
            var guild = await Globals.Client.GetGuildAsync(guildId);
            var channel = guild.GetChannel(channelId);
            var member = await guild.GetMemberAsync(memberId);

            var tz = Environment.OSVersion.Platform == PlatformID.Win32NT ? TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time") : TimeZoneInfo.FindSystemTimeZoneById("Europe/Vienna");
            var time = TimeZoneInfo.ConvertTimeFromUtc(stuff.InsertDate, tz);

            // Prevent Escape to ping people
            if (stuff.Arguments["Message"] == null)
                stuff.Arguments["Message"] = "";
            var message = stuff.Arguments["Message"].Replace("```", "'''");
            var minutes = time.Minute < 10 ? $"0{time.Minute}" : time.Minute.ToString();
            var sendMessage = message != "" ? $" ```{message}```" : "";

            var messageLink = $"https://discord.com/channels/{guildId}/{channelId}/{messageId}";
                        
            await channel.SendMessageAsync(
                $"{member.Mention} am `{time.Day}.{time.Month}.{time.Year} {time.Hour}:{minutes}` {messageLink}" +sendMessage);
            stuff.Active = false;
        }
    }
}