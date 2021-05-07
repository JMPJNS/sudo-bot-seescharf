using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using SudoBot.Database;
using DateTime = System.DateTime;

namespace SudoBot.Models
{
    public class User
    {
        [BsonId]
        public ObjectId ThisIsNotTheId { get; set; }
        public ulong UserId { private set; get; }
        public string UserName { get; private set; }
        public string Discriminator { get; private set; }
        
        public ulong GuildId { private set; get; }

        public bool Blocked { private set; get; }
        
        public List<UserPermissions> Permissions { get; private set; }
        
        public DateTime LastUpdated { private set; get; }

        
        public DateTime JoinDate { private set; get; }

        public int SpecialPoints { private set; get; }
        
        public int CountedMessages { private set; get; }
        
        public long Points { private set; get; }
        
        public string LastList { private set; get; }
        public string LastListItem { private set; get; }
        
        public int TicketsRemaining { private set; get; }

        // Logic Starts Here

        
        public long CalculatePoints()
        {
            var guild = Guild.GetGuild(GuildId).GetAwaiter().GetResult();
            int messages = CountedMessages;
            int specialPointspoints = SpecialPoints;
            
            int days = (int)(DateTime.UtcNow - JoinDate).TotalDays;
            Points = messages + specialPointspoints + days*guild.RankingTimeMultiplier;
            SaveUser().GetAwaiter().GetResult();
            return Points;

        }

        public async Task RemovePermission(UserPermissions perm)
        {
            Permissions.Remove(perm);
            await SaveUser();
        }

        public async Task AddPermission(UserPermissions perm)
        {
            var found = Permissions.Contains(perm);

            if (found) return;
            
            Permissions.Add(perm);
            await SaveUser();
        }
        
        public async Task RemoveTicket()
        {
            TicketsRemaining--;
            await SaveUser();
        }

        public async Task AddTickets(int count)
        {
            TicketsRemaining += count;
            await SaveUser();
        }

        public async Task<long> GetRank()
        {
            return await Mongo.Instance.GetUserRank(this) + 1;
        }
        
        public async Task<bool> AddCountedMessages(DiscordMessage message)
        {
            if (Blocked) return false;
            
            TimeSpan minDelay = TimeSpan.FromMinutes(10);

            if (DateTime.UtcNow - minDelay <= LastUpdated) return false;

            var guild = await Guild.GetGuild(GuildId);

            int countedMessageLength = 10;
            int multiplier;

            if (guild.RankingMultiplier == 0)
            {
                multiplier = 1;
                
            }
            else
            {
                multiplier = guild.RankingMultiplier;
            }
            
            int count = (message.Content.Length + countedMessageLength)/countedMessageLength;

            LastUpdated = DateTime.UtcNow;
            CountedMessages += count * multiplier;

            var rankUpdated = await UpdateRankRoles();

            await SaveUser();
            
            return true;
        }

        public async Task AddCountedMessagesByHand(int count)
        {
            CountedMessages += count;
            await SaveUser();
        }

        public async Task<CurrentNextRoleReturnType> GetCurrentAndNextRole()
        {
            Guild guild = await Guild.GetGuild(GuildId);
            var r = new CurrentNextRoleReturnType();
            if (guild.RankingRoles == null || guild.RankingRoles.Count == 0) return r;
            
            var dGuild = await Globals.Client.GetGuildAsync(GuildId);
            
            var sorted = guild.RankingRoles.OrderBy(x => x.Points).ToList();

            var current = sorted.LastOrDefault(x => x.Points < Points);
            var next = sorted.FirstOrDefault(x => x.Points > Points);


            r.Current = current != null ? dGuild.GetRole(current.Role) : null;
            
            if (next != null)
            {
                r.Next = dGuild.GetRole(next.Role);

                r.Remaining = next.Points - Points;
            }
            else
            {
                r.Next = null;
                r.Remaining = 0;
            }

            return r;
        }

        public struct CurrentNextRoleReturnType
        {
            public DiscordRole Current;
            public DiscordRole Next;
            public long Remaining;
        }

        public async Task<bool> UpdateRankRoles()
        {
            Guild guild = await Guild.GetGuild(GuildId);
            if (guild.RankingRoles == null || guild.RankingRoles.Count == 0) return false;
            
            var dGuild = await Globals.Client.GetGuildAsync(GuildId);
            var member = await dGuild.GetMemberAsync(UserId);

            var xp = CalculatePoints();

            bool rvalue = false;

            var sorted = guild.RankingRoles.OrderBy(x => x.Points).ToList();

            for (var i=0; i< sorted.Count; i++)
            {
                var r = sorted[i];
                Guild.RankingRole next;
                if (i < sorted.Count - 1) 
                     next = sorted[i + 1];
                else next = null;
                
                var role = dGuild.GetRole(r.Role);
                if (role == null)
                {
                    await guild.RemoveRankingRole(r.Role);
                }
                try
                {
                    if (xp > r.Points && xp < (next != null ? next.Points : int.MaxValue))
                    {
                        if (member.Roles.Contains(role)) continue;
                        rvalue = true;
                        await member.GrantRoleAsync(role);
                    }
                    else
                    {
                        if (!member.Roles.Contains(role)) continue;
                        await member.RevokeRoleAsync(role);
                    }
                }
                catch (Exception e)
                {
                    if (e.Message == "Unauthorized: 403")
                    {
                        if (guild.LocalLogChannel != 0)
                        {
                            await dGuild.GetChannel(guild.LocalLogChannel)
                                .SendMessageAsync(
                                    $"Der Bot hat keine Berechtigung die Rolle {role.Mention} zu vergeben");
                        }
                        else
                        {
                            throw new Exception("NO LOG CHANNEL");
                        }
                    }
                }
                
                
            }

            return rvalue;
        }

        public async Task AddSpecialPoints(int points)
        {
            SpecialPoints += points;
            await SaveUser();
        }

        public async Task UpdateUser(DiscordMember member)
        {
            UserName = member.Username;
            Discriminator = member.Discriminator;

            await SaveUser();
        }

        
        // Database Management stuff here

        public static async Task<User> GetOrCreateUser(DiscordMember member)
        {
            User user = await Mongo.Instance.GetUser(member.Id, member.Guild.Id);
            if (user != null)
            {
                user.UserName ??= member.Username;
                user.Discriminator ??= member.Discriminator;
                user.Permissions ??= new List<UserPermissions>();

                return user;
            }

            user = GetFreshUser(member);
            await Mongo.Instance.InsertUser(user);
            return user;
        }

        public async Task SetLastList(string name)
        {
            LastList = name;
            await SaveUser();
        }
        
        public async Task SetLastListItem(string name)
        {
            LastListItem = name;
            await SaveUser();
        }
        
        private static User GetFreshUser(DiscordMember member)
        {
            User u =  new User(
                member.Id,
                member.Guild.Id,
                member.JoinedAt,
                0,
                0,
                false
            );
            u.UserName = member.Username;
            u.Discriminator = member.Discriminator;
            return u;
        }

        private async Task SaveUser()
        {
            await Mongo.Instance.UpdateUser(this);
        }
        
        private User(ulong userId, ulong guildId, DateTimeOffset joinDate, int countedMessages, int specialPoints, bool blocked)
        {
            UserId = userId;
            GuildId = guildId;
            JoinDate = joinDate.DateTime;

            Blocked = blocked;

            CountedMessages = countedMessages;
            SpecialPoints = specialPoints;

            Permissions = new List<UserPermissions>();
            
            LastUpdated = DateTime.UtcNow;
            TicketsRemaining = 1;
        }
        
        public User() {}
    }
}