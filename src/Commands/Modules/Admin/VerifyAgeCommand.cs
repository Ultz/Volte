using System.Threading.Tasks;
using Qmmands;
using Volte.Commands.Results;

namespace Volte.Commands.Modules
{
    public partial class AdminModule
    {
        [Command("VerifyAge", "Va")]
        [Description("Enables or disables the automatic account age warnings when a user who joins has a very young account (30 days or less).")]
        [Remarks("verifyage {Boolean}")]
        public Task<ActionResult> VerifyAgeAutomaticallyAsync(bool enabled)
        {
            Context.GuildData.Configuration.Moderation.CheckAccountAge = enabled;
            Db.UpdateData(Context.GuildData);
            return Ok(enabled ? "Account age detection has been enabled." : "Account age detection has been disabled.");
        }
    }
}