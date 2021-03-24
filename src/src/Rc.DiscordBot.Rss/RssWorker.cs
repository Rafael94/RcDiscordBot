using CodeHollow.FeedReader;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Rc.DiscordBot.Handlers;
using Rc.DiscordBot.Models;
using Rc.DiscordBot.Services;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Rc.DiscordBot
{
    public class RssWorker : BackgroundService
    {
        public RssWorker(IOptions<RssConfig> rssConfig, DiscordService discordService)
        {
            _rssConfig = rssConfig.Value;
            _discordService = discordService;
        }

        private readonly RssConfig _rssConfig;
        private readonly DiscordService _discordService;
        private readonly DateTime _startTime = DateTime.Now;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {

            try
            {
                stoppingToken.ThrowIfCancellationRequested();
                using var timer = new Timer(HandleTimerCallback, null, _rssConfig.Interval, _rssConfig.Interval);

                while (!stoppingToken.IsCancellationRequested)
                {
                    // Do async work
                    await Task.Delay(int.MaxValue, stoppingToken);
                }
            }
            catch (Exception) {}       
        }
        private async void HandleTimerCallback(object? state)
        {
            for (var i = 0; i < _rssConfig.Feeds.Count; i++)
            {
                var feedList = await FeedReader.ReadAsync(_rssConfig.Feeds[i].Url);

                foreach (var feed in feedList.Items)
                {
                    // Liste ist nach Datum sortiert
                    if (feed.PublishingDate < _startTime)
                    {
                        break;
                    }

                    await _discordService.SendMessageAsync(_rssConfig.Feeds[i].DiscordServers, embed: CreateEmbed(_rssConfig.Feeds[i].Name, feed));

                    break;
                }
            }
        }

        private  static Embed CreateEmbed(string feedName, FeedItem item)
        {
            return new EmbedBuilder()
                               .WithTitle($"RSS - {feedName}: {item.Title}")
                               .WithCustomDescription(item.Description)
                               .WithUrl(item.Link)
                               .WithAuthor(item.Author)
                               .WithColor(Color.Blue)
                               .WithBotFooter()
                               .Build();
        }
    }
}
