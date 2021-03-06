﻿using System.Threading.Tasks;
using Qmmands;
using Volte.Core.Entities;
using Volte.Commands.Results;

namespace Volte.Commands.Modules
{
    public sealed partial class AdminModule
    {
        [Command("ServerPrefix", "Sp", "GuildPrefix", "Gp")]
        [Description("Sets the command prefix for this guild.")]
        [Remarks("serverprefix {String}")]
        [RequireGuildAdmin]
        public Task<ActionResult> ServerPrefixAsync([Remainder] string newPrefix)
        {
            Context.GuildData.Configuration.CommandPrefix = newPrefix;
            Db.UpdateData(Context.GuildData);
            return Ok($"Set this guild's prefix to **{newPrefix}**.");
        }
    }
}