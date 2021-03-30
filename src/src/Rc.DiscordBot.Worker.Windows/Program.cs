using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using System.Threading.Tasks;

namespace Rc.DiscordBot
{
    public class Program
    {
        public static async Task<int> Main(string[] args)
        {
            DiscordBotWorkerCoreModule.ConfigureLog();
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

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .UseWindowsService()
                .UseSerilog()
                .ConfigureServices((hostContext, services) =>
                {
                    DiscordBotWorkerCoreModule.ConfigureServices(hostContext, services);
                });
        }
    }
}
