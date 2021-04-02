﻿using System.Collections.Generic;

namespace Rc.DiscordBot.Models
{
    public record Feed
    {
        public string Name { get; set; } = default!;
        public string Url { get; set; } = default!;

        public List<MessageSendToDiscordServer> DiscordServers { get; set; } = new();
    }
}
