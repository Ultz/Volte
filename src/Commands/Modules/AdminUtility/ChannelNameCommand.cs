﻿using System.Threading.Tasks;
using DSharpPlus;
using Qmmands;
using Volte.Commands.Checks;
using Volte.Commands.Results;

namespace Volte.Commands.Modules
{
    public sealed partial class AdminUtilityModule
    {
        [Command("ChannelName", "Cn")]
        [Description("Sets the name of the current channel. Replaces all spaces with a -.")]
        [Remarks("channelname {String}")]
        [RequireBotChannelPermission(Permissions.ManageChannels)]
        public async Task<ActionResult> ChannelNameAsync([Remainder]string name)
        {
            await Context.Channel.ModifyAsync(c => c.Name = name.Replace(" ", "-"));
            return Ok($"Set this channel's name to **{name}**.");
        }
    }
}