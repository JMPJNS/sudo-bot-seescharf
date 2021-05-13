using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using SudoBot.Models;
using SudoBot.Specifics;

namespace SudoBot.Schedulers
{
    public class HungerGamesScheduler
    {
        public static async Task Execute(Scheduled stuff)
        {
            List<string> requiredKeys = new List<string>{"GuildId", "ChannelId", "MessageId", "MaxPlayers", "UseBots", "WithSudo"};

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
            
            var guild = await Globals.Client.GetGuildAsync(guildId);
            var channel = guild.GetChannel(channelId);
            var message = await channel.GetMessageAsync(messageId);

            var sudoGuild = await Guild.GetGuild(guild.Id);

            var emoji = DiscordEmoji.FromUnicode("🏟");
            var joinedUsers = (await message.GetReactionsAsync(emoji, 9999)).ToList();

            int maxPlayers = Int32.Parse(stuff.Arguments["MaxPlayers"]);
            Boolean useBots = Boolean.Parse(stuff.Arguments["UseBots"]);
            Boolean withSudo = Boolean.Parse(stuff.Arguments["WithSudo"]);

            if (!withSudo)
            {
                var botUsers = joinedUsers.FindAll(x => x.IsBot);
                foreach (var bot in botUsers)
                {
                    joinedUsers.Remove(bot);
                }
            }

            var names = new List<HungerGamesPlayer>();
            foreach (var user in joinedUsers)
            {
                if (names.Count >= maxPlayers) break;
                
                var member = await guild.GetMemberAsync(user.Id);
                if (names.Any(x => x.Name == member.DisplayName))
                {
                    var p = new HungerGamesPlayer();
                    p.Name = member.DisplayName + Globals.RandomString(2);
                    p.Id = member.Id;
                    
                    names.Add(p);
                }
                else
                {
                    var p = new HungerGamesPlayer();
                    p.Name = member.DisplayName;
                    p.Id = member.Id;
                    
                    names.Add(p);
                }
            }

            if (useBots)
            {
                var c = maxPlayers - names.Count;
                for (int i = 1; i <= maxPlayers; i++)
                {
                    var p = new HungerGamesPlayer();
                    p.Name = $"Bot{i}";
                    p.Id = 0;
                    
                    names.Add(p);
                }
            }
            
            var hg = new HungerGames(names);
            try
            {
                while (!await hg.RunCycle(channel, sudoGuild))
                {
                    // Wait for 30 seconds between cycles
                    await Task.Delay(30 * 1000);
                }

                if (hg.Winner != null)
                {
                    // Give Winner XP
                }
            }
            catch (Exception e)
            {
                await channel.SendMessageAsync($"Error: {e.Message}");
            }
            finally
            {
                stuff.Active = false;   
            }
        }
    }
}