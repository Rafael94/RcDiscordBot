using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Rc.DiscordBot.Handlers;
using Rc.DiscordBot.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Rc.DiscordBot.Services
{
    public sealed class DiscordService : IDisposable
    {
        // To detect redundant calls
        private bool _disposed = false;

        private readonly BotConfig _botConfig;
        private readonly ILogger<DiscordService> _logger;
        public DiscordClient Client { get; init; }


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Nicht verwendete Parameter entfernen", Justification = "<Ausstehend>")]
        public DiscordService(IOptions<BotConfig> botConfigOptions, DiscordClient client, ILogger<DiscordService> logger)
        {
            _botConfig = botConfigOptions.Value;
            Client = client;
            _logger = logger;

            SubscribeDiscordEvents();

        }

        public Task StartAsync()
        {
            _logger.LogInformation("Start Discord Bot");
            return Client.ConnectAsync();
        }

        public Task StopAsync()
        {
            _logger.LogInformation("Stop Discord Bot");
            return Client.DisconnectAsync();
        }

        private void SubscribeDiscordEvents()
        {
            Client.Ready += ReadyAsync;
        }

        private async Task ReadyAsync(DiscordClient client, ReadyEventArgs args)
        {

            _logger.LogInformation("Discord Bot Ready");
            await client.UpdateStatusAsync(new DSharpPlus.Entities.DiscordActivity(_botConfig.GameStatus), DSharpPlus.Entities.UserStatus.Online);
        }

        public async Task SendMessageAsync(IEnumerable<MessageSendToDiscordServer> discordServers, string? text = null, DiscordEmbed? embed = null)
        {
            var servers = Client.Guilds;

            // Server durchlaufen die Benachrichtigt werden sollen
            foreach (MessageSendToDiscordServer? discordServer in discordServers)
            {
                DiscordGuild? socketGuild = null;

                // Discord Server ID ermitteln
                foreach (var server in servers)
                {
                    if (string.Equals(server.Value.Name, discordServer.Name, StringComparison.OrdinalIgnoreCase))
                    {
                        socketGuild = server.Value;
                        break;
                    }
                }

                // Wenn der DiscordServer nicht gefunden worden ist, den nächsten Server abrufen
                if (socketGuild == null)
                {
                    continue;
                }

                foreach (var channel in socketGuild.Channels)
                {
                    if(channel.Value.Type == ChannelType.Text && string.Equals(channel.Value.Name, discordServer.Channel, StringComparison.OrdinalIgnoreCase))
                    {
                        await channel.Value.SendMessageAsync(content: text, embed: embed);
                    }
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                Client.Dispose();
            }

            _disposed = true;
        }
    }
}
