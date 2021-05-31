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
            new HungerGamesLine("(...) sucht nach einem Tomaten Drop.", new List<bool>{false}),
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
            new HungerGamesLine("(...) hat die Pistole falsch rum gehalten.", new List<bool>{true}),
            new HungerGamesLine("(...) hat bekanntschaft mit dem Bann-Hammer gemacht.", new List<bool>{true}),
            new HungerGamesLine("Ein Gewitter scheint aufzuziehen...  (...) kam nicht schnell genug vom Baum herunter und wurde von einem Blitz getroffen.", new List<bool>{true}),
            new HungerGamesLine("Autsch! (...) wird von einem Rudel wilder Hunde angegriffen und kann sich nicht verteidigen... Das war es wohl...", new List<bool>{true}),
            new HungerGamesLine("Ups! Die Beeren die (...) gegessen hat waren doch keine Erdbeeren :grimacing:", new List<bool>{true}),
            new HungerGamesLine("Fischen ist wichtig zum überleben. Blöd nur, dass (...) dabei von einem Grizzlybär geangelt wird.", new List<bool>{true}),
            new HungerGamesLine("Beim Entzünden des Lagerfeuers fängt die Kleidung von (...)  Feuer. (...) nutzt das aus und kann ihn überwältigen.", new List<bool>{true, false}),
            new HungerGamesLine("Das sieht nicht gut aus! Der Ast auf dem sich (...) befindet bricht ab und fällt genau in einen Ameisenhaufen voller Tödlicher Ameisen.", new List<bool>{true}),
            new HungerGamesLine("Eine Tomatenbombe platzt in der Mitte der Arena. Beim Versuch, so viele Tomaten wie möglich zu ergattern löst (...) einige Sprengfallen aus. Auch (...) wird von der Explosion erwischt und stirbt.", new List<bool>{true, true}),
            new HungerGamesLine("Kampfgeschrei ertönt als (...) und (...) aufeinander zu rennen. Leider achtete keiner der beiden auf den Boden und bemerkte den Treibsand.... Zumindest versinken sie gemeinsam.", new List<bool>{true, true}),
            new HungerGamesLine("Ein stickiger Nebel zieht auf! (...) ist zu langsam und erstickt. (...) schafft es gerade so zu überleben, fällt allerdings in einen tiefen Sumpf und schafft es nicht mehr heraus. (...) lacht sich darüber Schlapp, wird aber von einer Falle erwischt.", new List<bool>{true, true, true}),
            new HungerGamesLine("Ein stickiger Nebel zieht auf! (...) ist zu langsam und erstickt. (...) schafft es gerade so zu überleben. (...) lacht sich darüber Schlapp, wird aber von einer Falle erwischt.", new List<bool>{true, false, true}),
            new HungerGamesLine("(...) ist das Messer aus der Hand gefallen.", new List<bool>{true}),
            new HungerGamesLine("(...) hat sich früh morgens den Fuß gestoßen ... **Autsch**", new List<bool>{false}),
            new HungerGamesLine("(...) schlägt mit seiner Gürtelschnalle durch die Gegend, da keiner in der Nähe war ist das ziemlich fragwürdig.", new List<bool>{false}),
            new HungerGamesLine("(...) schlägt mit seiner Gürtelschnalle durch die Gegend und trifft dabei (...).", new List<bool>{false, true}),
            new HungerGamesLine("(...) wurde von HeyStan gebannt o.O", new List<bool>{true}),
            new HungerGamesLine("(...) wurde von einem Panda überrollt.", new List<bool>{true}),
            new HungerGamesLine("(...) ging die Kraft nach einem langen und erbitterten Kampf mit (...) aus.", new List<bool>{true, false}),
            new HungerGamesLine("(...) ging die Kraft nach einem langen und erbitterten Kampf mit einem wilden Panda aus.", new List<bool>{true}),
            new HungerGamesLine("(...) wurde von einem wilden Pokemon überwältigt.", new List<bool>{true}),
            new HungerGamesLine("(...) wurde von einem wilden Pokemon überrascht, dieses verschont ihn aber.", new List<bool>{false}),
            new HungerGamesLine("(...) findet eine Axt und fällt damit den Baum auf dem sich (...) befindet, er fällt dadurch in den Tot.", new List<bool>{false, true}),
            new HungerGamesLine("(...) schreit so laut durch den Wald, dass er vom Anti-Spam Bot gebannt wird.", new List<bool>{true}),
            new HungerGamesLine("(...) trifft auf ein wildes Celebi und wird von Blättersturm umhüllt und verschlungen.", new List<bool>{true}, rarity: 2),
            new HungerGamesLine("(...) trifft auf ein wildes Celebi und wird von Blättersturm umhüllt, doch kann dem gerade noch so ausweichen.", new List<bool>{false}, rarity: 2),
            new HungerGamesLine("(...) trifft auf ein wildes Celebi und wird von Blättersturm umhüllt, doch kann dem gerade noch so ausweichen. Allerdings kommt Yodajonasde, der ein riesen Celebi Fanboy ist und schubst ihn wieder in den Radius der Attacke. Feels Bad man #CelebiFTW", new List<bool>{true}, rarity: 3),
            new HungerGamesLine("(...) trifft auf ein wildes Aquana und und wird mit Hydropumpe attackiert.", new List<bool>{true}, rarity: 2),
            new HungerGamesLine("(...) trifft auf ein wildes Aquana und wird von Hydropumpe getroffen. Der Strahl schießt (...) aus der Arena. Bei Team Rocket würde man sagen 'Das war mal wieder ein Schuss in den Opfeeeeeen'", new List<bool>{true, true}, nameTwice: true, rarity: 2),
            new HungerGamesLine("(...) trifft auf ein wildes Aquana und kann der Hydropumpe gerade noch so ausweichen. ", new List<bool>{false}, rarity: 2),
            new HungerGamesLine("(...) trifft auf ein wildes Lavados und wird von Ausbrennen getroffen. Da schwindet der letzte Schimmer Hoffnung dahin.", new List<bool>{true}, rarity: 2),
            new HungerGamesLine("(...) trifft auf ein wildes Arktos und wird durch Blizzard verletzt. (...) erliegt an den Wunden.", new List<bool>{true, true}, nameTwice: true, rarity: 2),
            new HungerGamesLine("(...) trifft auf ein wildes Arktos und wird durch Blizzard verletzt. (...) hatte zum Glück das richtige Wissen um sich selbst zu verarzten. Puh. Nochmal Glück gehabt.", new List<bool>{false, false}, nameTwice: true, rarity: 2),
            new HungerGamesLine("(...) trifft auf ein wildes Zapdos. Das scheint gar nicht gut gelaunt zu sein. Er wird durch Ladungsstoß ins jenseits befördert. Auch (...), der zufällig in der Nähe war, wird davon getroffen.", new List<bool>{true, true}, rarity: 2),
            new HungerGamesLine("(...) trifft auf ein wildes Glurak. Glurak setzt Flügelschlag ein. Das scheint nicht gerade sehr effektiv zu sein.", new List<bool>{false}, rarity: 2),
            new HungerGamesLine("(...) trifft auf ein wildes Turtok, das versucht ihn mit Aquaknarre zu treffen. Er wurde dadurch getroffen und wegen seiner Zielkünste hat es dabei noch (...), (...) und (...) mitgenommen.", new List<bool>{true, true, true, true}, rarity: 2),
            new HungerGamesLine("(...) trifft auf ein wildes Turtok, das versucht ihn mit Aquaknarre zu treffen. Es hat heute wohl kein Zielwasser getrunken. Da hat man nochmal Glück gehabt.", new List<bool>{false}, rarity: 2),
            new HungerGamesLine("(...) will wohl gerade zu hoch hinaus. Als er versucht hatte die Gegend zu erkunden, und auf einen Berg zu klettern, wird er durch ein wildes Dragoran überrascht welches ihm mit Drachenrute kurzen Prozess machte.", new List<bool>{true}, rarity: 2),
            new HungerGamesLine("(...), (...) und (...) treffen rein zufällig bei einem See gleichzeitig auf ein Glaziola. Das scheint ihm nicht so zu gefallen und es will sein Revier verteidigen. Alle 3 werden durch die Attacke Blizzard eingefroren und zu Pulverschnee verarbeitet.", new List<bool>{true, true, true}, rarity: 2),
            new HungerGamesLine("Ehe (...) sich versieht begegnet er einem Evoli und wird durch Bezirzer verzaubert. Dies hat jedoch keine große Wirkung.", new List<bool>{false}, rarity: 2),
            new HungerGamesLine("Ehe (...) sich versieht begegnet er einem Evoli und wird durch Bezirzer verzaubert. Das wars dann wohl. (...) ist dadurch kampfunfähig und scheidet somit aus. Naja vielleicht wurde dabei ja die große Liebe gefunden.", new List<bool>{true, true}, nameTwice: true, rarity: 2),
            new HungerGamesLine("Ein wildes Mewtu erscheint. (...), (...) und (...) werden von seiner Attacke Psychokinese getroffen. (...) nutzt das aus und besiegelt ihr Schicksal.", new List<bool>{true, true, true, false}, rarity: 2),
            new HungerGamesLine("(...) trifft auf Herobrine. Nach einem langen Kampf kommt er gerade noch mit einer Enderperle davon", new List<bool>{false}, rarity: 3),
            new HungerGamesLine("(...) trifft auf Herobrine. Nach einem langen Kampf versucht er davon zu laufen, schafft dies aber nicht", new List<bool>{true}, rarity: 3),
            new HungerGamesLine("(...) holt sich ein Eis. (...) klaut sich das Eis aber wird dabei mit einer Pistole erschossen.", new List<bool>{false, true}),
            //TODO multiple players support (.1.), (.2.)...
            // new HungerGamesLine("(...) hat Streit mit (...). (...) will helfen, wird aber reingelegt, und fällt in ein Loch, dass (...) gebaut hat und stirbt. (...) und (...) haben sich in der Zwischenzeit wieder vertragen.
            // new HungerGamesLine("(...) hat Streit mit (...). (...) will helfen, wird aber reingelegt, und fällt in ein Loch, dass (...) gebaut hat und stirbt. (...) und (...) meinten anschließend, dass sie das in einem Schießduell entscheiden müssen. (...)/(...) gewinnt und beendet damit die Zeit von (...)/(...) hier.
            // new HungerGamesLine("(...) hat Streit mit (...). (...) will helfen, wird aber reingelegt, und fällt in ein Loch, dass (...) gebaut hat und stirbt. (...) und (...) meinten anschließend, dass sie das in einem Schießduell entscheiden müssen. (...) und (...) feuern gleichzeitig und sind somit beide raus.
            new HungerGamesLine("(...) wurde beim Versuch zu cheaten erwischt.", new List<bool>{true}),
            new HungerGamesLine("(...) hat so lange gewartet, dass ein neue Runde Hungergames beginnt sodass er, nach dem er sich so gefreut hatte, nicht darauf geachtet hatte wo er gerade hin läuft. Naja. Der Bär hat nun eine noch heftigere Killstreak.", new List<bool>{true}),
            new HungerGamesLine("(...) wird vom Wolf gefressen, der gerade auf der Suche nach Rotkäppchen war. Später erschießt der Jäger den Wolf und befreit (...) wieder. POG Jäger", new List<bool>{false, false}, nameTwice: true),
            new HungerGamesLine("(...) wurde von einem AMG platt gefahren", new List<bool>{true}),
            new HungerGamesLine("(...) wird beim suchen nach dem legendären Tomaten Drop von einem Unbekannten im Gebüsch erledigt.", new List<bool>{true}),
            new HungerGamesLine("(...) wird beim öffnen eines Tomaten Drops von (...) überfallen, jedoch werden beide vom allgegenwärtigen Panda vernichtet. (...) schnappt sich die Ausrüstung.", new List<bool>{true, true, false}),
            new HungerGamesLine("(...) ist nach einer langen HG-Runde an Altersschwäche gestorben. (...), (...) und (...) lachen ihn aus, bevor (...) vom Anticheatbot einen Teamingbann bekommt und die anderen knapp entkommen.", new List<bool>{true, false, false, false, true}, nameTwice: true),
            new HungerGamesLine("(...) buggt in der Luft fest und wird vom Server gekickt.", new List<bool>{true}),
            new HungerGamesLine("(...) erhält einen TomatenDrop und wurde kurz nach dem Öffnen, durch einen Headshot von (...), weggesniped.", new List<bool>{true, false}),
            new HungerGamesLine("(...) wurde durch den Stein, den (...) geworfen hat, schwer verletzt.", new List<bool>{false, false}),
            new HungerGamesLine("(...) versteckt sich in einer Höhle, (...) kriegt das mit und schießt eine Rakete rein.", new List<bool>{true, false}),
            new HungerGamesLine("(...) wollte Dynamit auf (...) werfen, aber hat vergessen los zu lassen.", new List<bool>{true, false}),
            new HungerGamesLine("(...) wurde von einem fliegenden Fisch erschlagen.", new List<bool>{true}),
            new HungerGamesLine("(...) hat die Mods nach !drops gefragt und wurde gebannt.", new List<bool>{true}),
            new HungerGamesLine("(...) stirbt durch einen herabfallenden Amboss.", new List<bool>{true}),
            new HungerGamesLine("(...) wollte aus der Arena fliehen. Die Schutzmechanismen der Arena waren wohl stärker als er.", new List<bool>{true}),
            new HungerGamesLine("(...) will (...) mit einer Briefbombe eliminieren. Leider hat er zu wenig Porto auf den Brief geklebt, weswegen der Brief zu ihm zurück kommt. Er öffnet den Brief ohne darüber nachzudenken und wird von der Bombe getötet.", new List<bool>{false, true}),
            new HungerGamesLine("(...) gönnt sich eine Pause, passt aber nicht auf, und schläft ein. (...) nutzt dieses und entlässt ihn in die unendliche Welt des Schlafs.", new List<bool>{true, false}),
            new HungerGamesLine("(...) läuft ins hohe Gras und trifft auf eine Horde von Taubsis ... die Schnäbel hatten einiges zu picken.", new List<bool>{true}),
            new HungerGamesLine("(...) versucht einem wilden Hunden zu entkommen. Als er auf einen Baum geklettert ist wird er leider von fallenden Pandas erschlagen, die genüsslich Bambus neben ihm gegessen hatten. Beim herunter fallen nehmen sie fast noch (...) mit, aber der konnte ganz knapp ausweichen.", new List<bool>{true, false}),
            new HungerGamesLine("(...) versucht einem wilden Hunden zu entkommen. Als er auf einen Baum geklettert ist versucht der Hund ihm hinter her zu kommen, aber schafft es nicht. Das war knapp. Aber gerade noch so gerettet.", new List<bool>{false}),
            new HungerGamesLine("(...) backt für (...) einen Kuchen. Dieser nimmt ihn dankend an. (...) nimmt einen Bissen und stirbt durch das beigemischte Gift", new List<bool>{false, false, true}, nameTwice: true),
            // new HungerGamesLine("(...) backt für (...) einen Kuchen. Dieser nimmt ihn dankend an. (...) isst den Kuchen und geht satt und unbeschadet erstmal ein Nickerchen machen.
            new HungerGamesLine("(...) backt für (...) einen Kuchen. Dieser nimmt ihn dankend an. Er versucht noch vor Ort einen Bissen zu essen. Durch die Bombe im Kuchen werden beide in die Luft gesprengt.", new List<bool>{true, true}),
            //TODO multiple players support...
            // new HungerGamesLine("(...) und (...) werfen sich mit Tomaten ab, aber bei (...) war ein Stein drin und trift (...) lebensgefährlich am Kopf.
            new HungerGamesLine("(...) geht schwimmen obwohl er nicht schwimmen kann.", new List<bool>{true}),
            new HungerGamesLine("(...) denkt das hier ist Minecraft, schlägt den Baum mit der Faust und verletzt dabei seine Hand.", new List<bool>{false}),
            new HungerGamesLine("(...) wurde zu tarzan und springt von Baum zu Baum.", new List<bool>{false}),
            new HungerGamesLine("(...) fällt von einen Baum und fällt dabei auf (...).", new List<bool>{true, true}),
            new HungerGamesLine("(...) und (...) haben Spaß am Rande der Arena und bewerfen sich mit Tomaten.", new List<bool>{false, false}),
            new HungerGamesLine("(...) verwechselt die Hungergames mit Djungelcamp und isst eine giftige Tomate. Im Djungelcamp hättest du dafür wenigstens Punkte bekommen. Feels Bad Man", new List<bool>{true}),
            // new HungerGamesLine("(...) ist der Armor und beschießt (...) und (...) mit einem Liebespfeil. Leider hat er zu spät bemerkt, dass es kein Liebespfeil war, sondern ein ganz normaler Pfeil und erschießt deswegen beide. (...) tut das so Leid, dass er sich selbst auch umbringt
            // new HungerGamesLine("(...) öffnet eine Truhe, findet darin eine Scar und jagt die folgenden Gegner (...), (...) und (...) hinterher. (...) kletterte einen Baum hoch und kann sich retten, (...) findet einen Busch und konnte sich in dem verstecken, doch (...) lief dummerweise in eine Sackgasse und wurde erschossen. Für mehr hat es auch nicht gereicht, da abschließend auch die Munition von (...) leer war. Puh. Da haben die anderen nochmal Glück gehabt.
        };
        
        public List<HungerGamesPlayer> PlayersAlive = new List<HungerGamesPlayer>();
        
        public List<List<HungerGamesPlayer>> Squads = new List<List<HungerGamesPlayer>>();

        private int _day = 1;
        private int _night = 0;
        private bool _isDay = true;

        private int currentCycleEventCount = 0;

        private bool _firstRun = true;

        public DiscordMember Winner;
        public string WinnerName;

        // Returns true if game is over
        public async Task<Boolean> RunCycle(DiscordChannel channel, Guild guild, bool debug = false)
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

                    int count = _rng.Next(150, 400);
                    await user.AddSpecialPoints(count);
                    
                    await channel.SendMessageAsync($"**{p.Mention}** hat gewonnen und erhält {count} Bonus {guild.RankingPointName}!");
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
                emb.Description = $"{PlayersAlive.Count} Spieler bereiten sich vor.";
                if (!debug) await channel.SendMessageAsync(embed: emb.Build());
                else Console.WriteLine(emb.Title);
                _firstRun = false;
            }
            
            if (currentCycleEventCount > 3 && _rng.Next(0, 6) == 5)
            {
                var emb = new DiscordEmbedBuilder();
                if (_isDay)
                {
                    _night += 1;
                    emb.Title = $"Die {_night} Nacht ist angebrochen.";
                }
                else
                {
                    _day += 1;
                    emb.Title = $"Der {_day} Tag hat begonnen.";
                }
                _isDay = !_isDay;

                emb.Description = $"Es sind noch {PlayersAlive.Count} Spieler am Leben.";
                
                if (!debug) await channel.SendMessageAsync(embed: emb.Build());
                else Console.WriteLine(emb.Title);

                currentCycleEventCount = 0;
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
            
            if (rolled.UseCount > 1)
            {
                int t = 100 / rolled.UseCount;
                var r = _rng.Next(0, 100);
                if (r > t)
                {
                    return false;
                }
            }

            if (rolled.UseCount <= 5)
                rolled.UseCount++;

            var filled = await ExecuteRolled(rolled);
            if (!debug) await channel.SendMessageAsync(filled);
            else Console.WriteLine(filled);
            
            currentCycleEventCount++;
            
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

            // to avoid enumarable was modified error in foreach loop
            List<List<HungerGamesPlayer>> deadSquads = new();

            foreach (var squad in Squads)
            {
                foreach (var m in squad)
                {
                    if (dies.Any(x => x == m))
                    {
                        deadSquads.Add(squad);
                    }
                }
            }

            foreach (var squad in deadSquads)
            {
                Squads.Remove(squad);
            }
                
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

        public int UseCount = 0;

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