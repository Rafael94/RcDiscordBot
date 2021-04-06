﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Rc.DiscordBot.Handlers;
using Rc.DiscordBot.Steam;
using Serilog;
using System;
using System.IO;
using System.Reflection;

namespace Rc.DiscordBot
{
    public class DiscordBotWorkerModule
    {
        public static void ConfigureLog()
        {
            IConfigurationRoot? configuration = new ConfigurationBuilder()
               .SetBasePath(Directory.GetCurrentDirectory())
               .AddJsonFile("appsettings.json")
               .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", true)
               .Build();


            Log.Logger = new LoggerConfiguration()
                 .ReadFrom.Configuration(configuration)
                .CreateLogger();

        }
        public static void ConfigureServices(HostBuilderContext hostContext, IServiceCollection services)
        {
            services.AddHostedService<Worker>();

            DiscordBotCoreModule.ConfigureServices(hostContext, services);
            DiscordBotAudioModule.ConfigureServices(hostContext, services);
            DiscordBotTwitchModule.ConfigureServices(hostContext, services);
            DiscordBotRssModule.ConfigureServices(hostContext, services);
            DiscordBotSteamModule.ConfigureServices(hostContext, services);

            //services.AddSingleton((IServiceProvider provider) =>
            //{
                //CommandHandler? commandHandler = (CommandHandler)ActivatorUtilities.CreateInstance(provider, typeof(CommandHandler));

                //commandHandler.AddModulesAsync(Assembly.GetAssembly(typeof(DiscordBotCoreModule))!).GetAwaiter().GetResult();
                //commandHandler.AddModulesAsync(Assembly.GetAssembly(typeof(DiscordBotAudioModule))!).GetAwaiter().GetResult();
               // commandHandler.AddModulesAsync(Assembly.GetAssembly(typeof(DiscordBotTwitchModule))!).GetAwaiter().GetResult();
               //commandHandler.AddModulesAsync(Assembly.GetAssembly(typeof(DiscordBotRssModule))!).GetAwaiter().GetResult();
                //commandHandler.AddModulesAsync(Assembly.GetAssembly(typeof(DiscordBotSteamModule))!).GetAwaiter().GetResult();

                //return commandHandler;
           // });

        }
    }
}
