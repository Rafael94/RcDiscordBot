using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Rc.DiscordBot.Models;
using Rc.DiscordBot.Services;
using Steam.Models;
using Steam.Models.SteamStore;
using SteamWebAPI2.Interfaces;
using SteamWebAPI2.Utilities;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Rc.DiscordBot
{
    public class SteamWorker : BackgroundService
    {
        public SteamWorker(IOptions<SteamConfig> steamConfig, SteamWebInterfaceFactory steamWebInterfaceFactory, ILogger<SteamWorker> logger, DiscordService discordService, SteamStore steamStore)
        {
            _steamConfig = steamConfig.Value;
            _steamWebInterfaceFactory = steamWebInterfaceFactory;
            _logger = logger;
            _discordService = discordService;
            _steamStore = steamStore;
        }

        private readonly SteamConfig _steamConfig;
        private readonly SteamWebInterfaceFactory _steamWebInterfaceFactory;
        private readonly ILogger<SteamWorker> _logger;
        private readonly DiscordService _discordService;
        private readonly SteamStore _steamStore;

        private DateTimeOffset _lastCheck = DateTimeOffset.Now;

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


            SteamNews? steamInterface = _steamWebInterfaceFactory.CreateSteamWebInterface<SteamNews>(new HttpClient());
            for (int i = 0; i < _steamConfig.News.Count; i++)
            {
                ISteamWebResponse<SteamNewsResultModel>? newsResponse = await steamInterface.GetNewsForAppAsync(_steamConfig.News[i].AppId, count: 5, feeds: _steamConfig.News[i].Feeds, tags: _steamConfig.News[i].Tags);
                _logger.LogDebug($"{newsResponse.Data.NewsItems.Count} Steam news entries were found for the game {newsResponse.Data.AppId}");

                // Will only be retrieved when new news is available
                StoreAppDetailsDataModel? gameDetails = null;

                foreach (NewsItemModel? news in newsResponse.Data.NewsItems)
                {
                    DateTimeOffset date = DateTimeOffset.FromUnixTimeSeconds((long)news.Date);

                    if (date < _lastCheck)
                    {
                        // News are sorted by date
                        break;
                    }

                    if (gameDetails == null)
                    {
                        gameDetails = await _steamStore.GetStoreAppDetailsAsync(newsResponse.Data.AppId);
                    }

                    await SendMessageAsync(_steamConfig.News[i], gameDetails, news, date);
                }
            }
            _lastCheck = DateTimeOffset.Now;
        }

        private async Task SendMessageAsync(News newsConfig, StoreAppDetailsDataModel app, NewsItemModel item, DateTimeOffset date)
        {
            try
            {
                await _discordService.SendMessageAsync(newsConfig.DiscordServers, embed: SteamEmbedHelper.CreateEmbed(app, item, date));
            }
            catch (Exception ex)
            {

                _logger.LogError(ex, "Error when sending the Steam News: " + System.Text.Json.JsonSerializer.Serialize(item));
            }

        }

    }
}
