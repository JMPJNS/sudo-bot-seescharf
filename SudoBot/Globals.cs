using System;
using System.Collections.Generic;
using DSharpPlus;

namespace SudoBot
{
    public class Globals
    {
        public static readonly string[] ModRoles = {"SudoBotAdmin", "SudoBotMod", "Admins", "Mods", "Admin 👑", "Server-Techniker🛠️", "Bot Developer"};
        public static readonly string[] AdminRoles = {"SudoBotAdmin", "Admins", "Senior Moderatoren ✨", "Admin 👑", "Server-Techniker🛠️", "Moderatoren ✨", "Bot Developer"};
        public static DebugLogger Logger;
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