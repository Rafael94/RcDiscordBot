using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rc.DiscordBot.Services
{
    public class Lazier<T> : Lazy<T> where T : class
    {
        public Lazier(IServiceProvider provider)
            : base(() => provider.GetRequiredService<T>())
        {
        }
    }
}
