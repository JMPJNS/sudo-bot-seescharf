using System;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using SudoBot.Attributes;
using SudoBot.Database;
using SudoBot.Models;

namespace SudoBot.Commands
{
    public class AdminCommands : BaseCommandModule
    {
        [CheckForPermissions(SudoPermission.Me, GuildPermission.Any)]
        [Command("giveGuildPermission")]
        public async Task GiveGuildPermission(CommandContext ctx, string p)
        {
            Enum.TryParse(p, out GuildPermission perm);
            if (perm == GuildPermission.Any)
            {
                await ctx.Channel.SendMessageAsync("Ungültige Eingabe");
                return;
            }
            var guildConfig = await Guild.GetGuild(ctx.Guild.Id);
            if (!guildConfig.Permissions.Contains(perm))
            {
                await guildConfig.GivePermission(perm);
                await ctx.Channel.SendMessageAsync("Die Rechte wurden Vergeben!");
            }
        }
        
        // Mute
        // // Alle Rollen weg nehmen, und 639522863023521822 geben, bei unmuted wieder rollen geben
        // Ban

        [CheckForPermissions(SudoPermission.Me, GuildPermission.Any)]
        [Command("getAllPermissions")]
        public async Task GetAllPermissions(CommandContext ctx)
        {
            var perms = string.Join("\n", Enum.GetNames(typeof(GuildPermission)));
            await ctx.Channel.SendMessageAsync(perms);
        }

        [CheckForPermissions(SudoPermission.Admin, GuildPermission.Any)]
        [Command("setLogChannel")]
        public async Task SetLogChannel(CommandContext ctx, DiscordChannel channel)
        {
            var guild = await Guild.GetGuild(ctx.Guild.Id);
            await guild.SetLocalLogChannel(channel.Id);
            await ctx.Channel.SendMessageAsync("Der Channel wurde Gesetzt");
        }

        [CheckForPermissions(SudoPermission.Mod, GuildPermission.Any)]
        [Command("ban")]
        public async Task BanMember(CommandContext ctx, DiscordMember member)
        {
            var guild = await Guild.GetGuild(ctx.Guild.Id);
            try
            {
                await member.BanAsync();
                var embed = new DiscordEmbedBuilder()
                    .WithColor(DiscordColor.Red)
                    .WithThumbnailUrl(member.AvatarUrl)
                    .WithDescription(member.Mention)
                    .AddField("Von", ctx.Member.Mention)
                    .WithTitle("Banned");
                var lChannel = ctx.Guild.GetChannel(guild.LocalLogChannel);
                await lChannel.SendMessageAsync(embed: embed.Build());
            }
            catch (Exception e)
            {
                if (e.Message == "Object reference not set to an instance of an object.")
                {
                    await ctx.Channel.SendMessageAsync("Kein Log Channel gesetzt!, setze einen mit `$setLogChannel #{Channel}`");
                }
                else if (e.Message == "Unauthorized: 403")
                {
                    await ctx.Channel.SendMessageAsync("Keine Ban Berechtigung");
                }
                else
                {
                    await ctx.Channel.SendMessageAsync($"{e.Message}, wenn dies ein Unschlüssiger Fehler ist, JMP#7777 kontaktieren");
                }
                
            }
        }
        
    }
}