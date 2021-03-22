using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Rc.DiscordBot.Models;
using Rc.DiscordBot.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (_liveMonitoringService.ConfigLiveMonitor())
            {
                stoppingToken.WaitHandle.WaitOne();

                _liveMonitoringService.Stop();
            }

            return Task.CompletedTask;
        }
    }
}
