using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rc.DiscordBot.Models
{
    public class TwitchDiscordServer
    {
        public TwitchDiscordServer()
        {
        }

        public string Name { get; set; } = default!;

        public string Channel { get; set; } = default!;
    }
}
