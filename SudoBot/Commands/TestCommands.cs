using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
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
        [SlashCommand("test", "A test Slash Command")]
        public async Task Test(InteractionContext ctx)
        {
            var roles = string.Join("\n", ctx.Guild.Roles.Select(x => $"{x.Value.Id}: {x.Value.Name}"));

            await ctx.CreateResponseAsync(roles);
        }
    }
}