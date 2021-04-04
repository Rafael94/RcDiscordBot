using AutoMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Rc.DiscordBot.Models;
using SteamWebAPI2.Interfaces;
using SteamWebAPI2.Mappings;
using SteamWebAPI2.Utilities;
using System;

namespace Rc.DiscordBot.Steam
{
    public static class DiscordBotSteamModule
    {
        public static void ConfigureServices(HostBuilderContext hostContext, IServiceCollection services)
        {
            SteamConfig? steamConfig = hostContext.Configuration.GetSection("Steam").Get<SteamConfig>();

            if (string.IsNullOrWhiteSpace(steamConfig.ApiKey))
            {
                return;
            }


            services.Configure<SteamConfig>(options => hostContext.Configuration.GetSection("Steam").Bind(options));

            services.AddSingleton((IServiceProvider services) =>
            {
                SteamConfig? options = services.GetRequiredService<IOptions<SteamConfig>>().Value;

                return new SteamWebInterfaceFactory(options.ApiKey);
            });

            services.AddSingleton((IServiceProvider services) =>
            {
                MapperConfiguration? mapperConfig = new MapperConfiguration(config =>
                {
                    config.AddProfile<SteamStoreProfile>();
                });

                IMapper? mapper = mapperConfig.CreateMapper();

                return new SteamStore(mapper);
            });

            services.AddHostedService<SteamWorker>();
        }
    }
}
