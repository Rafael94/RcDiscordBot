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
    public static class DiscordBotCoreModule
    {
        public static void ConfigureServices(HostBuilderContext hostContext, IServiceCollection services) 
        {
            // Konfigurations
            services.Configure<BotConfig>(options => hostContext.Configuration.GetSection("Discord").Bind(options));

            services.AddSingleton<DiscordService>();
            services.AddSingleton<DiscordSocketClient>();
          
            services.AddSingleton<CommandService>();
            
        }
    }
}
