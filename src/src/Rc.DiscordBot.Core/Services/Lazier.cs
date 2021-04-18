using Microsoft.Extensions.DependencyInjection;
using System;

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
