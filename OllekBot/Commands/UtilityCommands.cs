using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using OllekBot.DataInterfaces;
using OllekBot.Handlers;

namespace OllekBot.Commands
{
    public class UtilityCommands : BaseCommandModule
    {
        [Command("say")]
        [RequireRoles(RoleCheckMode.Any, new []{"Admins", "Mods", "Developers"})]
        public async Task Ping(CommandContext ctx, params string[] words)
        {
            var message = await ctx.Channel.SendMessageAsync(ctx.RawArgumentString);
        }
        
        [Command("listUsers")]
        public async Task ListUsers(CommandContext ctx)
        {
            foreach (User user in UserHandler.Users)
            {
                DiscordGuild currentGuild = await ctx.Client.GetGuildAsync(user.GuildId);
                if (ctx.Guild == currentGuild)
                {
                    var message = await ctx.Channel.SendMessageAsync($"{await currentGuild.GetMemberAsync(user.UserId)}");
                }
            }
        }
    }
}