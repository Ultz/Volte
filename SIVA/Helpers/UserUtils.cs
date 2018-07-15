﻿using Discord.WebSocket;
using SIVA.Core.Files.Readers;
using System.Linq;
using Discord.Commands;

namespace SIVA.Helpers
{
    public class UserUtils
    {
        /// <summary>
        ///     Checks if the user given is the bot owner.
        /// </summary>
        /// <param name="user">User to check.</param>
        /// <returns>true; if the user is the bot owner.</returns>
        
        public static bool IsBotOwner(SocketUser user)
        {
            return user.Id == Config.conf.Owner;
        }

        /// <summary>
        ///     Checks if the given SocketGuildUser has the given SocketRole.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="role"></param>
        /// <returns></returns>
        
        public static bool HasRole(SocketGuildUser user, SocketRole role)
        {
            return user.Roles.Contains(role);
        }

        /// <summary>
        ///     Checks if the given SocketGuildUser has the given SocketRole Id.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="roleId"></param>
        /// <returns></returns>
        
        public static bool HasRole(SocketGuildUser user, ulong roleId)
        {
            return user.Roles.Contains(user.Guild.Roles.First(r => r.Id == roleId));
        }

        public static bool IsAdmin(SocketCommandContext ctx)
        {
            var config = ServerConfig.Get(ctx.Guild);
            var adminRole = ctx.Guild.Roles.FirstOrDefault(r => r.Id == config.AdminRole);
            return adminRole != null && ((SocketGuildUser)ctx.User).Roles.Contains(adminRole);
        }
    }
}