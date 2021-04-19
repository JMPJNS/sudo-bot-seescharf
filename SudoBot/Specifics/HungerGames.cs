using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using SudoBot.Models;

namespace SudoBot.Specifics
{
    public class HungerGames
    {
        public List<HungerGamesLine> Lines = new List<HungerGamesLine>
        {
            new HungerGamesLine("(...) sucht nach einem Tomaten Drop.",
            new List<bool>{false}),
            new HungerGamesLine("(...), (...) und (...) schließen einen nicht Angriffspakt. Der Anticheatbot merkt das und sie werden gebannt.",
            new List<bool>{true, true, true}),
            new HungerGamesLine("(...), (...) und (...) schließen einen nicht Angriffspakt. Der Anticheatbot merkt das **nicht** und sie bilden einen Squad.",
            new List<bool>{false, false, false}, 1, false, true),
            new HungerGamesLine("(...), (...) und (...) kämpfen um einen Tomaten Drop. (...) ergreift es glücklich, erhält Boomstan und jagt sich selbst in die Luft.",
            new List<bool>{false, false, false, true}, 1, true),
            new HungerGamesLine("(...), (...) und (...) kämpfen um einen Tomaten Drop. (...) ergreift es glücklich, erhält Boomstan und jagt alle außer sich selbst in die Luft.",
            new List<bool>{true, true, true, false}, 1, true),
            new HungerGamesLine("(...), (...) und (...) kämpfen um einen Tomaten Drop. Dieser stellt sich als eine Falle heraus und jagt alle in die Luft.",
            new List<bool>{true, true, true}),
            new HungerGamesLine("(...), (...) und (...) kämpfen um einen Tomaten Drop. Dieser stellt sich als eine Falle heraus, geht in die Luft, aber trifft keinen. Puh, Glück gehabt.",
            new List<bool>{false, false, false}),
            new HungerGamesLine("Das Squad mit (...), (...), (...) finden eine 1 Monat Friend of Staff OG Karte. Dadurch entsteht Krieg in der Gruppe und am Ende überlebt nur (...) . Außerdem stellt sich die Karte als Fake heraus. Feels bad man",
                new List<bool>{true, true, true, false}, 1, true, false, true),
            new HungerGamesLine("(...) findet einen Tomaten Drop. Omg! Nicht zu FASSEN! Dieser enthält Yodajonasde´s verschollener blauer RGB-Farbraum. Durch den Glanz dieser Farbenpracht sterben (...), (...) und (...)",
                new List<bool>{false, true, true, true}, 3),
            new HungerGamesLine("(...) wird von einem Streuzauber in eine Grube geblasen und stirbt.",
                new List<bool>{true}),
            new HungerGamesLine("(...) besiegt (...) in einem Duell, aber verschont gütig dessen Leben.",
                new List<bool>{false, false}),
            new HungerGamesLine("(...) fällt von einem Baum und stirbt.",
                new List<bool>{true}),
            new HungerGamesLine("(...) fällt von einem Baum, kann sich allerdings an einem Ast wieder auffangen.",
                new List<bool>{false}),
            new HungerGamesLine("(...) schlafwandelt und stürzt dabei eine Klippe runter. Damit hat nicht mal ein Mathematiker gerechnet.",
                new List<bool>{true}, dayState: HungerGamesDayState.Night),
            new HungerGamesLine("(...) erkundet die Arena.",
                new List<bool>{false}),
            new HungerGamesLine("(...) und (...) feuern gleichzeitig Schüsse aufeinander ab und töten sich gegenseitig.",
                new List<bool>{true, true}),
            new HungerGamesLine("(...) vergleicht die bisherigen Ereignisse des Spiels mit den Events aus Videospielen und ist positiv überrascht.",
                new List<bool>{false}),
            new HungerGamesLine("(...) findet einen Tomaten Drop. Darin befindet sich ein Blitzzauber. (...) wird vom Blitz getroffen.",
                new List<bool>{false, true}, 1, true),
            new HungerGamesLine("(...) findet einen Tomaten Drop. Darin befindet sich ein Blitzzauber. (...) wird vom Blitz getroffen.",
                new List<bool>{false, true}),
            new HungerGamesLine("(...) merkt, dass er Magie beherscht und schickt (...) mit Phantom Blow in den Tod.",
                new List<bool>{false, true}),
            new HungerGamesLine("Der Squad mit (...), (...), (...) geht nach einem Streit wieder auseinander.",
                new List<bool>{false, false, false}, 1, false, false, true),
            new HungerGamesLine("Der Anticheatbot wurde am Ende doch aktiv und (...), (...), (...) flogen in hohem Bogen aus der Runde.",
                new List<bool>{true, true, true}, 1, false, false, true),
            new HungerGamesLine("Nach mehrerem Gemunkel und Angst in der Gruppe mit (...), (...), (...) machte einer den ersten Zug und am Ende blieb nur noch (...) übrig.",
                new List<bool>{true, true, true, false}, 1, false, false, true),
            new HungerGamesLine("(...) verschluckt sich an einem Schlürfsaft.", new List<bool>{true}),
            new HungerGamesLine("(...) hat so gutes Bogen Aim, dass er (...) mit einem Pfeil durchbohrt.", new List<bool>{false, true}),
        };
        
        public List<HungerGamesPlayer> PlayersAlive = new List<HungerGamesPlayer>();
        
        public List<List<HungerGamesPlayer>> Squads = new List<List<HungerGamesPlayer>>();

        private int _day = 1;
        private int _night = 0;
        private bool _isDay = true;

        private bool _firstRun = true;

        public DiscordMember Winner;
        public string WinnerName;

        // Returns true if game is over
        public async Task<Boolean> RunCycle(DiscordChannel channel, bool debug = false)
        {
            if (PlayersAlive.Count <= 0)
            {
                await channel.SendMessageAsync($"Keiner hat gewonnen!");
                return true;
            }

            if (PlayersAlive.Count == 1)
            {
                var winner = PlayersAlive.FirstOrDefault();

                if (debug)
                {
                    Console.WriteLine($"**{winner.Name}** hat gewonnen!");
                    WinnerName = winner.Name;
                    return true;
                }

                DiscordMember p = null;

                try
                {
                    p = await channel.Guild.GetMemberAsync(winner.Id);
                }
                catch (Exception e)
                {
                    //ignore
                }

                if (p != null)
                {
                    Winner = p;
                    WinnerName = p.DisplayName;
                    
                    var user = await User.GetOrCreateUser(p);

                    int count = _rng.Next(250, 1000);
                    await user.AddSpecialPoints(count);
                    
                    await channel.SendMessageAsync($"**{p.Mention}** hat gewonnen!");
                }
                else
                {
                    await channel.SendMessageAsync($"**{winner.Name}** hat gewonnen!");
                    WinnerName = winner.Name;
                }

                return true;
            }
            
            // Change DayState randomly

            if (_firstRun)
            {
                var emb = new DiscordEmbedBuilder();
                emb.Title = "Der 1 Tag hat begonnen.";
                if (!debug) await channel.SendMessageAsync(embed: emb.Build());
                else Console.WriteLine(emb.Title);
                _firstRun = false;
            }
            
            if (_rng.Next(0, 8) == 7)
            {
                var emb = new DiscordEmbedBuilder();
                if (_isDay)
                {
                    _night += 1;
                    emb.Title = $"Die {_night} Nacht hat angebrochen.";
                }
                else
                {
                    _day += 1;
                    emb.Title = $"Der {_day} Tag hat begonnen.";
                }
                _isDay = !_isDay;
                if (!debug) await channel.SendMessageAsync(embed: emb.Build());
                else Console.WriteLine(emb.Title);
            }
            
            // Do the Magic

            HungerGamesLine rolled;
            List<HungerGamesLine> rollable;
            if (PlayersAlive.Count <= 4)
            {
                if (Squads.Count >= 1)
                {
                    rollable = Lines.Where(x => x.KillsSquad == true).ToList();
                }
                else
                {
                    rollable = Lines.Where(x => x.Dies.Count(y => y) <= 1 && x.KillsSquad == false && x.MakesSqud == false).ToList();
                }
            }
            else
            {
                if (Squads.Count >= 2)
                {
                    rollable = Lines.Where(x => x.KillsSquad == true).ToList();
                }
                else if (Squads.Count == 0)
                {
                    rollable = Lines.Where(x => x.KillsSquad == false).ToList();
                }
                else
                {
                    rollable = Lines;
                }
            }

            if (PlayersAlive.Count == 2)
            {
                if (_rng.Next(0, 14) == 7)
                    rollable = Lines.Where(x => x.Dies.Count <= (x.NameTwice ? 3 : 2)).ToList();
            }

            if (_isDay)
            {
                rollable = rollable.Where(x =>
                    x.DayState == HungerGamesDayState.Both || x.DayState == HungerGamesDayState.Day).ToList();
            }
            else
            {
                rollable = rollable.Where(x =>
                    x.DayState == HungerGamesDayState.Both || x.DayState == HungerGamesDayState.Night).ToList();
            }
            
            rolled = rollable[_rng.Next(0, rollable.Count)];

            if (rolled.Rarity > 1)
            {
                int t = 100 / rolled.Rarity;
                var r = _rng.Next(0, 100);
                if (r > t)
                {
                    return false;
                }
            }
            
            var filled = await ExecuteRolled(rolled);
            if (!debug) await channel.SendMessageAsync(filled);
            else Console.WriteLine(filled);
            
            return false;
        }

        private async Task<String> ExecuteRolled(HungerGamesLine line)
        {
            var grabCount = line.Dies.Count;
            if (line.NameTwice) grabCount--;

            List<HungerGamesPlayer> chosen;

            var shuffeled = PlayersAlive.OrderBy(n => Guid.NewGuid()).ToList();

            if (line.KillsSquad)
            {
                var squad = Squads.OrderBy(n => Guid.NewGuid()).ToList()[0];
                
                chosen = squad;
                Squads.Remove(squad);
            }
            else
            {
                chosen = shuffeled.Take(grabCount).ToList();
            }
            
            if (chosen.Count < line.Dies.Count)
            {
                var repeat = chosen[_rng.Next(0, chosen.Count)];
                chosen.Add(repeat);
            }

            List<HungerGamesPlayer> dies = new List<HungerGamesPlayer>();

            for (int i = 0; i < chosen.Count; i++)
            {
                if (line.Dies[i])
                {
                    dies.Add(chosen[i]);
                }
                else
                {
                    if (dies.Contains(chosen[i]))
                    {
                        dies.Remove(chosen[i]);
                    }
                }
            }

            if (line.MakesSqud)
            {
                Squads.Add(chosen.Except(dies).ToList());
            }
            
            if (chosen.Count != line.Dies.Count)
            {
                Console.WriteLine("error");
            }

            PlayersAlive = PlayersAlive.Except(dies).ToList();
                
            var filled = FillNames(line.Line, chosen);
            return filled;
        }
        
        private String FillNames(String line, List<HungerGamesPlayer> players)
        {
            var regex = new Regex(Regex.Escape("(...)"));

            foreach (var player in players)
            {
                line = regex.Replace(line, $"**{player.Name}**", 1);
            }
            
            return line;
        }

        private Random _rng;
        public HungerGames(List<HungerGamesPlayer> players)
        {
            _rng = new Random();
            PlayersAlive = players;
        }
    }

    public class HungerGamesPlayer
    {
        public String Name;
        public ulong Id;
        
        // Add items here, and just return false early from handle step to skip things that need items if nobody has any
    }

    public enum HungerGamesDayState
    {
        Day,
        Night,
        Both
    }

    public class HungerGamesLine
    {
        // (...) gets replaced with Character
        public String Line;
        // List of characters in the line that die, example: `(...) kills (...)`, would be [False, True]
        public List<Boolean> Dies;
        // The higher the number, the lower the chance it is used, example: rarity 2 line gets rolled, 50% chance it gets used, 50% chance it gets rerolled, 66% reroll chance on rarity 3....
        public int Rarity;
        // if 1 of the specified names gets repeated, if the same character dies and not dies, it does whatever comes last
        public Boolean NameTwice;
        // true if the line creates a squad of its members
        public Boolean MakesSqud;
        // true if the line kills its squad, if no squad exists, line gets rerolled
        public Boolean KillsSquad;
        // says if the line is spoken at day / night or both
        public HungerGamesDayState DayState;

        public HungerGamesLine(String line, List<Boolean> dies, int rarity = 1, Boolean nameTwice = false, Boolean makesSquad = false, Boolean killsSquad = false, HungerGamesDayState dayState = HungerGamesDayState.Both)
        {
            Line = line;
            Dies = dies;
            Rarity = rarity;
            NameTwice = nameTwice;
            MakesSqud = makesSquad;
            KillsSquad = killsSquad;
            DayState = dayState;
        }
    }
}