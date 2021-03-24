using CodeHollow.FeedReader;
using Discord.WebSocket;
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
    public class RssWorker : BackgroundService
    {
        public RssWorker(RssConfig rssConfig, DiscordSocketClient discordClient)
        {
            _rssConfig = rssConfig;
            _discordClient = discordClient;
        }

        private readonly RssConfig _rssConfig;
        private readonly DiscordSocketClient _discordClient;
        private readonly DateTime _startTime = DateTime.Now;

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var timer = new Timer(HandleTimerCallback, null, TimeSpan.Zero, _rssConfig.Interval);
            stoppingToken.WaitHandle.WaitOne();
            timer.Dispose();
            return Task.CompletedTask;
        }
        private async void HandleTimerCallback(object? state)
        {
            for(var i = 0; i <_rssConfig.Feeds.Count;i++)
            {
                var feedList = await FeedReader.ReadAsync(_rssConfig.Feeds[i].Url);
               
                foreach(var feed in feedList.Items)
                {
                    if(feed.PublishingDate < _startTime)
                    {
                        break;
                    }

                    
                }
            }         
        }
    }
}
