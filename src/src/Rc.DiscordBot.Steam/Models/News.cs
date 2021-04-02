using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rc.DiscordBot.Models
{
    public record News
    {
        public uint AppId { get; set; } 

        public List<MessageSendToDiscordServer> DiscordServers { get; set; } = new();
    }
}
