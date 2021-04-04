﻿using CodeHollow.FeedReader;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Rc.DiscordBot.Models;
using Rc.DiscordBot.Services;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Rc.DiscordBot
{
    public class RssWorker : BackgroundService
    {
        public RssWorker(IOptions<RssConfig> rssConfig, DiscordService discordService, ILogger<RssWorker> logger)
        {
            _rssConfig = rssConfig.Value;
            _discordService = discordService;
            _logger = logger;
        }

        private readonly RssConfig _rssConfig;
        private readonly DiscordService _discordService;
        private readonly ILogger<RssWorker> _logger;

        private DateTime _lastCheck = DateTime.Now;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {

            try
            {
                stoppingToken.ThrowIfCancellationRequested();
                _logger.LogInformation($"Init RSS Timer with Intervall {_rssConfig.Interval}");
                using Timer? timer = new(HandleTimerCallback, null, _rssConfig.Interval, _rssConfig.Interval);

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
            _logger.LogInformation($"Read RSS Feeds. Count: {_rssConfig.Feeds.Count}");
            for (int i = 0; i < _rssConfig.Feeds.Count; i++)
            {
                CodeHollow.FeedReader.Feed? feedList = await FeedReader.ReadAsync(_rssConfig.Feeds[i].Url);
                _logger.LogDebug($"Feed {_rssConfig.Feeds[i].Name} has {feedList.Items.Count} Entries. Last Update {feedList.LastUpdatedDate}");

                if (feedList.LastUpdatedDate < _lastCheck || feedList.Items?.Count == 0)
                {
                    continue;
                }

                // Falls die Feed Items kein Datum enthalten, wird nur der erste Eintrag gesendet
                if (feedList.Items!.First().PublishingDate == null)
                {
                    await SendMessageAsync(_rssConfig.Feeds[i], feedList.Items!.First());
                }
                else
                {
                    foreach (FeedItem? feed in feedList.Items!)
                    {
                        // Liste ist nach Datum sortiert
                        if (feed.PublishingDate < _lastCheck)
                        {
                            break;
                        }

                        await SendMessageAsync(_rssConfig.Feeds[i], feed);
                    }
                }
            }

            _lastCheck = DateTime.Now;
        }

        private async Task SendMessageAsync(Models.Feed feedConfig, FeedItem feedItem)
        {
            await _discordService.SendMessageAsync(feedConfig.DiscordServers, embed: RssEmbedHelper.CreateEmbed(feedConfig.Name, feedItem));
        }
    }
}
