using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.CommandsNext.Entities;
using DSharpPlus.Entities;

namespace SudoBot
{
    public sealed class SudoHelpFormatter : BaseHelpFormatter
    {
        private readonly DefaultHelpFormatter _d;
        private Command Command { get; set; }
        private DiscordEmbedBuilder Eb { get; set; }

        public SudoHelpFormatter(CommandContext ctx)
            : base(ctx)
        {
            this._d = new DefaultHelpFormatter(ctx);
            this.Eb = new DiscordEmbedBuilder().WithTitle("Hilfe").WithColor(DiscordColor.Aquamarine);
        }

        public override BaseHelpFormatter WithCommand(Command command)
        {
            Command = command;

            Eb.WithDescription(Formatter.InlineCode(command.Name) + ": " + (command.Description ?? "Keine Beschreibung vorhanden."));

            if (command is CommandGroup cr && cr.IsExecutableWithoutSubcommands)
                Eb.WithDescription(Eb.Description + "\n\nDies ist eine Ausführbare Command Gruppe, optional einen Subcommand spezifizieren. \n`$help " + cr.Name + " [Subcommand]`");
            
            if (command is CommandGroup commandGroup && !commandGroup.IsExecutableWithoutSubcommands)
                Eb.WithDescription(Eb.Description + "\n\nDies ist eine Command Gruppe, bitte einen Subcommand spezifizieren. \n`$help " + commandGroup.Name + " [Subcommand]`");

            IReadOnlyList<string> aliases = command.Aliases;
            if (aliases != null && aliases.Count > 0)
                Eb.AddField("- Aliases", string.Join(", ", command.Aliases.Select(Formatter.InlineCode)));

            IReadOnlyList<CommandOverload> overloads = command.Overloads;
            
            if (!(command is CommandGroup))
            {
                var ausführungsString = "";
                foreach (CommandOverload commandOverload in command.Overloads.OrderByDescending(x => x.Priority))
                {
                    var smallArgumentString = "";

                    foreach( CommandArgument argument in commandOverload.Arguments) {
                        smallArgumentString += $" <{argument.Name}>";
                    }

                    if (command.Parent == null) {
                        ausführungsString =  $"`${command.Name}{smallArgumentString}`";
                    } else {
                        ausführungsString = $"`${command.Parent.Name} {command.Name}{smallArgumentString}`";
                    }
                }
                Eb.AddField("- Ausführung",ausführungsString);
            }

            if (command.ExecutionChecks.Count > 0)
            {
                foreach (var check in command.ExecutionChecks)
                {
                    if (check is CooldownAttribute cd)
                    {
                        var reset = cd.Reset;
                        string remaining = "";
                        if (reset.Days > 0)
                        {
                            remaining += $"{reset.Days} Tage ";
                        }
                        if (reset.Hours > 0)
                        {
                            remaining += $"{reset.Hours} Stunden ";
                        }
                        if (reset.Minutes > 0)
                        {
                            remaining += $"{reset.Minutes} Minuten ";
                        }
                        if (reset.Seconds > 0)
                        {
                            remaining += $"{reset.Seconds} Sekunden ";
                        }

                        Eb.AddField("- Cooldown", remaining);
                    }
                }
            }

            if (overloads != null && overloads.Count > 0)
            {
                // StringBuilder stringBuilder = new StringBuilder();
                var argumentsString = "";
                int counter = 1;
                foreach (CommandOverload commandOverload in command.Overloads.OrderByDescending(x => x.Priority))
                {
                    if (command.Overloads.Count > 1 && counter == 1)
                    {
                        argumentsString += $"1. \n";
                    }
                    
                    if (counter > 1)
                    {
                        argumentsString += $"\n{counter}. \n";
                    }
                    foreach( CommandArgument argument in commandOverload.Arguments) {
                        argumentsString += $"`{argument.Name} ({this.CommandsNext.GetUserFriendlyTypeName(argument.Type)})` {argument.Description ?? "Keine Beschreibung vorhanden."}\n";
                    }

                    counter++;
                }

                if (argumentsString.Length > 0) Eb.AddField("- Arguments", argumentsString, false);
            }
            
            return (BaseHelpFormatter)this;
        }

        public override BaseHelpFormatter WithSubcommands(IEnumerable<Command> subcommands)
        {

            var commandsStrings = new List<string> {""};
            var index = 0;

            foreach(var command in subcommands)
            {
                string s = "";
                
                s = command.Description != null 
                    ? $"**{command.Name}{((command is CommandGroup) ? " [Gruppe]" : "")}**\n{command.Description}\n\n" 
                    : $"**{command.Name}{((command is CommandGroup) ? " [Gruppe]" : "")}**\n\n";

                if (commandsStrings[index].Length + s.Length < 1024)
                {
                    commandsStrings[index] += s;
                }
                else
                {
                    index++;
                    commandsStrings.Add(s);
                }
            }

            try
            {
                foreach (var s in commandsStrings)
                {
                    Eb.AddField(this.Command != null ? "- Subcommands" : "- Commands", s);
                }
            }
            catch (Exception e)
            {
                Eb.AddField("Exception", e.Message);
            }
            

            if (this.Command != null)
                Eb.AddField("- Ausführung", $"`${this.Command.Name} [Subcommand]`");
            
            return this;
        }
        

        public override CommandHelpMessage Build()
        {
            if (this.Command == null)
                this.Eb.WithDescription("Auflistung aller Toplevel Commands und Commandgruppen.\n Für weitere informationen `$help [Command]`");
            
            var embed = new DiscordEmbedBuilder(Eb)
            {
                Color = new DiscordColor(0xD091B2)
            };
            return new CommandHelpMessage(embed: embed);
        }
    }
}