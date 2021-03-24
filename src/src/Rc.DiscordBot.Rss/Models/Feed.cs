using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rc.DiscordBot.Models
{
    public record Feed
    {
        public Feed()
        {
            DiscordServers = new();
        }

        public string Name { get; set; } = default!;
        public string Url { get; set; } = default!;

        public List<MessageSendToDiscordServer> DiscordServers { get; set; }
    }
}
