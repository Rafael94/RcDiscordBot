using DSharpPlus;
using DSharpPlus.EventArgs;
using Lavalink4NET;
using Lavalink4NET.Player;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Rc.DiscordBot
{
    public class AudioHostedService : IHostedService
    {
        private readonly DiscordClient _discordClient;
        private readonly IAudioService _audioService;
        private readonly ILogger<AudioHostedService> _logger;


        public AudioHostedService(DiscordClient discordClient, IAudioService audioService, ILogger<AudioHostedService> logger)
        {
            _discordClient = discordClient;
            _audioService = audioService;
            _logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {

            _discordClient.Ready += (DiscordClient client, ReadyEventArgs args) =>
            {
                return Task.Factory.StartNew(async () =>
                  {
                      try
                      {
                          await _audioService.InitializeAsync();
                      }
                      catch (Exception ex)
                      {

                          _logger.LogError("Error by AudioService InitializeAsync", ex);
                      }

                  });
            };

            _discordClient.VoiceStateUpdated += (DiscordClient client, VoiceStateUpdateEventArgs args) =>
            {
                return Task.Factory.StartNew(async () =>
                {
                    var player = _audioService.GetPlayer<QueuedLavalinkPlayer>(args.Guild.Id);

                    if (player == null)
                    {
                        return;
                    }

                    var memberCount = args.Channel.Users.Count();
                    // 1 => Nur der Bot befindet sich im Channel
                    if (memberCount == 1)
                    {
                        if (player.State == PlayerState.Playing)
                        {
                            await player.PauseAsync();
                        }
                    }
                    else if (memberCount == 2 && player.State == PlayerState.Paused)
                    {
                        if (player.CurrentTrack != null || player.Queue.Count > 0)
                        {
                            await player.ResumeAsync();
                        }
                    }

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
                    catch (Exception) { }
                }
            }

            await DisconnectPlayersAsync(_audioService.GetPlayers<LavalinkPlayer>());
            await DisconnectPlayersAsync(_audioService.GetPlayers<QueuedLavalinkPlayer>());
            await DisconnectPlayersAsync(_audioService.GetPlayers<VoteLavalinkPlayer>());
        }

    }
}
