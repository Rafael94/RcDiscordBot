using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Rc.DiscordBot.Models;

namespace Rc.DiscordBot
{
    public static class DiscordBotRssModule
    {
        public static void ConfigureServices(HostBuilderContext hostContext, IServiceCollection services)
        {
            services.Configure<RssConfig>(options => hostContext.Configuration.GetSection("Rss").Bind(options));

            services.AddHostedService<RssWorker>();
        }
    }
}
