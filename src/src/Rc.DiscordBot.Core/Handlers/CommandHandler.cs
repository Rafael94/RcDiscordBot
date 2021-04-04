using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Rc.DiscordBot.Models;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Rc.DiscordBot.Handlers
{
    public class CommandHandler
    {
        private readonly DiscordSocketClient _client;
        private readonly CommandService _commands;
        private readonly IServiceProvider _services;
        private readonly BotConfig _botConfig;
        private readonly ILogger<CommandHandler> _logger;

        public CommandHandler(DiscordSocketClient client, CommandService commands, IServiceProvider services, IOptions<BotConfig> botConfig, ILogger<CommandHandler> logger)
        {
            _client = client;
            _commands = commands;
            _services = services;
            _botConfig = botConfig.Value;
            _logger = logger;

            HookEvents();
        }

        /* Initialize the CommandService. */
        public async Task AddModulesAsync(Assembly assembly)
        {
            await _commands.AddModulesAsync(
                assembly: assembly,
                services: _services);

        }

        /* Hook Command Specific Events. */
        public void HookEvents()
        {
            _commands.CommandExecuted += CommandExecutedAsync;
            _commands.Log += LogAsync;
            _client.MessageReceived += HandleCommandAsync;
        }

        /* When a MessageRecived Event triggers from the Client.
              Handle the message here. */
        private Task HandleCommandAsync(SocketMessage socketMessage)
        {
            int argPos = 0;
            //Check that the message is a valid command, ignore everything we don't care about. (Private message, messages from other Bots, Etc)
            if (socketMessage is not SocketUserMessage message || message.Author.IsBot || message.Author.IsWebhook || message.Channel is IPrivateChannel)
            {
                return Task.CompletedTask;
            }

            /* Check that the message has our Prefix */
            if (!message.HasStringPrefix(_botConfig.Prefix, ref argPos))
            {
                return Task.CompletedTask;
            }

            /* Create the CommandContext for use in modules. */
            SocketCommandContext? context = new SocketCommandContext(_client, socketMessage as SocketUserMessage);

            /* Check if the channel ID that the message was sent from is in our Config - Blacklisted Channels. */
            System.Collections.Generic.IEnumerable<ulong>? blacklistedChannelCheck = from a in _botConfig.BlacklistedChannels
                                                                                     where a == context.Channel.Id
                                                                                     select a;
            ulong blacklistedChannel = blacklistedChannelCheck.FirstOrDefault();

            /* If the Channel ID is in the list of blacklisted channels. Ignore the command. */
            if (blacklistedChannel == context.Channel.Id)
            {
                return Task.CompletedTask;
            }
            else
            {
                Task<IResult>? result = _commands.ExecuteAsync(context, argPos, _services, MultiMatchHandling.Best);

                /* Report any errors if the command didn't execute succesfully. */
                if (!result.Result.IsSuccess)
                {
                    context.Channel.SendMessageAsync(result.Result.ErrorReason);
                }

                /* If everything worked fine, command will run. */
                return result;
            }
        }

        public static async Task CommandExecutedAsync(Optional<CommandInfo> command, ICommandContext context, IResult result)
        {
            /* command is unspecified when there was a search failure (command not found); we don't care about these errors */
            if (!command.IsSpecified)
            {
                return;
            }

            /* the command was succesful, we don't care about this result, unless we want to log that a command succeeded. */
            if (result.IsSuccess)
            {
                return;
            }

            /* the command failed, let's notify the user that something happened. */
            await context.Channel.SendMessageAsync($"error: {result}");
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
    }
}
