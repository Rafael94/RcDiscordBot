using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
