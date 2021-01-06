using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using SudoBot.Attributes;
using SudoBot.Database;
using SudoBot.Models;

namespace SudoBot.Commands
{
    [Group("mod")]
    [Aliases("m")]
    [Description("Moderation Stuff")]
    public class ModCommands : BaseCommandModule
    {
        [CheckForPermissions(SudoPermission.Mod, GuildPermission.Any)]
        [Description("Banne einen Member")]
        [Command("ban")]
        public async Task BanMember(CommandContext ctx, [Description("Der zu bannende Member")]DiscordMember member)
        {
            var guild = await Guild.GetGuild(ctx.Guild.Id);
            try
            {
                await member.BanAsync();
                var embed = new DiscordEmbedBuilder()
                    .WithColor(DiscordColor.Red)
                    .WithThumbnail(member.AvatarUrl)
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

        [Command]
        [CheckForPermissions(SudoPermission.Mod, GuildPermission.Any)]
        public async Task CreateRole(CommandContext ctx, string name)
        {
            var role = await ctx.Guild.CreateRoleAsync(name);
            if (role == null)
            {
                throw new Exception("Role Creation Failed");
            }
        }
        
        [Command]
        [CheckForPermissions(SudoPermission.Mod, GuildPermission.Any)]
        public async Task CreateChannel(CommandContext ctx, string name, DiscordChannel parent = null)
        {
            if (parent != null && parent.IsCategory == false)
            {
                throw new Exception("Parent must be a category");
            }
            var channel = await ctx.Guild.CreateChannelAsync(name, ChannelType.Text, parent);
            
            if (channel == null)
            {
                throw new Exception("Channel Creation Failed");
            }
        }

        //TODO blacklisted words, channel basiert
        [Command("clean"), Description("Die letzten X bot nachrichten löschen")]
        public async Task Clean(CommandContext ctx, int count = 5) {
            var botMember = await ctx.Guild.GetMemberAsync(ctx.Client.CurrentUser.Id);    
            await Purge(ctx, botMember, count);
        }
        
        [CheckForPermissions(SudoPermission.Mod, GuildPermission.Any)]
        [Description("Mehrere Nachrichten eines members löschen.")]
        [Command("purge")]
        public async Task Purge(CommandContext ctx, DiscordMember member, int count) {
            var allMessages = await ctx.Channel.GetMessagesBeforeAsync(ctx.Message.Id);
            var filtered = allMessages.Where(message => message.Author.Id == member.Id).ToList();

            if (filtered.Count() < count) count = filtered.Count();

            var deleteMessages = new List<DiscordMessage>();

            for (int i = 0; i < count; i++) {
                deleteMessages.Add(filtered[i]);
            }

            await ctx.Channel.DeleteMessagesAsync(deleteMessages);
            await ctx.Message.DeleteAsync();
        }
        
    }
}
