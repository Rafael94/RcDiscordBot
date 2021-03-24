using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Rc.DiscordBot.Handlers;
using System;
using System.Reflection;

namespace Rc.DiscordBot
{
    public class DiscordBotWorkerCoreModule
    {
        public static void ConfigureServices(HostBuilderContext hostContext, IServiceCollection services)
        {
            services.AddHostedService<Worker>();

            DiscordBotCoreModule.ConfigureServices(hostContext, services);
            DiscordBotAudioModule.ConfigureServices(hostContext, services);
            DiscordBotTwitchModule.ConfigureServices(hostContext, services);
            DiscordBotRssModule.ConfigureServices(hostContext, services);

            services.AddSingleton((IServiceProvider provider) =>
            {
                CommandHandler? commandHandler = (CommandHandler)ActivatorUtilities.CreateInstance(provider, typeof(CommandHandler));

                commandHandler.AddModulesAsync(Assembly.GetAssembly(typeof(DiscordBotCoreModule))!).GetAwaiter().GetResult();
                commandHandler.AddModulesAsync(Assembly.GetAssembly(typeof(DiscordBotAudioModule))!).GetAwaiter().GetResult();
                commandHandler.AddModulesAsync(Assembly.GetAssembly(typeof(DiscordBotTwitchModule))!).GetAwaiter().GetResult();
                commandHandler.AddModulesAsync(Assembly.GetAssembly(typeof(DiscordBotRssModule))!).GetAwaiter().GetResult();

                return commandHandler;
            });

        }
    }
}
