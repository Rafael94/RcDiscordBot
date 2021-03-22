using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Options;
using Rc.DiscordBot.Handlers;
using Rc.DiscordBot.Models;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Rc.DiscordBot.Services
{
    public class DiscordService
    {
        private readonly BotConfig _botConfig;
        private readonly DiscordSocketClient _client;


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Nicht verwendete Parameter entfernen", Justification = "<Ausstehend>")]
        public DiscordService(IOptions<BotConfig> botConfigOptions, DiscordSocketClient client, CommandHandler commandHandler /* Wird benötigt, damit der Commandhandler initialisiert wird*/)
        {
            _botConfig = botConfigOptions.Value;
            _client = client;

            SubscribeDiscordEvents();
        }

        private async Task InitBotAsync()
        {
            await _client.LoginAsync(TokenType.Bot, _botConfig.BotToken);
            await _client.StartAsync();
        }

        public async Task StartAsync(CancellationToken stoppingToken)
        {
            await InitBotAsync();
            stoppingToken.WaitHandle.WaitOne();
        }

        private void SubscribeDiscordEvents()
        {
            _client.Ready += ReadyAsync;
            _client.Log += LogAsync;
        }

        private async Task ReadyAsync()
        {
            try
            {
                await _client.SetGameAsync(_botConfig.GameStatus);
            }
            catch (Exception ex)
            {
                // await LoggingService.LogInformationAsync(ex.Source, ex.Message);
            }

        }

        /*Used whenever we want to log something to the Console. 
            Todo: Hook in a Custom LoggingService. */
        private async Task LogAsync(LogMessage logMessage)
        {
            //await LoggingService.LogAsync(logMessage.Source, logMessage.Severity, logMessage.Message);
        }
    }
}
