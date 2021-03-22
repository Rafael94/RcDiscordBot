using System.Collections.Generic;

namespace Rc.DiscordBot.Models
{
    public class AudioConfig
    {
        public AudioConfig()
        {
            Streams = new Dictionary<string, StreamConfig>();
        }

        public Dictionary<string, StreamConfig> Streams { get; set; }
    }
}
