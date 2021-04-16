using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rc.DiscordBot.Models
{
    public record LavaConfig
    {
        public string Password { get; set; } = default!;
        public string Host { get; set; } = default!;
        public bool Secured { get; set; } = false;
        public int Port { get; set; } = 2333;
    }
}
