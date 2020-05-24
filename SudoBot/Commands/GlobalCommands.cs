using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using SudoBot.Attributes;
using SudoBot.Models;

namespace SudoBot.Commands
{
    public class GlobalCommands : BaseCommandModule
    {
      
        [Command("invite")]
        [Description("Invite Link um den Bot einzuladen (kann jeder verwenden)")]
        public async Task Invite(CommandContext ctx)
        {
            await ctx.Channel.SendMessageAsync(
                "https://discord.com/oauth2/authorize?client_id=705548602994458684&scope=bot&permissions=1544023122");
        }
        
        [CheckForPermissions(SudoPermission.Mod, GuildPermission.Any)]
        [Description("Schreibe eine Nachricht als Bot user in den aktuellen Channel")]
        [Command("say")]
        public async Task Say(CommandContext ctx, [Description("Die Nachricht")]params string[] words)
        {
            await ctx.Channel.SendMessageAsync(string.Join(" ",words));
            await ctx.Message.DeleteAsync();
        }
    }
}