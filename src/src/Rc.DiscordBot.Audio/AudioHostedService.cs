using DSharpPlus;
using DSharpPlus.EventArgs;
using DSharpPlus.Lavalink;
using DSharpPlus.Net;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Rc.DiscordBot.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Rc.DiscordBot
{
    public class AudioHostedService : IHostedService
    {
        private readonly DiscordClient _discordClient;
        private readonly LavaConfig _lavaConfig;
        private LavalinkExtension? _lavalink;

        public AudioHostedService(DiscordClient discordClient, IOptions<LavaConfig> lavaConfig)
        {
            _discordClient = discordClient;
            _lavaConfig = lavaConfig.Value;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
           
            _discordClient.Ready += async (DiscordClient client, ReadyEventArgs args) =>
            {
                _ = Task.Factory.StartNew(async () =>
                  {
                      var endpoint = new ConnectionEndpoint
                      {
                          Hostname = _lavaConfig.Endpoint,
                          Port = _lavaConfig.Port, // From your server configuration
                        Secured = _lavaConfig.Secured
                      };

                      var lavalinkConfig = new LavalinkConfiguration
                      {
                          Password = _lavaConfig.Password,
                          RestEndpoint = endpoint,
                          SocketEndpoint = endpoint,
                          ResumeKey = _lavaConfig.ResumeKey,
                          ResumeTimeout = _lavaConfig.ResumeTimeout,
                      };


                      _lavalink = _discordClient.UseLavalink();

                      await _lavalink.ConnectAsync(lavalinkConfig);
                  });
            };                     
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {

            return Task.CompletedTask;
        }
    }
}
