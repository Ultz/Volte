using Discord;
using Discord.WebSocket;
using Volte.Core;
using Volte.Services;

namespace Volte.Data.Objects.EventArgs
{
    public sealed class UserJoinedEventArgs : System.EventArgs
    {
        private readonly DatabaseService _db = VolteBot.GetRequiredService<DatabaseService>();
        public IGuildUser User { get; }
        public IGuild Guild { get; }
        public GuildConfiguration Config { get; }
        public DiscordSocketClient Client { get; }

        public UserJoinedEventArgs(IGuildUser user)
        {
            User = user;
            Guild = user.Guild;
            Config = _db.GetConfig(Guild);
            Client = VolteBot.Client;
        }
    }
}