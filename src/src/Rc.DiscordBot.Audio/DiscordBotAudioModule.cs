using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Rc.DiscordBot.Models;
using Rc.DiscordBot.Modules;
using Rc.DiscordBot.Services;
using System;
using Victoria;

namespace Rc.DiscordBot
{
    public static class DiscordBotAudioModule
    {
        public static void ConfigureServices(HostBuilderContext hostContext, IServiceCollection services)
        {
            services.Configure<AudioConfig>(options => hostContext.Configuration.GetSection("Audio").Bind(options));
            services.Configure<LavaConfig>(options => hostContext.Configuration.GetSection("Lavalink").Bind(options));

            services.AddSingleton<AudioModule>()
                .AddSingleton<LavaLinkAudio>()
                .AddSingleton<LavaNode>()
                .AddSingleton((IServiceProvider services) =>
                {
                    return services.GetRequiredService<IOptions<LavaConfig>>().Value;
                });
        }
    }
}
