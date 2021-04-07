using DSharpPlus;
using DSharpPlus.EventArgs;
using Lavalink4NET;
using Lavalink4NET.Player;
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
        private readonly IAudioService _audioService;


        public AudioHostedService(DiscordClient discordClient, IAudioService audioService)
        {
            _discordClient = discordClient;
            _audioService = audioService;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {

            _discordClient.Ready += (DiscordClient client, ReadyEventArgs args) =>
            {
                return Task.Factory.StartNew(async () =>
                  {
                      await _audioService.InitializeAsync();
                  });
            };

            return Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            static async Task DisconnectPlayersAsync(IReadOnlyList<LavalinkPlayer> players)
            {
                foreach (var player in players)
                {
                    try
                    {
                        await player.DisconnectAsync();
                        player.Dispose();
                    }
                    catch (Exception){}                 
                }
            }

            await DisconnectPlayersAsync(_audioService.GetPlayers<LavalinkPlayer>());
            await DisconnectPlayersAsync(_audioService.GetPlayers<QueuedLavalinkPlayer>());
            await DisconnectPlayersAsync(_audioService.GetPlayers<VoteLavalinkPlayer>());
        }

    }
}
