using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using SudoBot.Attributes;
using SudoBot.Database;
using SudoBot.Models;
using SudoBot.Specifics;

namespace SudoBot.Commands
{
    public class TestCommands : ApplicationCommandModule
    {
        [SlashCommand("test", "A aughhh Slash Command")]
        [SlashCommandPermissions(Permissions.Administrator)]
        public async Task Test(InteractionContext ctx)
        {
            await ctx.Channel.SendMessageAsync("amogus");
            await ctx.CreateResponseAsync("ahhh");
            await ctx.DeleteResponseAsync();
        }
    }
}