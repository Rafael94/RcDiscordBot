using Discord;
using Discord.WebSocket;
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
        public DiscordSocketClient Client { get; init; }


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Nicht verwendete Parameter entfernen", Justification = "<Ausstehend>")]
        public DiscordService(IOptions<BotConfig> botConfigOptions, DiscordSocketClient client, CommandHandler commandHandler /* Wird benötigt, damit der Commandhandler initialisiert wird*/, ILogger<DiscordService> logger)
        {
            _botConfig = botConfigOptions.Value;
            Client = client;
            _logger = logger;

            SubscribeDiscordEvents();

        }

        public async Task StartAsync()
        {
            _logger.LogInformation("Start Discord Bot");
            await Client.LoginAsync(TokenType.Bot, _botConfig.BotToken);
            await Client.StartAsync();
        }

        public async Task StopAsync()
        {
            _logger.LogInformation("Stop Discord Bot");
            await Client.StopAsync();
            await Client.LogoutAsync();
        }

        private void SubscribeDiscordEvents()
        {
            Client.Ready += ReadyAsync;
            Client.Log += LogAsync;
        }

        private async Task ReadyAsync()
        {

            _logger.LogInformation("Discord Bot Ready");
            await Client.SetGameAsync(_botConfig.GameStatus);
        }

        /*Used whenever we want to log something to the Console. 
            Todo: Hook in a Custom LoggingService. */
        private Task LogAsync(LogMessage logMessage)
        {
            switch (logMessage.Severity)
            {
                case LogSeverity.Critical:
                    _logger.LogCritical(logMessage.Exception, $"Source: {logMessage.Source} Message: {logMessage.Message}");
                    break;
                case LogSeverity.Error:
                    _logger.LogError(logMessage.Exception, $"Source: {logMessage.Source} Message: {logMessage.Message}");
                    break;
                case LogSeverity.Warning:
                    _logger.LogWarning(logMessage.Exception, $"Source: {logMessage.Source} Message: {logMessage.Message}");
                    break;
                case LogSeverity.Info:
                    _logger.LogInformation(logMessage.Exception, $"Source: {logMessage.Source} Message: {logMessage.Message}");
                    break;
                case LogSeverity.Verbose:
                    _logger.LogTrace(logMessage.Exception, $"Source: {logMessage.Source} Message: {logMessage.Message}");
                    break;
                case LogSeverity.Debug:
                    _logger.LogDebug(logMessage.Exception, $"Source: {logMessage.Source} Message: {logMessage.Message}");
                    break;
            }

            return Task.CompletedTask;
        }

        public async Task SendMessageAsync(IEnumerable<MessageSendToDiscordServer> discordServers, string? text = null, Embed? embed = null)
        {
            IReadOnlyCollection<SocketGuild>? servers = Client.Guilds;

            // Server durchlaufen die Benachrichtigt werden sollen
            foreach (MessageSendToDiscordServer? discordServer in discordServers)
            {
                SocketGuild? socketGuild = null;

                // Discord Server ID ermitteln
                foreach (SocketGuild? server in servers)
                {
                    if (string.Equals(server.Name, discordServer.Name, StringComparison.OrdinalIgnoreCase))
                    {
                        socketGuild = server;
                        break;
                    }
                }

                // Wenn der DiscordServer nicht gefunden worden ist, den nächsten Server abrufen
                if (socketGuild == null)
                {
                    continue;
                }

                foreach (SocketGuildChannel? channel in socketGuild.Channels)
                {
                    // Nur Text Channels beachten
                    if ((channel is SocketTextChannel txtChannel))
                    {
                        // Prüfen ob es im Channel gepostet werden soll
                        if (string.Equals(channel.Name, discordServer.Channel, StringComparison.OrdinalIgnoreCase))
                        {
                            await txtChannel.SendMessageAsync(text: text, embed: embed);
                            break;
                        }
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
