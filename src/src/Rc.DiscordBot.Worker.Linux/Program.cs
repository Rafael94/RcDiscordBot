using Microsoft.Extensions.Hosting;

namespace Rc.DiscordBot
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
            .UseSystemd()
            .ConfigureServices((hostContext, services) =>
            {
                DiscordBotWorkerCoreModule.ConfigureServices(hostContext, services);
            });
        }
    }
}
