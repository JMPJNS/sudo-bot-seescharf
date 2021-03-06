﻿using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.Lavalink;
using SudoBot.Attributes;
namespace SudoBot.Commands
{
    [Group("music")]
    [Description("Music Bot Stuff")]
    public class MusicCommands : BaseCommandModule
    {

        private async Task<LavalinkGuildConnection> GetConnection(CommandContext ctx)
        {
            if (ctx.Member.VoiceState == null || ctx.Member.VoiceState.Channel == null)
            {
                throw new ExternalException("You are not connected to a Voice Channel");
            }
            
            var channel = ctx.Member.VoiceState.Channel;
            var lava = ctx.Client.GetLavalink();
            var node = lava.ConnectedNodes.Values.First();
            if (node.IsConnected)
            {
                var conn = node.GetGuildConnection(ctx.Guild);
                if (conn != null)
                {
                    return conn;
                }
            }

            await node.ConnectAsync(channel);
            var connection = node.GetGuildConnection(ctx.Guild);
            if (connection == null)
            {
                throw new Exception("Lavalink is not Connected, contact JMP");
            }

            return connection;
        }

        [Command, CheckForPermissions(SudoPermission.Any, GuildPermission.Music)]
        public async Task Play(CommandContext ctx, [RemainingText] string search)
        {
            var conn = await GetConnection(ctx);

            var loadResult = await conn.Node.Rest.GetTracksAsync(search);

            if (loadResult.LoadResultType == LavalinkLoadResultType.LoadFailed 
                || loadResult.LoadResultType == LavalinkLoadResultType.NoMatches)
            {
                await ctx.RespondAsync($"Track search failed for {search}.");
                return;
            }
            
            var tracks = loadResult.Tracks.Take(5).ToArray();
            
            var embed = new DiscordEmbedBuilder()
                .WithColor(DiscordColor.CornflowerBlue)
                .WithTitle($"Found {tracks.Length}")
                .WithFooter(search)
                .WithDescription("Respond with number to play");

            for (var i = 0; i < tracks.Count(); i++)
            {
                embed.AddField((i + 1).ToString(), $"[{tracks[i].Title}]({tracks[i].Uri})");
            }

            await ctx.RespondAsync(embed: embed.Build());
            
            var ita = ctx.Client.GetInteractivity();

            var m = await ita.WaitForMessageAsync(x => x.Author == ctx.Message.Author, TimeSpan.FromSeconds(30));
            
            var num = Int32.Parse(m.Result.Content) - 1;

            if (num > tracks.Length)
            {
                await ctx.RespondAsync("Invalid Track Number");
                return;
            }

            await conn.PlayAsync(tracks[num]);

            await ctx.RespondAsync($"Now playing {tracks[num].Title}!");
        }
        
        [Command, CheckForPermissions(SudoPermission.Any, GuildPermission.Music)]
        public async Task Stop(CommandContext ctx)
        {
            var conn = await GetConnection(ctx);

            await conn.DisconnectAsync();
        }
        

        [Command, CheckForPermissions(SudoPermission.Any, GuildPermission.Music)]
        public async Task Pause(CommandContext ctx)
        {
            var conn = await GetConnection(ctx);

            if (conn.CurrentState.CurrentTrack == null)
            {
                await ctx.RespondAsync("There are no tracks loaded.");
                return;
            }

            await conn.PauseAsync();
            // conn.DisconnectAsync();
        }

        [Command, CheckForPermissions(SudoPermission.Any, GuildPermission.Music)]
        public async Task Info(CommandContext ctx)
        {
            var conn = await GetConnection(ctx);
            
            var url = conn.CurrentState.CurrentTrack.Uri.ToString();
            var queryString = url.Substring(url.IndexOf('?')).Split('#')[0];
            var parameters = System.Web.HttpUtility.ParseQueryString(queryString);

            var embed = new DiscordEmbedBuilder()
                .WithColor(DiscordColor.MidnightBlue)
                .WithTitle(conn.CurrentState.CurrentTrack.Title)
                .AddField("Pos", conn.CurrentState.PlaybackPosition.TotalSeconds.ToString())
                .AddField("Len", conn.CurrentState.CurrentTrack.Length.TotalSeconds.ToString());
            
            if (url.Contains("youtube") && parameters.Get("v") != null)
            {
                embed.WithThumbnail($"http://img.youtube.com/vi/{parameters.Get("v")}/0.jpg");
            }

            await ctx.RespondAsync(embed: embed.Build());
        }
    }
}