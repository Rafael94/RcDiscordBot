using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Rc.DiscordBot.Handlers;
using Rc.DiscordBot.Models;
using Rc.DiscordBot.Services;
using System.Reflection;

namespace Rc.DiscordBot
{
    public static class DiscordBotTwitchModule
    {
        public static void ConfigureServices(HostBuilderContext hostContext, IServiceCollection services)
        {
            TwitchConfig? twitchConfig = hostContext.Configuration.GetSection("Twitch").Get<TwitchConfig>();

            if (string.IsNullOrWhiteSpace(twitchConfig.Secret) || string.IsNullOrWhiteSpace(twitchConfig.ClientId))
            {
                return;
            }

            if (twitchConfig.TwitchChannels?.Count == 0)
            {
                return;
            }

            services.PostConfigure<CommandHandler>((commandHandler) => commandHandler.Assemblies.Add(Assembly.GetExecutingAssembly()));

            services.Configure<TwitchConfig>(options => hostContext.Configuration.GetSection("Twitch").Bind(options));

            services.AddSingleton<TwitchLiveMonitorService>();
            services.AddHostedService<TwitchWorker>();
        }
    }
}
