
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Rc.DiscordBot.Handlers;
using Rc.DiscordBot.Models;
using Rc.DiscordBot.Services;
using SteamWebAPI2.Interfaces;
using SteamWebAPI2.Utilities;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Rc.DiscordBot
{
    public class SteamWorker : BackgroundService
    {
        public SteamWorker(IOptions<SteamConfig> steamConfig, SteamWebInterfaceFactory steamWebInterfaceFactory, ILogger<SteamWorker> logger, DiscordService discordService)
        {
            _steamConfig = steamConfig.Value;
            _steamWebInterfaceFactory = steamWebInterfaceFactory;
            _logger = logger;
            _discordService = discordService;
        }

        private readonly SteamConfig _steamConfig;
        private readonly SteamWebInterfaceFactory _steamWebInterfaceFactory;
        private readonly ILogger<SteamWorker> _logger;
        private readonly DiscordService _discordService;

        private DateTime _lastCheck = DateTime.Now;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {

            try
            {
                stoppingToken.ThrowIfCancellationRequested();
                _logger.LogInformation($"Init Steam News Timer with Intervall {_steamConfig.Interval}");
                using Timer? timer = new(HandleTimerCallback, null, _steamConfig.Interval, _steamConfig.Interval);

                while (!stoppingToken.IsCancellationRequested)
                {
                    // Do async work
                    await Task.Delay(int.MaxValue, stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error by RSS Starting");
            }
        }
        private async void HandleTimerCallback(object? state)
        {
            _logger.LogInformation($"Read SteamNews. Count: {_steamConfig.News.Count}");
            var steamInterface = _steamWebInterfaceFactory.CreateSteamWebInterface<SteamNews>(new HttpClient());
            for (var i = 0; i < _steamConfig.News.Count; i++)
            {
               var news = await steamInterface.GetNewsForAppAsync(_steamConfig.News[i].AppId, count: 5);
            }
            _lastCheck = DateTime.Now;
        }

        /*private async Task SendMessageAsync(Models.Feed feedConfig, FeedItem feedItem)
        {
            await _steamWebInterfaceFactory.SendMessageAsync(feedConfig.DiscordServers, embed: RssEmbedHelper.CreateEmbed(feedConfig.Name, feedItem));
        }*/
    }
}
