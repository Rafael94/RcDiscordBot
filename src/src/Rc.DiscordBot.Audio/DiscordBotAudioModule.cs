using DSharpPlus;
using DSharpPlus.Lavalink;
using DSharpPlus.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Rc.DiscordBot.Handlers;
using Rc.DiscordBot.Models;
using Rc.DiscordBot.Modules;
using Rc.DiscordBot.Services;
using System;
using System.Reflection;

namespace Rc.DiscordBot
{
    public static class DiscordBotAudioModule
    {
        public static void ConfigureServices(HostBuilderContext hostContext, IServiceCollection services)
        {
            services.Configure<AudioConfig>(options => hostContext.Configuration.GetSection("Audio").Bind(options));

            services.Configure<LavaConfig>(options => hostContext.Configuration.GetSection("Lavalink").Bind(options));

            services.AddSingleton<LavaLinkAudio>()
                    .AddSingleton<IHostedService, AudioHostedService>();

            services.PostConfigure<CommandHandler>((commandHandler) => commandHandler.Assemblies.Add(Assembly.GetExecutingAssembly()));
            /*.AddSingleton<AudioModule>()

            .AddSingleton<LavaNode>()
            .AddSingleton((IServiceProvider services) =>
            {
                return services.GetRequiredService<IOptions<LavaConfig>>().Value;
            });*/
        }
    }
}
