using System.Collections.Generic;

namespace SudoBot.Models
{
    public class BotConfig
    {
        public string Name { get; set; }
        public string Token { get; set; }
        public List<string> Prefixes { get; set; }
    }
}