using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Rc.DiscordBot.Handlers;
using Rc.DiscordBot.Models;
using Rc.DiscordBot.Modules;
using Rc.DiscordBot.Services;

namespace Rc.DiscordBot
{
    public static class DiscordBotAudioModule
    {
        public static void ConfigureServices(HostBuilderContext hostContext, IServiceCollection services) 
        {
            services.Configure<AudioConfig>(options => hostContext.Configuration.GetSection("Audio").Bind(options));

            services.AddSingleton<AudioModule>();
            services.AddSingleton<AudioService>();
        }
    }
}
