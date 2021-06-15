using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using SudoBot.Models;

namespace SudoBot.Handlers
{
    public class ReactionIqHandler
    {
        // private List<string> emotes = new List<string>{"ollekLove",
        //                                                "CCOllek",
        //                                                "ollekSalute",
        //                                                "duckwave",
        //                                                "pepehappy",
        //                                                "elmoFire",
        //                                                "kekwO",
        //                                                "stonks_up",
        //                                                "pepetap"};

        private List<ulong> emoteIds = new List<ulong>{642742376510521375,
                                                       836600952805916722,
                                                       850322682649378846,
                                                       850841580998623233,
                                                       850832552188182608,
                                                       850898574103412736,
                                                       726398802872762389,
                                                       690290756031742429,
                                                       732198331618426940,
                                                       854262904056315934
        };

        private ulong ChannelId = 717135456185221120;

        public async Task HandleMessageSend(MessageCreateEventArgs args) {
            if (args.Channel.Id != ChannelId) return;
            if (args.Message.Author.IsBot) return;

            foreach(var emote in emoteIds) {
                try {
                    var em = DiscordEmoji.FromGuildEmote(Globals.Client, emote);
                    await args.Message.CreateReactionAsync(em);
                } catch (Exception) {}
            }
        }

        public async Task HandleReactionAdded(MessageReactionAddEventArgs args)
        {
            if (args.User.IsBot) return;
            if (!emoteIds.Contains(args.Emoji.Id)) return;
            
            DiscordMember member = await args.Guild.GetMemberAsync(args.User.Id);
            var user = await User.GetOrCreateUser(member);

            await user.AddSpecialPoints(10);
        }
        
        public async Task HandleReactionRemoved(MessageReactionRemoveEventArgs args)
        {
            if (args.User.IsBot) return;
            if (!emoteIds.Contains(args.Emoji.Id)) return;
            
            DiscordMember member = await args.Guild.GetMemberAsync(args.User.Id);
            var user = await User.GetOrCreateUser(member);

            await user.AddSpecialPoints(-10);
        }
    }
}