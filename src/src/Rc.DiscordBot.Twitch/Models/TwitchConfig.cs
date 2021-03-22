using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rc.DiscordBot.Models
{
    public class TwitchConfig
    {
        public string? ClientId { get; set; }
        public string? Secret { get; set; }
        public int ThumbnailWidth { get; set; } = 1024;
        public int ThumbnailHeight { get; set; } = 576;
        public short OnlineCheckIntervall { get; set; } = 60;
        public Dictionary<string, TwitchChannel>? TwitchChannels { get; set; }

        public string ChannelOnline { get; set; } = "Der Channel '@ChannelName' ist online gegangen.";
        public string ChannelOffline { get; set; } = "Der Channel '@ChannelName' ist offline gegangen.";
        public string ChannelUpdated { get; set; } = "Der Channel '@ChannelName' wurde aktualisiert.";
    }
}
