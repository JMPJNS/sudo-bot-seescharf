using System;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using SudoBot.Attributes;
using SudoBot.Database;
using SudoBot.Models;
using SudoBot.Specifics;

namespace SudoBot.Commands
{
    [Group("admin")]
    [Aliases("a")]
    [Description("Administrations Stuff")]
    public class AdminCommands : BaseCommandModule
    {
        public Translation Translator { private get; set; }
        
        [CheckForPermissions(SudoPermission.Me, GuildPermission.Any)]
        [Command("give-guild-permission")]
        [Aliases("ggp")]
        public async Task GiveGuildPermission(CommandContext ctx, string p, DiscordGuild guild = null)
        {
            guild ??= ctx.Guild;
            Enum.TryParse(p, out GuildPermission perm);
            if (perm == GuildPermission.Any)
            {
                await ctx.Channel.SendMessageAsync("Ungültige Eingabe");
                return;
            }
            var guildConfig = await Guild.GetGuild(guild.Id);
            if (!guildConfig.Permissions.Contains(perm))
            {
                await guildConfig.GivePermission(perm);
                await ctx.Channel.SendMessageAsync("Die Rechte wurden Vergeben!");
            }
        }

        [CheckForPermissions(SudoPermission.Admin, GuildPermission.Any)]
        [Command("set-language"), Aliases("set-lang")]
        public async Task SetLanguage(CommandContext ctx, [Description("language: en/de")] string l)
        {
            Translation.Lang lang;
            if (l.ToLower() == "de" || l.ToLower() == "deutsch" || l.ToLower() == "german")
            {
                lang = Translation.Lang.De;
            }
            else
            {
                lang = Translation.Lang.En;
            }

            var guild = await Guild.GetGuild(ctx.Guild.Id);
            await guild.SetLanguage(lang);
            await ctx.RespondAsync(Translator.Translate("DONE", guild.Language));
        }

        [CheckForPermissions(SudoPermission.Me, GuildPermission.Any)]
        [Command("set-all-langs")]
        public async Task SetAllLangs(CommandContext ctx)
        {
            try
            {
                var guilds = await Mongo.Instance.GetAllGuilds();
                foreach (var g in guilds)
                {
                    await g.SetLanguage(Translation.Lang.De);
                }

                await ctx.RespondAsync("done");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            
        }

        // Mute
        // // Alle Rollen weg nehmen, und 639522863023521822 geben, bei unmuted wieder rollen geben
        
        [CheckForPermissions(SudoPermission.Me, GuildPermission.Any)]
        [Command("get-all-permissions")]
        [Aliases("gap")]
        public async Task GetAllPermissions(CommandContext ctx)
        {
            var perms = string.Join("\n", Enum.GetNames(typeof(GuildPermission)));
            await ctx.Channel.SendMessageAsync(perms);
        }
        
        [CheckForPermissions(SudoPermission.Me, GuildPermission.Any)]
        [Command("execute-command"), Description("Execute Shell Command"), RequireOwner]
        [Aliases("exec")]
        public async Task ExecuteShellCommand(CommandContext ctx, [RemainingText]string command)
        {
            var res = await Globals.RunCommand(command);
            await ctx.RespondAsync(res);
        }

        [CheckForPermissions(SudoPermission.Me, GuildPermission.Any)]
        [Command("block-user")]
        public async Task BlockUser(CommandContext ctx, DiscordMember member)
        {
            var user = await User.GetOrCreateUser(member);
            await user.AddPermission(UserPermissions.Blocked);
            await ctx.RespondAsync($"```User {member.Mention} blocked from using the Bot```");
        }
        
        [CheckForPermissions(SudoPermission.Me, GuildPermission.Any)]
        [Command("unblock-user")]
        public async Task UnblockUser(CommandContext ctx, DiscordMember member)
        {
            var user = await User.GetOrCreateUser(member);
            await user.RemovePermission(UserPermissions.Blocked);
            await ctx.RespondAsync($"```User {member.Mention} unblocked from using the Bot```");
        }

        [CheckForPermissions(SudoPermission.Admin, GuildPermission.Any)]
        [Description("Setze den Log Channel für diverse Bot Aktionen")]
        [Command("set-log-channel"), Aliases("slc")]
        public async Task SetLogChannel(CommandContext ctx, [Description("Der Channel für die Nachrichten")]DiscordChannel channel)
        {
            var guild = await Guild.GetGuild(ctx.Guild.Id);
            await guild.SetLocalLogChannel(channel.Id);
            await ctx.Channel.SendMessageAsync("Der Channel wurde Gesetzt");
        }
        
        [CheckForPermissions(SudoPermission.Admin, GuildPermission.Any)]
        [Description("Setze den Channel, der für diverse Bot Aktionen erlaubt ist. \nSobald dieser gesetzt wurde ist Beispielsweise `$rank` nur noch in dem verfügbar.\n`$admin unset-command-channel` um dies wieder zu Deaktivieren.")]
        [Command("set-command-channel"), Aliases("scc")]
        public async Task SetCommandChannel(CommandContext ctx, [Description("Der Channel für die Commands")]DiscordChannel channel)
        {
            var guild = await Guild.GetGuild(ctx.Guild.Id);
            await guild.SetCommandChannel(channel.Id);
            await ctx.Channel.SendMessageAsync("Der Channel wurde Gesetzt");
        }
        
        [CheckForPermissions(SudoPermission.Admin, GuildPermission.Any)]
        [Description("Deaktivieren, dass einige Commands nur in Einem Channel funktionieren")]
        [Command("unset-command-channel"), Aliases("uscc")]
        public async Task SetCommandChannel(CommandContext ctx)
        {
            var guild = await Guild.GetGuild(ctx.Guild.Id);
            await guild.SetCommandChannel(0);
            await ctx.Channel.SendMessageAsync("Command Limitierung wurde Aufgehoben.");
        }
    }
}