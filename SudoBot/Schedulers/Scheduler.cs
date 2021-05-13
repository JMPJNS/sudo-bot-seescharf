using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;
using SudoBot.Models;
using SudoBot.Specifics;

namespace SudoBot.Schedulers
{
    public class Scheduler
    {
        public static async Task Execute(Scheduled stuff)
        {
            try
            {
                // Reminders
                if (stuff.Type.Contains(ScheduledType.Reminder))
                {
                    await ReminderScheduler.Execute(stuff);
                }
                
                // HungerGames
                if (stuff.Type.Contains(ScheduledType.HungerGames))
                {
                    await HungerGamesScheduler.Execute(stuff);
                }
            }
            catch (Exception e)
            {
                Globals.Client.Logger.Log(LogLevel.Error,  $"Error in Scheduler {stuff.Id}: {e.Message}", DateTime.Now);
            }
        }
    }
}