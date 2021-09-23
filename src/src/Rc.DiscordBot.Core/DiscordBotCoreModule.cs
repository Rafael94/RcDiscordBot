
using DSharpPlus;
using DSharpPlus.CommandsNext;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Rc.DiscordBot.Handlers;
using Rc.DiscordBot.Models;
using Rc.DiscordBot.Services;
using System;
using System.Reflection;

namespace Rc.DiscordBot
{
    public static class DiscordBotCoreModule
    {
        public static void ConfigureServices(HostBuilderContext hostContext, IServiceCollection services)
        {
            // Konfigurations
            services.Configure<BotConfig>(options => hostContext.Configuration.GetSection("Discord").Bind(options));

            services.PostConfigure<CommandHandler>((commandHandler) => commandHandler.Assemblies.Add(Assembly.GetExecutingAssembly()));

            services.AddSingleton<DiscordService>();
            services.AddSingleton((serviceProvider) =>
            {
                BotConfig? config = serviceProvider.GetRequiredService<IOptions<BotConfig>>().Value;
                CommandHandler? handler = serviceProvider.GetRequiredService<IOptions<CommandHandler>>().Value;

                DiscordClient? client = new(new DiscordConfiguration()
                {
                    Token = config.BotToken,
                    TokenType = TokenType.Bot,
                    Intents = DiscordIntents.AllUnprivileged,
                });

                CommandsNextExtension? commands = client.UseCommandsNext(new CommandsNextConfiguration()
                {
                    StringPrefixes = new[] { config.Prefix },
                    Services = serviceProvider
                });

                foreach (Assembly? assembly in handler.Assemblies)
                {
                    commands.RegisterCommands(assembly);
                }

                return client;
            });

            services.AddTransient(typeof(Lazy<>), typeof(Lazier<>));
        }
    }
}
