using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using SudoBot.Attributes;
using SudoBot.Models;

namespace SudoBot.Commands
{
    [Group("tag")]
    [Aliases("t")]
    [Description("Tag Stuff")]
    public class TagCommands: BaseCommandModule
    {
        [Command("create")]
        [CheckForPermissions(SudoPermission.Mod, GuildPermission.Any)]
        [Description("Erstelle einen Tag")]
        public async Task CreateTag(CommandContext ctx, String name, params String[] content)
        {
            var tz = Environment.OSVersion.Platform == PlatformID.Win32NT ? TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time") : TimeZoneInfo.FindSystemTimeZoneById("Europe/Vienna");
            var foundTag = await Tag.GetTag(name);

            if (foundTag != null)
            {
                await ctx.Channel.SendMessageAsync($"Der Tag `{name}` existiert bereits.");
                return;
            }
            
            int index = ctx.RawArgumentString.IndexOf(ctx.RawArguments[1], StringComparison.Ordinal);
            string cleanArgs = ctx.RawArgumentString.Remove(0, index);

            if (cleanArgs.EndsWith('"'))
            {
                cleanArgs = cleanArgs.Remove(cleanArgs.Length - 1);
            }

            var t = new Tag
            {
                Content = cleanArgs,
                Name = name,
                Type = TagType.Guild,
                UserCreated = ctx.User.Id,
                ChannelCreated = ctx.Channel.Id,
                GuildCreated = ctx.Guild.Id,
                DateCreated = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz)
            };

            await t.CreateTag();

            await ctx.Channel.SendMessageAsync($"Tag `{name}` erstellt.");
        }

        [Command("edit")]
        [CheckForPermissions(SudoPermission.Mod, GuildPermission.Any)]
        [Description("Bearbeite einen Tag")]
        public async Task EditTag(CommandContext ctx, String name, params String[] content)
        {
            var tz = Environment.OSVersion.Platform == PlatformID.Win32NT ? TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time") : TimeZoneInfo.FindSystemTimeZoneById("Europe/Vienna");

            var foundTag = await Tag.GetTag(name);

            if (foundTag == null)
            {
                await ctx.Channel.SendMessageAsync($"Der Tag `{name}` existiert nicht.");
                return;
            }
            
            int index = ctx.RawArgumentString.IndexOf(ctx.RawArguments[1], StringComparison.Ordinal);
            string cleanArgs = ctx.RawArgumentString.Remove(0, index);

            if (cleanArgs.EndsWith('"'))
            {
                cleanArgs = cleanArgs.Remove(cleanArgs.Length - 1);
            }

            foundTag.Content = cleanArgs;

            await foundTag.UpdateTag();

            await ctx.Channel.SendMessageAsync($"Tag `{name}` bearbeitet.");
        }

        [Command("delete")]
        [CheckForPermissions(SudoPermission.Mod, GuildPermission.Any)]
        [Description("Lösche einen Tag")]
        public async Task DeleteTag(CommandContext ctx, String name)
        {
            var foundTag = await Tag.GetTag(name);

            if (foundTag == null)
            {                
                await ctx.Channel.SendMessageAsync($"Tag `{name}` existiert nicht");
                return;
            }

            await foundTag.DeleteTag();

            await ctx.Channel.SendMessageAsync($"Tag {name} gelöscht");
        }
        

        [GroupCommand]
        [CheckForPermissions(SudoPermission.Any, GuildPermission.Any)]
        [Description("Finde einen Tag")]
        public async Task GetTag(CommandContext ctx, String name)
        {
            var foundTag = await Tag.GetTag(name);

            if (foundTag == null)
            {
                var similar = await Tag.FindSimilarTags(name);

                String similarMessage = "Meintest Du:\n";

                foreach (var s in similar)
                {
                    similarMessage += $"`{s.Name}`\n";
                }

                var sendString = similar.Count > 0
                    ? $"Tag `{name}` existiert nicht\n{similarMessage}"
                    : $"Tag `{name}` existiert nicht";
                
                await ctx.Channel.SendMessageAsync(sendString);


                return;
            }

            await ctx.Channel.SendMessageAsync(foundTag.Content);
        }
        
        // [GroupCommand]
        // [Description("Finde einen Tag")]
        // public async Task GetTagTyped(CommandContext ctx, TagType type, String name)
        // {
        //     var foundTag = await Tag.GetTag(name, type);
        //
        //     if (foundTag == null)
        //     {
        //         await ctx.Channel.SendMessageAsync($"Tag `{name}` exestiert nicht");
        //         return;
        //     }
        //
        //     await ctx.Channel.SendMessageAsync(foundTag.Content);
        // }
        
        
    }
}