using System;
using System.Collections.Generic;

namespace Rc.DiscordBot.Models
{
    public record RssConfig
    {
        public RssConfig()
        {
            Feeds = new();
        }

        public TimeSpan Interval { get; set; } = TimeSpan.FromHours(2);

        public List<Feed> Feeds { get; set; }
    }
}
