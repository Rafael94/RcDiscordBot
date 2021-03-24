using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Rc.DiscordBot.Services;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Rc.DiscordBot
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly DiscordService _discordService;

        public Worker(ILogger<Worker> logger, DiscordService discordService)
        {
            _logger = logger;
            _discordService = discordService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Start Discord");

            try
            {
                stoppingToken.ThrowIfCancellationRequested();
                await _discordService.StartAsync();

                // Warten bis der BackgroundService beendet wird
                while (!stoppingToken.IsCancellationRequested)
                {
                    await Task.Delay(int.MaxValue, stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fehler beim Discord Service");
            }
            finally
            {
              await   _discordService.StopAsync();
            }
        }
    }
}
