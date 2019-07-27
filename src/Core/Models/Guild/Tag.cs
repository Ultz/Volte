namespace Volte.Core.Models.Guild
{
    public sealed class Tag
    {
        public string Name { get; set; }
        public string Response { get; set; }
        public ulong CreatorId { get; set; }
        public ulong GuildId { get; set; }
        public long Uses { get; set; }

        public string SanitizeContent() 
            => Response
                .Replace("@everyone", "@\u200Beveryone")
                .Replace("@here", "@\u200Bhere");
    }
}