using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rc.DiscordBot.Models
{
    public class TwitchChannel
    {
        public TwitchChannel()
        {
            DiscordServers = new();
        }

        public string DisplayName { get; set; } = default!;

        public bool NotificationWhenOnline { get; set; } = true;
        public bool NotificationWhenOffline { get; set; } = true;
        public bool NotificationWhenUpdated { get; set; } = true;

        /// <summary>
        /// Key => Servername
        /// Value => Channels
        /// </summary>
        public List<TwitchDiscordServer> DiscordServers { get; set; }
    }
}
