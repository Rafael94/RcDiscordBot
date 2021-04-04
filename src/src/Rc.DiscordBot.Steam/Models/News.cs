using System.Collections.Generic;

namespace Rc.DiscordBot.Models
{
    public record News
    {
        public uint AppId { get; set; }

        public string Name { get; set; } = default!;

        public string[]? Tags { get; set; }

        public string? Feeds { get; set; }

        public List<MessageSendToDiscordServer> DiscordServers { get; set; } = new();
    }
}
