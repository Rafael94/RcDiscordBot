using Discord;
using Discord.WebSocket;
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
        public DiscordSocketClient Client { get; }


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Nicht verwendete Parameter entfernen", Justification = "<Ausstehend>")]
        public DiscordService(IOptions<BotConfig> botConfigOptions, DiscordSocketClient client, CommandHandler commandHandler /* Wird benötigt, damit der Commandhandler initialisiert wird*/)
        {
            _botConfig = botConfigOptions.Value;
            Client = client;

            SubscribeDiscordEvents();
        }

        public async Task StartAsync()
        {
            await Client.LoginAsync(TokenType.Bot, _botConfig.BotToken);
            await Client.StartAsync();
        }

        public async Task StopAsync()
        {
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
            try
            {
                await Client.SetGameAsync(_botConfig.GameStatus);
            }
            catch (Exception ex)
            {
                // await LoggingService.LogInformationAsync(ex.Source, ex.Message);
            }

        }

        /*Used whenever we want to log something to the Console. 
            Todo: Hook in a Custom LoggingService. */
        private Task LogAsync(LogMessage logMessage)
        {
            //await LoggingService.LogAsync(logMessage.Source, logMessage.Severity, logMessage.Message);
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

        
        public void Dispose() => Dispose(true);

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
