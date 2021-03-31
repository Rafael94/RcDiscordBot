using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Rc.DiscordBot
{
    public class Program
    {
        private const Os
        public static async Task<int> Main(string[] args)
        {
            ConfigureLog();
            try
            {
                await CreateHostBuilder(args).Build().RunAsync();
            }
            catch (System.Exception ex)
            {
                Log.Fatal(ex, "Host terminated unexpectedly!");
                return 1;
            }
            finally
            {
                Log.CloseAndFlush();
            }
            
            return 0;
        }

        private static void ConfigureLog()
        {
            var configuration = new ConfigurationBuilder()
               .SetBasePath(Directory.GetCurrentDirectory())
               .AddJsonFile("appsettings.json")
               .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")}.json", true)
               .Build();

            Log.Logger = new LoggerConfiguration()
                 .ReadFrom.Configuration(configuration)
                .CreateLogger();

        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
#if WINDOWS
                .UseWindowsService()
#elif LINUX
                .UseSystemd()
#endif
                .UseSerilog()
                .ConfigureServices((hostContext, services) =>
                {
                    DiscordBotWorkerModule.ConfigureServices(hostContext, services);
                });
        }
    }
}
