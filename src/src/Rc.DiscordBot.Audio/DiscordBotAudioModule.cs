
using Lavalink4NET;
using Lavalink4NET.DSharpPlus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Rc.DiscordBot.Handlers;
using Rc.DiscordBot.Models;
using System.Reflection;

namespace Rc.DiscordBot
{
    public static class DiscordBotAudioModule
    {
        public static void ConfigureServices(HostBuilderContext hostContext, IServiceCollection services)
        {
            services
                .Configure<AudioConfig>(options => hostContext.Configuration.GetSection("Audio").Bind(options))
                .Configure<LavaConfig>(options => hostContext.Configuration.GetSection("Lavalink").Bind(options))
                .PostConfigure<CommandHandler>((commandHandler) => commandHandler.Assemblies.Add(Assembly.GetExecutingAssembly()))
                .AddSingleton<IHostedService, AudioHostedService>()
                .AddSingleton<IAudioService, LavalinkNode>()
                .AddSingleton<IDiscordClientWrapper, DiscordClientWrapper>()
                .AddSingleton((services) =>
                {
                    var config = services.GetRequiredService<IOptions<LavaConfig>>().Value;

                    return new LavalinkNodeOptions
                    {
                        Password = config.Password,
                        RestUri = $"http{(config.Secured ? "s" : "")}://{config.Host}:{config.Port}",
                        WebSocketUri = $"ws://{config.Host}:{config.Port}",
                        AllowResuming = false,
                        SessionTimeout = 10
                    };
                });

        }
    }
}
