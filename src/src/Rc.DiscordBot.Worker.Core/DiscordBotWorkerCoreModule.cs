using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Rc.DiscordBot.Handlers;
using Rc.DiscordBot.Modules;
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

            services.AddSingleton((IServiceProvider provider) =>
            {
                var commandHandler = (CommandHandler)ActivatorUtilities.CreateInstance(provider, typeof(CommandHandler));

                commandHandler.AddModulesAsync(Assembly.GetAssembly(typeof(DiscordBotCoreModule))!).GetAwaiter().GetResult();
                commandHandler.AddModulesAsync(Assembly.GetAssembly(typeof(DiscordBotAudioModule))!).GetAwaiter().GetResult();
                //commandHandler.AddModulesAsync(Assembly.GetAssembly(typeof(DiscordBotTwitchModule))!).GetAwaiter().GetResult();

                return commandHandler;
            });

        }
    }
}
