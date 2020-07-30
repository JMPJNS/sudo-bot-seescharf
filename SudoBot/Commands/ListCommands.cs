using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using SudoBot.Attributes;
using SudoBot.Models;

namespace SudoBot.Commands
{
    [Group("list"), Aliases("l")]
    [Description("Commands um listen zu führen (Anime list, Book list...)")]
    public class ListCommands : BaseCommandModule
    {
        [Command("create"), Aliases("c"), Description("Eine neue Liste erstellen"), CheckForPermissions(SudoPermission.Me, GuildPermission.Any)]
        public async Task CreateList(CommandContext ctx, string name)
        {
            SudoList list;
            try
            {
                list = new SudoList(name, ctx.User.Id);
                var user = await User.GetOrCreateUser(ctx.Member);
                await user.SetLastList(list.Name);
            }
            catch (DuplicateNameException e)
            {
                await ctx.RespondAsync("Liste existiert bereits");
                return;
            }

            await ctx.RespondAsync($"Liste {list.Name} erstellt!");
        }

        [GroupCommand, Description("Eine Liste / List Items anzeigen"), CheckForPermissions(SudoPermission.Me, GuildPermission.Any)]
        public async Task ShowList(CommandContext ctx, string listName = null, string itemName = null)
        {
            if (listName == null)
            {
                var e = new DiscordEmbedBuilder();
                var allLists = await SudoList.GetAllLists(ctx.Member.Id);
                
                e.WithTitle(ctx.Member.Nickname);
                e.WithFooter($"{allLists.Count} Listen");

                foreach (var l in allLists)
                {
                    e.AddField(l.Name, $"{l.Items.Count} Items");
                }

                await ctx.RespondAsync(embed: e.Build());
                return;
            }
            
            int itemsPerPage = 5;
            SudoList list = await SudoList.GetList(ctx.User.Id, listName);
            if (list == null)
            {
                await ctx.RespondAsync("Liste nicht gefunden");
                return;
            }

            var user = await User.GetOrCreateUser(ctx.Member);

            await user.SetLastList(list.Name);
            
            var itemCount = list.Items != null ? list.Items.Count : 0;
            
            var embed = new DiscordEmbedBuilder();
            embed.WithTitle(itemName == null ? listName : $"{listName} / {itemName}");
            embed.WithFooter($"{itemCount} Items");


            if (itemName != null)
            {
                var item = list.Items?.FirstOrDefault(i => i.Name == itemName);

                if (item == null)
                {
                    await ctx.RespondAsync("Item nicht gefunden");
                    return;
                }

                if (item is AnilistItem ai)
                {
                    embed.WithThumbnail(ai.ImageUrl);
                    embed.AddField($"{ai.TotalEpisodes} Episoden", $"{ai.WatchedEpisodes} Gesehen");
                    Uri uri;
                    try
                    {
                        uri = new Uri(ai.Url);
                    }
                    catch (Exception e)
                    {
                        await ctx.RespondAsync("Item hat Invalide URL");
                        return;
                    }

                    embed.WithUrl(ai.Url);

                    await ctx.RespondAsync(embed: embed.Build());
                }
                else
                {
                    await ctx.RespondAsync("Item kann nicht angezeigt werden");
                }
                
                return;
            }

            // var startIndex = page * itemsPerPage;
            var startIndex = 0;
            var currentPage = list.Items?.Skip(startIndex).Take(itemsPerPage);

            if (currentPage != null)
            {
                foreach (var item in currentPage)
                {
                    embed.AddField(item.Name, item.GetTypeName());
                }
            }

            await ctx.RespondAsync(embed: embed.Build());
        }

        [Command("delete"), Description("Liste / List Item Löschen"),
         CheckForPermissions(SudoPermission.Me, GuildPermission.Any)]
        public async Task Delete(CommandContext ctx, string listName, string itemName = null)
        {
            throw new NotImplementedException();
        }
        
        [Command("rename"), Description("Liste / List Item Umbenennen"),
         CheckForPermissions(SudoPermission.Me, GuildPermission.Any)]
        public async Task Rename(CommandContext ctx, string newName, string listName, string itemName = null)
        {
            throw new NotImplementedException();
        }
        
        [Command("add"), Aliases("a"), Description("List Item Hinzufügen"), CheckForPermissions(SudoPermission.Me, GuildPermission.Any)]
        public async Task AddItem(CommandContext ctx, string itemName, string listName, string itemType, string url = null)
        {
            var user = await User.GetOrCreateUser(ctx.Member);
            listName ??= user.LastList;

            if (listName == null)
            {
                await ctx.RespondAsync("Bitte einen Validen List Namen angeben (`$list`)");
                return;
            }

            var list = await SudoList.GetList(ctx.User.Id, listName);
            if (list == null)
            {
                await ctx.RespondAsync("Liste nicht gefunden");
                return;
            }

            if (itemType == null)
            {
                await ctx.RespondAsync("Bitte einen Validen Listen Type angeben (`$list get-types`)");
                return;
            } else if (itemType == "anilist")
            {
                if (url == null)
                {
                    await ctx.RespondAsync("Bitte eine Valide Anilist Url angeben");
                    return;
                }
                
                var item = new AnilistItem(itemName, ctx.User.Id, url);
                try
                {
                    await list.InsertItem(item);
                }
                catch (DuplicateNameException e)
                {
                    await ctx.RespondAsync(e.Message);
                    return;
                }
                catch (OverflowException e)
                {
                    await ctx.RespondAsync(e.Message);
                    return;
                }

                await user.SetLastListItem(item.Name);
                await ctx.RespondAsync("Item Eingefügt");
            }
            else
            {
                await ctx.RespondAsync("Invalider Item type ($list get-types)");
            }
        }
        
        [Command("decrement"), Aliases("d"), Description("List Item Decrementieren"), CheckForPermissions(SudoPermission.Me, GuildPermission.Any)]
        public async Task DecrementItem(CommandContext ctx, string listName = null, string itemName = null)
        {
            var user = await User.GetOrCreateUser(ctx.Member);
            listName ??= user.LastList;
            itemName ??= user.LastListItem;

            if (listName == null)
            {
                await ctx.RespondAsync("Bitte einen Validen List Namen angeben (`$list`)");
                return;
            }
            if (itemName == null)
            {
                await ctx.RespondAsync($"Bitte einen Validen Item Namen angeben (`$list {listName}`)");
                return;
            }

            var list = await SudoList.GetList(ctx.User.Id, listName);
            if (list == null)
            {
                await ctx.RespondAsync("Liste nicht gefunden (`$list`)");
                return;
            }

            var item = list.Items.FirstOrDefault(i => i.Name == itemName);
            if (item == null)
            {
                await ctx.RespondAsync("Item nicht gefunden (`$list {listName}`)");
                return;
            }

            if (item is AnilistItem ai)
            {
                if (ai.WatchedEpisodes > 0)
                    ai.WatchedEpisodes -= 1;
                else
                {
                    await ctx.RespondAsync($"Noch keine Episoden gesehen");
                    return;
                }

                await list.SaveList();
            }
            else
            {
                await ctx.RespondAsync("Item kann nicht decrementiert werden");
                return;
            }
            await ctx.RespondAsync("Done");
        }

        [Command("increment"), Aliases("i"), Description("List Item Incrementieren"), CheckForPermissions(SudoPermission.Me, GuildPermission.Any)]
        public async Task IncrementItem(CommandContext ctx, string listName = null, string itemName = null)
        {
            var user = await User.GetOrCreateUser(ctx.Member);
            listName ??= user.LastList;
            itemName ??= user.LastListItem;

            if (listName == null)
            {
                await ctx.RespondAsync("Bitte einen Validen List Namen angeben (`$list`)");
                return;
            }
            if (itemName == null)
            {
                await ctx.RespondAsync($"Bitte einen Validen Item Namen angeben (`$list {listName}`)");
                return;
            }

            var list = await SudoList.GetList(ctx.User.Id, listName);
            if (list == null)
            {
                await ctx.RespondAsync("Liste nicht gefunden (`$list`)");
                return;
            }

            var item = list.Items.FirstOrDefault(i => i.Name == itemName);
            if (item == null)
            {
                await ctx.RespondAsync("Item nicht gefunden (`$list {listName}`)");
                return;
            }

            if (item is AnilistItem ai)
            {
                if (ai.WatchedEpisodes < ai.TotalEpisodes)
                    ai.WatchedEpisodes += 1;
                else
                {
                    await ctx.RespondAsync($"Bereits Alles von {itemName} Gesehen");
                    return;
                }

                await list.SaveList();
            }
            else
            {
                await ctx.RespondAsync("Item kann nicht incrementiert werden");
                return;
            }

            await ctx.RespondAsync("Done");
        }
        
        [Command("get-types"), Aliases("g"), Description("Mögliche Arten Von list items Anzeigen"), CheckForPermissions(SudoPermission.Me, GuildPermission.Any)]
        public async Task GetPossibleItemTypes(CommandContext ctx)
        {
            string listTypes = "";
            listTypes += "anilist";
            
            await ctx.RespondAsync(listTypes);
        }
    }
}