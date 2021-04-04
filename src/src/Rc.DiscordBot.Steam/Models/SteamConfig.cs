using System;
using System.Collections.Generic;

namespace Rc.DiscordBot.Models
{
    public record SteamConfig
    {
        public TimeSpan Interval { get; set; } = TimeSpan.FromHours(2);

        public string? ApiKey { get; set; }

        public List<News> News { get; set; } = new();
    }
}
