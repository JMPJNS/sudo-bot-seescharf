using System;
using System.Threading.Tasks;

namespace SudoBot
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Starting!");
            Globals.CdnKey = Environment.GetEnvironmentVariable("CDN_API_KEY");
            var bot = new Bot();
            bot.RunAsync().GetAwaiter().GetResult();
            // Task.Delay(-1).GetAwaiter().GetResult();
        }
    }
}