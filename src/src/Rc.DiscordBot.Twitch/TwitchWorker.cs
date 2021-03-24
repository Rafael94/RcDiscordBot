using Microsoft.Extensions.Hosting;
using Rc.DiscordBot.Services;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Rc.DiscordBot
{
    public class TwitchWorker : BackgroundService
    {
        private readonly TwitchLiveMonitorService _liveMonitoringService;

        public TwitchWorker(TwitchLiveMonitorService liveMonitoringService)
        {
            _liveMonitoringService = liveMonitoringService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _liveMonitoringService.ConfigLiveMonitor();

            try
            {
                stoppingToken.ThrowIfCancellationRequested();
                while (!stoppingToken.IsCancellationRequested)
                {
                    // Do async work
                    await Task.Delay(int.MaxValue, stoppingToken);
                }
            }
            catch (Exception)
            {
            }
            finally
            {
                _liveMonitoringService.Stop();
            }
        }
    }
}
