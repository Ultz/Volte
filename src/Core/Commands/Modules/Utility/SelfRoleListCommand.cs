using System.Linq;
using System.Threading.Tasks;
using Discord;
using Qmmands;
using Volte.Core.Extensions;

namespace Volte.Core.Commands.Modules.Utility
{
    public partial class UtilityModule : VolteModule
    {
        [Command("SelfRoleList", "Srl")]
        [Description("Gets a list of self roles available for this guild.")]
        [Remarks("Usage: |prefix|selfrolelist")]
        public async Task SelfRoleListAsync()
        {
            var roleList = string.Empty;
            var config = Db.GetConfig(Context.Guild);
            if (config.SelfRoles.Count > 0)
            {
                config.SelfRoles.ForEach(role =>
                {
                    var currentRole = Context.Guild.Roles.FirstOrDefault(r => r.Name.EqualsIgnoreCase(role));
                    roleList += $"**{currentRole?.Name}**\n";
                });
                await Context.CreateEmbed(roleList).SendTo(Context.Channel);
            }
            else
            {
                roleList = "No roles available to self-assign in this guild.";
                await Context.CreateEmbed(roleList).SendTo(Context.Channel);
            }
        }
    }
}