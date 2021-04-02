using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Rc.DiscordBot.Models;
using SteamWebAPI2.Utilities;
using System;

namespace Rc.DiscordBot.Steam
{
    public static class DiscordBotSteamModule
    {
        public static void ConfigureServices(HostBuilderContext hostContext, IServiceCollection services)
        {
            SteamConfig? twitchConfig = hostContext.Configuration.GetSection("Steam").Get<SteamConfig>();

            if (string.IsNullOrWhiteSpace(twitchConfig.ApiKey))
            {
                return;
            }


            services.Configure<SteamConfig>(options => hostContext.Configuration.GetSection("Steam").Bind(options));

            services.AddSingleton((IServiceProvider services) =>
            {
                var options = services.GetRequiredService<IOptions<SteamConfig>>().Value;

                return new SteamWebInterfaceFactory(options.ApiKey);
            });
        }
    }
}
