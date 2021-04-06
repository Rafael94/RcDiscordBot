
using DSharpPlus;
using DSharpPlus.CommandsNext;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Rc.DiscordBot.Handlers;
using Rc.DiscordBot.Models;
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
            services.AddSingleton((serviceProvider) =>
            {
                var config = serviceProvider.GetRequiredService<IOptions<BotConfig>>().Value;
                var handler = serviceProvider.GetRequiredService<IOptions<CommandHandler>>().Value;

                var client = new DiscordClient(new DiscordConfiguration()
                {
                    Token = config.BotToken,
                    TokenType = TokenType.Bot,
                    Intents = DiscordIntents.AllUnprivileged,                 
                });
               
                var commands = client.UseCommandsNext(new CommandsNextConfiguration()
                {
                    StringPrefixes = new[] { config.Prefix },
                    Services = serviceProvider
                });

                foreach(var assembly in handler.Assemblies)
                {
                    commands.RegisterCommands(assembly);
                }
                
                return client;
            });

        }
    }
}
