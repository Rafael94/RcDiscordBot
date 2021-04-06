using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Rc.DiscordBot.Handlers;
using Rc.DiscordBot.Models;
using System.Reflection;

namespace Rc.DiscordBot
{
    public static class DiscordBotRssModule
    {
        public static void ConfigureServices(HostBuilderContext hostContext, IServiceCollection services)
        {
            services.Configure<RssConfig>(options => hostContext.Configuration.GetSection("Rss").Bind(options));

            services.PostConfigure<CommandHandler>((commandHandler) => commandHandler.Assemblies.Add(Assembly.GetExecutingAssembly()));

            services.AddHostedService<RssWorker>();
        }
    }
}
