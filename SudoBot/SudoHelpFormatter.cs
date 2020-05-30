using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DSharpPlus;
using DSharpPlus.CommandsNext;
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

            if (command is CommandGroup commandGroup && !commandGroup.IsExecutableWithoutSubcommands)
                Eb.WithDescription(Eb.Description + "\n\nDieser Command ist eine Command Gruppe, bitte einen Subcommand spezifizieren.");

            IReadOnlyList<string> aliases = command.Aliases;
            if (aliases != null && aliases.Count > 0)
                Eb.AddField("Aliases", string.Join(", ", command.Aliases.Select(Formatter.InlineCode)));

            IReadOnlyList<CommandOverload> overloads = command.Overloads;
            if (overloads != null && overloads.Count > 0)
            {
                StringBuilder stringBuilder = new StringBuilder();
                foreach (CommandOverload commandOverload in command.Overloads.OrderByDescending(x => x.Priority))
                {
                    stringBuilder.Append('`').Append(command.QualifiedName);
                    foreach (CommandArgument commandArgument in (IEnumerable<CommandArgument>)commandOverload.Arguments)
                        stringBuilder.Append(commandArgument.IsOptional || commandArgument.IsCatchAll ? " [" : " <").Append(commandArgument.Name).Append(commandArgument.IsCatchAll ? "..." : "").Append(commandArgument.IsOptional || commandArgument.IsCatchAll ? ']' : '>');
                    stringBuilder.Append("`\n");
                    foreach (CommandArgument commandArgument in (IEnumerable<CommandArgument>)commandOverload.Arguments)
                        stringBuilder.Append('`').Append(commandArgument.Name).Append(" (").Append(this.CommandsNext.GetUserFriendlyTypeName(commandArgument.Type)).Append(")`: ").Append(commandArgument.Description ?? "No description provided.").Append('\n');
                    stringBuilder.Append('\n');
                }
                Eb.AddField("Arguments", stringBuilder.ToString().Trim(), false);
            }
            return (BaseHelpFormatter)this;
        }

        public override BaseHelpFormatter WithSubcommands(IEnumerable<Command> subcommands)
        {
            return this._d.WithSubcommands(subcommands);
        }

        public override CommandHelpMessage Build()
        {
            var hmsg = this._d.Build();
            var embed = new DiscordEmbedBuilder(hmsg.Embed)
            {
                Color = new DiscordColor(0xD091B2)
            };
            return new CommandHelpMessage(embed: embed);
        }
    }
}