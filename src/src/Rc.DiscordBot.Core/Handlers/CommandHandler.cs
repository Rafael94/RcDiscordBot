using System.Collections.Generic;
using System.Reflection;

namespace Rc.DiscordBot.Handlers
{
    public class CommandHandler
    {
        public List<Assembly> Assemblies { get; set; } = new();
    }
}
