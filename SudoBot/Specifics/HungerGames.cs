using System;
using System.Collections.Generic;

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
                new List<bool>{true}),
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
        };
        
        public List<List<String>> Squads = new List<List<string>>();
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

        public HungerGamesLine(String line, List<Boolean> dies, int rarity = 1, Boolean nameTwice = false, Boolean makesSquad = false, Boolean killsSquad = false)
        {
            Line = line;
            Dies = dies;
            Rarity = rarity;
            NameTwice = nameTwice;
        }
    }
}