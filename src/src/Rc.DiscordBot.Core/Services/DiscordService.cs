using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Options;
using Rc.DiscordBot.Handlers;
using Rc.DiscordBot.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Rc.DiscordBot.Services
{
    public class DiscordService
    {
        private readonly BotConfig _botConfig;
        public DiscordSocketClient Client { get; }


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Nicht verwendete Parameter entfernen", Justification = "<Ausstehend>")]
        public DiscordService(IOptions<BotConfig> botConfigOptions, DiscordSocketClient client, CommandHandler commandHandler /* Wird benötigt, damit der Commandhandler initialisiert wird*/)
        {
            _botConfig = botConfigOptions.Value;
            Client = client;

            SubscribeDiscordEvents();
        }

        private async Task InitBotAsync()
        {
            await Client.LoginAsync(TokenType.Bot, _botConfig.BotToken);
            await Client.StartAsync();
        }

        public async Task StartAsync()
        {
            await InitBotAsync();
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
        private  Task LogAsync(LogMessage logMessage)
        {
            //await LoggingService.LogAsync(logMessage.Source, logMessage.Severity, logMessage.Message);
            return Task.CompletedTask;
        }

        public async Task SendMessageAsync(IEnumerable<MessageSendToDiscordServer> discordServers, string? text = null, Embed? embed = null)
        {
            var servers = Client.Guilds;

            // Server durchlaufen die Benachrichtigt werden sollen
            foreach (var discordServer in discordServers)
            {
                SocketGuild? socketGuild = null;

                // Discord Server ID ermitteln
                foreach (var server in servers)
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

                foreach (var channel in socketGuild.Channels)
                {
                    // Nur Text Channels beachten
                    if ((channel is SocketTextChannel txtChannel))
                    {
                        // Prüfen ob es im Channel gepostet werden soll
                        if (string.Equals(channel.Name, discordServer.Channel, StringComparison.OrdinalIgnoreCase))
                        {
                            await txtChannel.SendMessageAsync(embed: embed);
                            break;
                        }
                    }
                }

            }
        }
    }
}
