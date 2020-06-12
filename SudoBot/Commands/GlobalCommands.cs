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

	[Command("vote")]
	[Description("Den Bot auf top.gg voten")]
	public async Task Vote(CommandContext ctx)
	{
		await ctx.Channel.SendMessageAsync("https://top.gg/bot/705548602994458684");
	}
        
        [CheckForPermissions(SudoPermission.Mod, GuildPermission.Any)]
        [Description("Schreibe eine Nachricht als Bot user in den aktuellen Channel")]
        [Command("say")]
        public async Task Say(CommandContext ctx, [Description("Die Nachricht")]params string[] words)
        {
            await ctx.Channel.SendMessageAsync(string.Join(" ",words));
            await ctx.Message.DeleteAsync();
        }

        [Description("Der Developer des Bots")] [Command("developer"), Aliases("dev")]
        public async Task Developer(CommandContext ctx, params string[] nix)
        {
            var me = await ctx.Client.GetUserAsync(Globals.MyId);
            var embed = new DiscordEmbedBuilder()
                .WithColor(DiscordColor.Aquamarine)
                .WithThumbnailUrl(me.AvatarUrl)
                .WithTitle("Entwickler des Bots")
                .WithDescription($"{me.Username}#{me.Discriminator}");
            await ctx.Channel.SendMessageAsync(embed:embed.Build());
        }

        [Description("Say in guild / channel")]
        [Command("sg")]
        [CheckForPermissions(SudoPermission.Me, GuildPermission.Any)]
        public async Task sayGlobal(CommandContext ctx, ulong guild, ulong channel,
            params string[] words)
        {
            var g = await ctx.Client.GetGuildAsync(guild);
            var c = g.GetChannel(channel);
            
            await c.SendMessageAsync(string.Join(" ",words));
        }
    }
}
