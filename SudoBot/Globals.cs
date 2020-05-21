using System;
using System.Collections.Generic;
using DSharpPlus;
using DSharpPlus.Entities;

namespace SudoBot
{
    public class Globals
    {
        public static readonly string[] ModRoles = {"SudoBotAdmin", "SudoBotMod", "Admins", "Mods", "Admin 👑", "Server-Techniker🛠️", "Bot Developer"};
        public static readonly string[] AdminRoles = {"SudoBotAdmin", "Admins", "Senior Moderatoren ✨", "Admin 👑", "Server-Techniker🛠️", "Moderatoren ✨", "Bot Developer"};
        public static DebugLogger Logger;
        public static DiscordClient Client;
        public static DiscordChannel LogChannel
        {
            get
            {
                if (Environment.GetEnvironmentVariable("DBNAME") == "SudoBot")
                {
                    var guild = Client.GetGuildAsync(468835109844418575).GetAwaiter().GetResult();
                    return guild.GetChannel(712253992243036281);
                }
                else
                {
                    var guild = Client.GetGuildAsync(468835109844418575).GetAwaiter().GetResult();
                    return guild.GetChannel(712254249148350495);
                }
            }
        }
    }
}

public static class IListExtensions
{
    public static IList<T> Shuffle<T>(this IList<T> list)
    {
        var rng = new Random();
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }

        return list;
    }
}