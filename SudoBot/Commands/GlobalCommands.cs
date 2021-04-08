using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.Extensions.DependencyInjection;
using SudoBot.Attributes;
using SudoBot.Database;
using SudoBot.Models;

namespace SudoBot.Commands
{
    public class GlobalCommands : BaseCommandModule
    {

        public Translation Translator { private get; set; }
        
        [Command("invite")]
        [CheckForPermissions(SudoPermission.Any, GuildPermission.Any)]
        [Description("Invite Link um den Bot einzuladen (kann jeder verwenden)")]
        public async Task Invite(CommandContext ctx)
        {
            await ctx.Channel.SendMessageAsync(
                "https://discord.com/oauth2/authorize?client_id=705548602994458684&scope=bot&permissions=1544023122");
        }

        [Command("vote")]
        [CheckForPermissions(SudoPermission.Any, GuildPermission.Any)]
        [Description("Den Bot auf top.gg voten")]
        public async Task Vote(CommandContext ctx)
        {
            await ctx.Channel.SendMessageAsync("https://top.gg/bot/705548602994458684");
        }
        
        [Command("source"), Aliases("code", "source-code")]
        [CheckForPermissions(SudoPermission.Any, GuildPermission.Any)]
        [Description("Den Source Code vom Bot Anzeigen")]
        public async Task Source(CommandContext ctx)
        {
            await ctx.Channel.SendMessageAsync("https://github.com/JMPJNS/sudo-bot-seescharf");
        }

        [Command("guild")]
        [CheckForPermissions(SudoPermission.Any, GuildPermission.Any)]
        [Description("Invite link zum Sudo Discord")]
        public async Task Guild(CommandContext ctx)
        {
            await ctx.Channel.SendMessageAsync("https://discord.gg/gHV2uhb");
        }
        
        [Command("enlarge")]
        [CheckForPermissions(SudoPermission.Any, GuildPermission.Any)]
        [Description("Emote groß machen")]
        public async Task Enlarge(CommandContext ctx, [Description("Das Emoji zu vergrößern")]DiscordEmoji e)
        {
            var args = ctx.RawArgumentString;
            var cmd = ctx.CommandsNext.FindCommand("u make-big", out args);
            await cmd.ExecuteAsync(ctx);
        }

        [Command("guild-count")]
        [CheckForPermissions(SudoPermission.Me, GuildPermission.Any)]
        [Description("How many guilds the bot is in")]
        public async Task GuildCount(CommandContext ctx) {
            await ctx.RespondAsync(ctx.Client.Guilds.Count.ToString());
        }
        
        [Command("user-count")]
        [CheckForPermissions(SudoPermission.Me, GuildPermission.Any)]
        [Description("How many users the bot has")]
        public async Task UserCount(CommandContext ctx) {
            await ctx.RespondAsync((await Mongo.Instance.GetUserCount()).ToString());
        }

        [Command("set-status"), Description("Set Bot Status"), RequireOwner]
        [CheckForPermissions(SudoPermission.Me, GuildPermission.Any)]
        public async Task SetStatus(CommandContext ctx, string status)
        {
            await ctx.Client.UpdateStatusAsync(new DiscordActivity(status, ActivityType.ListeningTo));
        }
        
        [Command("remind")]
        [CheckForPermissions(SudoPermission.Any, GuildPermission.Any)]
        [Description("Eine Erinnerung zu einem Bestimmten Zeitpunkt Erstellen erstellen")]
        public async Task Reminder(CommandContext ctx, [Description("um")] string format , [Description("Zeitpunkt {beispiel: 12:00}")] DateTime time, [Description("Nachricht"), RemainingText] string message)
        {
            var dif = DateTime.Now - DateTime.UtcNow;
            var timespan = time - DateTime.Now;
            await UtilityCommands.ReminderCommand(ctx, format, DateTime.UtcNow + timespan - dif, message);
        }
        
        [Command("remind")]
        [CheckForPermissions(SudoPermission.Any, GuildPermission.Any)]
        [Description("Eine Erinnerung in x (s/m/h/d) erstellen")]
        public async Task Reminder(CommandContext ctx, [Description("in")] string format, [Description("Zeitspanne {beispiel: 12m}")] TimeSpan timespan, [Description("Nachricht"), RemainingText] string message)
        {
            await UtilityCommands.ReminderCommand(ctx, format, DateTime.UtcNow + timespan, message);
        }

        [Command("translate"), Description("get translation for line in language"),
         CheckForPermissions(SudoPermission.Me, GuildPermission.Any)]
        public async Task Translate(CommandContext ctx, string line, String lang = "en", [RemainingText] String args = "")
        {
            Translation.Lang language = lang.ToLower() switch
            {
                "en" => Translation.Lang.En,
                "de" => Translation.Lang.De,
                _ => Translation.Lang.En
            };

            var argList = args.Split(",").ToList();

            await ctx.RespondAsync(Translator.Translate(line, language, argList));
        }
        
        
        // Copyright 2018 Emzi0767
        //
        // Licensed under the Apache License, Version 2.0 (the "License");
        // you may not use this file except in compliance with the License.
        //     You may obtain a copy of the License at
        //
        // http://www.apache.org/licenses/LICENSE-2.0
        //
        // Unless required by applicable law or agreed to in writing, software
        //     distributed under the License is distributed on an "AS IS" BASIS,
        // WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
        //     See the License for the specific language governing permissions and
        // limitations under the License.
        // Original: https://github.com/Emzi0767/Discord-Companion-Cube-Bot/commit/708111bd1e391aeb921931ec5e002c218bc3376a#diff-4b1bfdba524893a69046e4e9ac23e635
        [Command("eval"), Description("Evaluates a snippet of C# code, in context."), RequireOwner]
        [CheckForPermissions(SudoPermission.Me, GuildPermission.Any)]
        public async Task Eval(CommandContext ctx, [RemainingText, Description("Code to evaluate.")] string code)
        {
            var cs1 = code.IndexOf("```") + 3;
            cs1 = code.IndexOf('\n', cs1) + 1;
            var cs2 = code.LastIndexOf("```");

            if (cs1 == -1 || cs2 == -1)
            {
                // throw new ArgumentException("You need to wrap the code into a code block.", nameof(code));
            }
            else
            {
                code = code.Substring(cs1, cs2 - cs1);
            }

            var embed = new DiscordEmbedBuilder
            {
                Title = "Evaluating...",
                Color = new DiscordColor(0xD091B2)
            };
            var msg = await ctx.RespondAsync("", embed: embed.Build());

            var globals = new EvaluationEnvironment(ctx);
            var sopts = ScriptOptions.Default
                .WithImports("System", "System.Collections.Generic", "System.Diagnostics", "System.IO", "System.Net", "System.Linq", "System.Net.Http", "System.Net.Http.Headers", "System.Reflection", "System.Text", 
                             "System.Threading.Tasks", "Newtonsoft.Json", "Fizzler.Systems.HtmlAgilityPack", "HtmlAgilityPack", "DSharpPlus", "DSharpPlus.CommandsNext", "DSharpPlus.Entities", "DSharpPlus.EventArgs", "DSharpPlus.Exceptions", "SudoBot.Models", "SudoBot.Database", "SudoBot.Parser")
                .WithReferences(AppDomain.CurrentDomain.GetAssemblies().Where(xa => !xa.IsDynamic && !string.IsNullOrWhiteSpace(xa.Location)));
            
            var sw1 = Stopwatch.StartNew();
            var cs = CSharpScript.Create(code, sopts, typeof(EvaluationEnvironment));
            var csc = cs.Compile();
            sw1.Stop();
            
            if (csc.Any(xd => xd.Severity == DiagnosticSeverity.Error))
            {
                embed = new DiscordEmbedBuilder
                {
                    Title = "Compilation failed",
                    Description = string.Concat("Compilation failed after ", sw1.ElapsedMilliseconds.ToString("#,##0"), "ms with ", csc.Length.ToString("#,##0"), " errors."),
                    Color = new DiscordColor(0xD091B2)
                };
                foreach (var xd in csc.Take(3))
                {
                    var ls = xd.Location.GetLineSpan();
                    embed.AddField(string.Concat("Error at ", ls.StartLinePosition.Line.ToString("#,##0"), ", ", ls.StartLinePosition.Character.ToString("#,##0")), Formatter.InlineCode(xd.GetMessage()), false);
                }
                if (csc.Length > 3)
                {
                    embed.AddField("Some errors ommited", string.Concat((csc.Length - 3).ToString("#,##0"), " more errors not displayed"), false);
                }
                await msg.ModifyAsync(embed: embed.Build());
                return;
            }

            Exception rex = null;
            ScriptState<object> css = null;
            var sw2 = Stopwatch.StartNew();
            try
            {
                css = await cs.RunAsync(globals);
                rex = css.Exception;
            }
            catch (Exception ex)
            {
                rex = ex;
            }
            sw2.Stop();

            if (rex != null)
            {
                embed = new DiscordEmbedBuilder
                {
                    Title = "Execution failed",
                    Description = string.Concat("Execution failed after ", sw2.ElapsedMilliseconds.ToString("#,##0"), "ms with `", rex.GetType(), ": ", rex.Message, "`."),
                    Color = new DiscordColor(0xD091B2),
                };
                await msg.ModifyAsync(embed: embed.Build());
                return;
            }

            // execution succeeded
            embed = new DiscordEmbedBuilder
            {
                Title = "Evaluation successful",
                Color = new DiscordColor(0xD091B2),
            };

            embed.AddField("Result", css.ReturnValue != null ? css.ReturnValue.ToString() : "No value returned", false)
                .AddField("Compilation time", string.Concat(sw1.ElapsedMilliseconds.ToString("#,##0"), "ms"), true)
                .AddField("Execution time", string.Concat(sw2.ElapsedMilliseconds.ToString("#,##0"), "ms"), true);

            if (css.ReturnValue != null)
                embed.AddField("Return type", css.ReturnValue.GetType().ToString(), true);

            await msg.ModifyAsync(embed: embed.Build());
        }

        [Description("Der Developer des Bots")]
        [CheckForPermissions(SudoPermission.Any, GuildPermission.Any)]
        [Command("developer"), Aliases("dev")]
        public async Task Developer(CommandContext ctx, params string[] nix)
        {
            var me = await ctx.Client.GetUserAsync(Globals.MyId);
            var embed = new DiscordEmbedBuilder()
                .WithColor(DiscordColor.Aquamarine)
                .WithThumbnail(me.AvatarUrl)
                .WithTitle("Entwickler des Bots")
                .WithDescription($"{me.Username}#{me.Discriminator}");
            await ctx.Channel.SendMessageAsync(embed: embed.Build());
        }

        [Description("Say in guild / channel")]
        [Command("sg"), Hidden()]
        [CheckForPermissions(SudoPermission.Me, GuildPermission.Any)]
        public async Task sayGlobal(CommandContext ctx, ulong guild, ulong channel,
            params string[] words)
        {
            var g = await ctx.Client.GetGuildAsync(guild);
            var c = g.GetChannel(channel);

            await c.SendMessageAsync(string.Join(" ", words));
        }
    }
    public sealed class EvaluationEnvironment
    {
        public CommandContext Context { get; }

        public DiscordMessage Message => this.Context.Message;
        public DiscordChannel Channel => this.Context.Channel;
        public DiscordGuild Guild => this.Context.Guild;
        public DiscordUser User => this.Context.User;
        public DiscordMember Member => this.Context.Member;
        public DiscordClient Client => this.Context.Client;
        public HttpClient Http => this.Context.Services.GetService<HttpClient>();

        public EvaluationEnvironment(CommandContext ctx)
        {
            this.Context = ctx;
        }
    }
}
