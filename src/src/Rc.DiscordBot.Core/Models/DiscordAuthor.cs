using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rc.DiscordBot.Models
{
    public record DiscordAuthor(string Name, string? Url = null, string? IconUrl = null);
}