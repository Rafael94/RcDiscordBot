using System.Collections.Generic;

namespace Rc.DiscordBot.Models
{
    public class AudioConfig
    {
        public AudioConfig()
        {
            Streams = new ();
        }

        public List<StreamConfig> Streams { get; set; }
    }
}
