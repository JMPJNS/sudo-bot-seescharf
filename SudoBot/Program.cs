using System;
using System.Threading.Tasks;

namespace SudoBot
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Starting!");
            var bot = new Bot();
            bot.RunAsync().GetAwaiter().GetResult();
            // Task.Delay(-1).GetAwaiter().GetResult();
        }
    }
}