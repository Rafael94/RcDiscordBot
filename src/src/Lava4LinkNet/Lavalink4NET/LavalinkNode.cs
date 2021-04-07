/*
 *  File:   LavalinkNode.cs
 *  Author: Angelo Breuer
 *
 *  The MIT License (MIT)
 *
 *  Copyright (c) Angelo Breuer 2020
 *
 *  Permission is hereby granted, free of charge, to any person obtaining a copy
 *  of this software and associated documentation files (the "Software"), to deal
 *  in the Software without restriction, including without limitation the rights
 *  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 *  copies of the Software, and to permit persons to whom the Software is
 *  furnished to do so, subject to the following conditions:
 *
 *  The above copyright notice and this permission notice shall be included in
 *  all copies or substantial portions of the Software.
 *
 *  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 *  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 *  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 *  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 *  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 *  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 *  THE SOFTWARE.
 */

namespace Lavalink4NET
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Events;
    using Lavalink4NET.Logging;
    using Lavalink4NET.Payloads.Events;
    using Lavalink4NET.Payloads.Node;
    using Lavalink4NET.Payloads.Player;
    using Lavalink4NET.Player;
    using Lavalink4NET.Statistics;
    using Payloads;

    /// <summary>
    ///     Used for connecting to a single lavalink node.
    /// </summary>
    public class LavalinkNode : LavalinkSocket, IAudioService, IDisposable
    {
        private readonly bool _disconnectOnStop;
        private readonly IDiscordClientWrapper _discordClient;
        private bool _disposed;

        /// <summary>
        ///     Initializes a new instance of the <see cref="LavalinkNode"/> class.
        /// </summary>
        /// <param name="options">the node options for connecting</param>
        /// <param name="client">the discord client</param>
        /// <param name="logger">the logger</param>
        /// <param name="cache">an optional cache that caches track requests</param>
        /// <exception cref="ArgumentNullException">
        ///     thrown if the specified <paramref name="options"/> parameter is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        ///     thrown if the specified <paramref name="client"/> is <see langword="null"/>.
        /// </exception>
        public LavalinkNode(LavalinkNodeOptions options, IDiscordClientWrapper client, ILogger? logger = null, ILavalinkCache? cache = null)
            : base(options, client, logger, cache)
        {
            if (options is null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            Label = options.Label;
            _discordClient = client ?? throw new ArgumentNullException(nameof(client));
            Players = new ConcurrentDictionary<ulong, LavalinkPlayer>();

            _disconnectOnStop = options.DisconnectOnStop;
            _discordClient.VoiceServerUpdated += VoiceServerUpdated;
            _discordClient.VoiceStateUpdated += VoiceStateUpdated;
        }

        /// <summary>
        ///     Asynchronous event which is dispatched when a player connected to a voice channel.
        /// </summary>
        public event AsyncEventHandler<PlayerConnectedEventArgs>? PlayerConnected;

        /// <summary>
        ///     Asynchronous event which is dispatched when a player disconnected from a voice channel.
        /// </summary>
        public event AsyncEventHandler<PlayerDisconnectedEventArgs>? PlayerDisconnected;

        /// <summary>
        ///     An asynchronous event which is triggered when a new statistics update was received
        ///     from the lavalink node.
        /// </summary>
        public event AsyncEventHandler<NodeStatisticsUpdateEventArgs>? StatisticsUpdated;

        /// <summary>
        ///     An asynchronous event which is triggered when a track ended.
        /// </summary>
        public event AsyncEventHandler<TrackEndEventArgs>? TrackEnd;

        /// <summary>
        ///     An asynchronous event which is triggered when an exception occurred while playing a track.
        /// </summary>
        public event AsyncEventHandler<TrackExceptionEventArgs>? TrackException;

        /// <summary>
        ///     Asynchronous event which is dispatched when a track started.
        /// </summary>
        public event AsyncEventHandler<TrackStartedEventArgs>? TrackStarted;

        /// <summary>
        ///     An asynchronous event which is triggered when a track got stuck.
        /// </summary>
        public event AsyncEventHandler<TrackStuckEventArgs>? TrackStuck;

        /// <summary>
        ///     Asynchronous event which is dispatched when a voice WebSocket connection was closed.
        /// </summary>
        public event AsyncEventHandler<WebSocketClosedEventArgs> WebSocketClosed;

        /// <summary>
        ///     Gets the current lavalink server version that is supported by the library.
        /// </summary>
        public static Version SupportedVersion { get; } = new Version(3, 0, 0);

        /// <summary>
        ///     Gets or sets the node label.
        /// </summary>
        public string? Label { get; set; }

        /// <summary>
        ///     Gets the last received node statistics; or <see langword="null"/> if no statistics
        ///     are available for the node.
        /// </summary>
        public NodeStatistics? Statistics { get; private set; }

        /// <summary>
        ///     Gets the player dictionary.
        /// </summary>
        protected IDictionary<ulong, LavalinkPlayer> Players { get; }

        /// <summary>
        ///     Disposes the node.
        /// </summary>
        public override void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;

            // call base handling
            base.Dispose();

            // unregister event listeners
            _discordClient.VoiceServerUpdated -= VoiceServerUpdated;
            _discordClient.VoiceStateUpdated -= VoiceStateUpdated;

            // dispose all players
            foreach (var player in Players)
            {
                player.Value.Dispose();
            }

            Players.Clear();
        }

        /// <summary>
        ///     Gets the audio player for the specified <paramref name="guildId"/>.
        /// </summary>
        /// <typeparam name="TPlayer">the type of the player to use</typeparam>
        /// <param name="guildId">the guild identifier to get the player for</param>
        /// <returns>the player for the guild</returns>
        /// <exception cref="InvalidOperationException">
        ///     thrown when a player was already created for the guild specified by <paramref
        ///     name="guildId"/>, but the requested player type ( <typeparamref name="TPlayer"/>)
        ///     differs from the created one.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        ///     thrown if the node socket has not been initialized. (Call <see
        ///     cref="LavalinkSocket.InitializeAsync"/> before sending payloads)
        /// </exception>
        /// <exception cref="ObjectDisposedException">thrown if the instance is disposed</exception>
        public TPlayer? GetPlayer<TPlayer>(ulong guildId)
            where TPlayer : LavalinkPlayer
        {
            EnsureNotDisposed();
            EnsureInitialized();

            if (!Players.TryGetValue(guildId, out var player))
            {
                return null;
            }

            if (player.State == PlayerState.Destroyed)
            {
                Players.Remove(player.GuildId);
                return null;
            }

            if (!(player is TPlayer player1))
            {
                throw new InvalidOperationException("A player was already created for the specified guild, but " +
                    "the requested player type differs from the created one.");
            }

            return player1;
        }

        /// <summary>
        ///     Gets the audio player for the specified <paramref name="guildId"/>.
        /// </summary>
        /// <param name="guildId">the guild identifier to get the player for</param>
        /// <returns>the player for the guild</returns>
        public LavalinkPlayer? GetPlayer(ulong guildId) => GetPlayer<LavalinkPlayer>(guildId);

        /// <summary>
        ///     Gets all players of the specified <typeparamref name="TPlayer"/>.
        /// </summary>
        /// <typeparam name="TPlayer">
        ///     the type of the players to get; use <see cref="LavalinkPlayer"/> to get all players
        /// </typeparam>
        /// <returns>the player list</returns>
        /// <exception cref="ObjectDisposedException">thrown if the instance is disposed</exception>
        public IReadOnlyList<TPlayer> GetPlayers<TPlayer>() where TPlayer : LavalinkPlayer
        {
            EnsureNotDisposed();
            return Players.Select(s => s.Value).OfType<TPlayer>().ToList().AsReadOnly();
        }

        /// <summary>
        ///     Gets a value indicating whether a player is created for the specified <paramref name="guildId"/>.
        /// </summary>
        /// <param name="guildId">
        ///     the snowflake identifier of the guild to create the player for
        /// </param>
        /// <returns>
        ///     a value indicating whether a player is created for the specified <paramref name="guildId"/>
        /// </returns>
        /// <exception cref="ObjectDisposedException">thrown if the instance is disposed</exception>
        public bool HasPlayer(ulong guildId)
        {
            EnsureNotDisposed();
            return Players.ContainsKey(guildId);
        }

        /// <inheritdoc/>
        /// <exception cref="ObjectDisposedException">thrown if the instance is disposed</exception>
        public Task<TPlayer> JoinAsync<TPlayer>(ulong guildId, ulong voiceChannelId, bool selfDeaf = false, bool selfMute = false) where TPlayer : LavalinkPlayer, new()
            => JoinAsync(CreateDefaultFactory<TPlayer>, guildId, voiceChannelId, selfDeaf, selfMute);

        /// <inheritdoc/>
        /// <exception cref="ObjectDisposedException">thrown if the instance is disposed</exception>
        public Task<LavalinkPlayer> JoinAsync(ulong guildId, ulong voiceChannelId, bool selfDeaf = false, bool selfMute = false)
            => JoinAsync(CreateDefaultFactory<LavalinkPlayer>, guildId, voiceChannelId, selfDeaf, selfMute);

        /// <inheritdoc/>
        /// <exception cref="ObjectDisposedException">thrown if the instance is disposed</exception>
        public async Task<TPlayer> JoinAsync<TPlayer>(
            PlayerFactory<TPlayer> playerFactory, ulong guildId, ulong voiceChannelId, bool selfDeaf = false,
            bool selfMute = false) where TPlayer : LavalinkPlayer
        {
            EnsureNotDisposed();

            var player = GetPlayer<TPlayer>(guildId);

            if (player is null)
            {
                Players[guildId] = player = LavalinkPlayer.CreatePlayer(
                    playerFactory, this, _discordClient, guildId, _disconnectOnStop);
            }

            if (!player.VoiceChannelId.HasValue || player.VoiceChannelId != voiceChannelId)
            {
                await player.ConnectAsync(voiceChannelId, selfDeaf, selfMute);
            }

            return player;
        }

        /// <summary>
        ///     Mass moves all players of the current node to the specified <paramref name="node"/> asynchronously.
        /// </summary>
        /// <param name="node">the node to move the players to</param>
        /// <returns>a task that represents the asynchronous operation</returns>
        /// <exception cref="ArgumentNullException">
        ///     thrown if the specified <paramref name="node"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     thrown if the specified <paramref name="node"/> is the same as the player node.
        /// </exception>
        /// <exception cref="ObjectDisposedException">thrown if the instance is disposed</exception>
        public async Task MoveAllPlayersAsync(LavalinkNode node)
        {
            EnsureNotDisposed();

            if (node is null)
            {
                throw new ArgumentNullException(nameof(node), "The specified target node is null.");
            }

            if (node == this)
            {
                throw new ArgumentException("Can not move the player to the same node.", nameof(node));
            }

            var players = Players.ToArray();
            Players.Clear();

            // await until all players were moved to the new node
            await Task.WhenAll(players.Select(player => MovePlayerInternalAsync(player.Value, node)));

            // log
            Logger?.Log(this, string.Format("Moved {0} player(s) to a new node.", players.Length), LogLevel.Debug);
        }

        /// <summary>
        ///     Moves the specified <paramref name="player"/> to the specified <paramref
        ///     name="node"/> asynchronously (while keeping its data and the same instance of the player).
        /// </summary>
        /// <param name="player">the player to move</param>
        /// <param name="node">the node to move the player to</param>
        /// <returns>a task that represents the asynchronous operation.</returns>
        /// <exception cref="ArgumentNullException">
        ///     thrown if the specified <paramref name="player"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        ///     thrown if the specified <paramref name="node"/> is <see langword="null"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     thrown if the specified <paramref name="node"/> is the same as the player node.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     thrown if the specified <paramref name="player"/> is already served by the specified
        ///     <paramref name="node"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     thrown if the specified <paramref name="node"/> does not serve the specified
        ///     <paramref name="player"/>.
        /// </exception>
        /// <exception cref="ObjectDisposedException">thrown if the instance is disposed</exception>
        public async Task MovePlayerAsync(LavalinkPlayer player, LavalinkNode node)
        {
            EnsureNotDisposed();

            if (player is null)
            {
                throw new ArgumentNullException(nameof(player), "The player to move is null.");
            }

            if (node is null)
            {
                throw new ArgumentNullException(nameof(node), "The specified target node is null.");
            }

            if (node == this)
            {
                throw new ArgumentException("Can not move the player to the same node.", nameof(node));
            }

            if (player.LavalinkSocket == node)
            {
                throw new ArgumentException("The specified player is already served by the targeted node.");
            }

            if (!Players.Contains(new KeyValuePair<ulong, LavalinkPlayer>(player.GuildId, player)))
            {
                throw new ArgumentException("The specified player is not a player from the current node.", nameof(player));
            }

            // remove the player from the current node
            Players.Remove(player.GuildId);

            // move player
            await MovePlayerInternalAsync(player, node);

            // log
            Logger?.Log(this, string.Format("Moved player for guild {0} to new node.", player.GuildId), LogLevel.Debug);
        }

        /// <summary>
        ///     Notifies a player disconnect asynchronously.
        /// </summary>
        /// <param name="eventArgs">the event arguments passed with the event</param>
        /// <returns>a task that represents the asynchronously operation.</returns>
        protected internal override Task NotifyDisconnectAsync(PlayerDisconnectedEventArgs eventArgs)
            => OnPlayerDisconnectedAsync(eventArgs);

        /// <summary>
        ///     Handles an event payload asynchronously.
        /// </summary>
        /// <param name="payload">the payload</param>
        /// <returns>a task that represents the asynchronous operation</returns>
        /// <exception cref="ObjectDisposedException">thrown if the instance is disposed</exception>
        protected virtual async Task OnEventReceived(EventPayload payload)
        {
            EnsureNotDisposed();

            if (!Players.TryGetValue(payload.GuildId, out var player))
            {
                return;
            }

            // a track ended
            if (payload is TrackEndEvent trackEndEvent)
            {
                var args = new TrackEndEventArgs(player,
                    trackEndEvent.TrackIdentifier,
                    trackEndEvent.Reason);

                await Task.WhenAll(OnTrackEndAsync(args),
                    player.OnTrackEndAsync(args));
            }

            // an exception occurred while playing a track
            if (payload is TrackExceptionEvent trackExceptionEvent)
            {
                var args = new TrackExceptionEventArgs(player,
                    trackExceptionEvent.TrackIdentifier,
                    trackExceptionEvent.Error);

                await Task.WhenAll(OnTrackExceptionAsync(args),
                    player.OnTrackExceptionAsync(args));
            }

            // a track got stuck
            if (payload is TrackStuckEvent trackStuckEvent)
            {
                var args = new TrackStuckEventArgs(player,
                    trackStuckEvent.TrackIdentifier,
                    trackStuckEvent.Threshold);

                await Task.WhenAll(OnTrackStuckAsync(args),
                    player.OnTrackStuckAsync(args));
            }

            // a track started
            if (payload is TrackStartEvent trackStartEvent)
            {
                var args = new TrackStartedEventArgs(player,
                    trackStartEvent.TrackIdentifier);

                await Task.WhenAll(OnTrackStartedAsync(args),
                    player.OnTrackStartedAsync(args));
            }

            // the voice web socket was closed
            if (payload is WebSocketClosedEvent webSocketClosedEvent)
            {
                Logger?.Log(this, string.Format("Voice WebSocket was closed for player: {0}" +
                    "\nClose Code: {1} ({2}, Reason: {3}, By Remote: {4}",
                    payload.GuildId, webSocketClosedEvent.CloseCode,
                    (int)webSocketClosedEvent.CloseCode, webSocketClosedEvent.Reason,
                    webSocketClosedEvent.ByRemote ? "Yes" : "No"),
                    webSocketClosedEvent.ByRemote ? LogLevel.Warning : LogLevel.Debug);

                var eventArgs = new WebSocketClosedEventArgs(
                    closeCode: webSocketClosedEvent.CloseCode,
                    reason: webSocketClosedEvent.Reason,
                    byRemote: webSocketClosedEvent.ByRemote);

                await OnWebSocketClosedAsync(eventArgs);
            }
        }

        /// <summary>
        ///     Processes the payload and invokes the <see cref="LavalinkSocket.PayloadReceived"/>
        ///     event asynchronously. (Can be override for event catching)
        /// </summary>
        /// <param name="eventArgs">the event arguments</param>
        /// <returns>a task that represents the asynchronous operation</returns>
        /// <exception cref="ObjectDisposedException">thrown if the instance is disposed</exception>
        protected override async Task OnPayloadReceived(PayloadReceivedEventArgs eventArgs)
        {
            EnsureNotDisposed();

            var payload = eventArgs.Payload;

            // received an event
            if (payload is EventPayload eventPayload)
            {
                await OnEventReceived(eventPayload);
            }

            // received a payload for a player
            if (payload is IPlayerPayload playerPayload)
            {
                await OnPlayerPayloadReceived(playerPayload);
            }

            // statistics update received
            if (payload is StatsUpdatePayload statsUpdate)
            {
                Statistics = new NodeStatistics(statsUpdate.Players, statsUpdate.PlayingPlayers,
                    statsUpdate.Uptime, statsUpdate.Memory, statsUpdate.Processor, statsUpdate.FrameStatistics);

                await OnStatisticsUpdateAsync(new NodeStatisticsUpdateEventArgs(Statistics));
            }

            await base.OnPayloadReceived(eventArgs);
        }

        /// <summary>
        ///     Dispatches the <see cref="PlayerConnected"/> event asynchronously.
        /// </summary>
        /// <param name="eventArgs">the event arguments passed with the event</param>
        /// <returns>a task that represents the asynchronous operation</returns>
        protected virtual Task OnPlayerConnectedAsync(PlayerConnectedEventArgs eventArgs)
            => PlayerConnected.InvokeAsync(this, eventArgs);

        /// <summary>
        ///     Dispatches the <see cref="PlayerDisconnected"/> event asynchronously.
        /// </summary>
        /// <param name="eventArgs">the event arguments passed with the event</param>
        /// <returns>a task that represents the asynchronous operation</returns>
        protected virtual Task OnPlayerDisconnectedAsync(PlayerDisconnectedEventArgs eventArgs)
            => PlayerDisconnected.InvokeAsync(this, eventArgs);

        /// <summary>
        ///     Handles a player payload asynchronously.
        /// </summary>
        /// <param name="payload">the payload</param>
        /// <returns>a task that represents the asynchronous operation</returns>
        /// <exception cref="ObjectDisposedException">thrown if the instance is disposed</exception>
        protected virtual Task OnPlayerPayloadReceived(IPlayerPayload payload)
        {
            EnsureNotDisposed();

            var player = GetPlayer<LavalinkPlayer>(ulong.Parse(payload.GuildId));

            // a player update was received
            if (player != null && payload is PlayerUpdatePayload playerUpdate)
            {
                player.UpdateTrackPosition(playerUpdate.Status.UpdateTime,
                    playerUpdate.Status.Position);
            }

            return Task.CompletedTask;
        }

        /// <summary>
        ///     Invokes the <see cref="StatisticsUpdated"/> event asynchronously. (Can be override
        ///     for event catching)
        /// </summary>
        /// <param name="eventArgs">the event arguments</param>
        /// <returns>a task that represents the asynchronous operation</returns>
        protected virtual Task OnStatisticsUpdateAsync(NodeStatisticsUpdateEventArgs eventArgs)
            => StatisticsUpdated.InvokeAsync(this, eventArgs);

        /// <summary>
        ///     Invokes the <see cref="TrackEnd"/> event asynchronously. (Can be override for event catching)
        /// </summary>
        /// <param name="eventArgs">the event arguments</param>
        /// <returns>a task that represents the asynchronous operation</returns>
        protected virtual Task OnTrackEndAsync(TrackEndEventArgs eventArgs)
            => TrackEnd.InvokeAsync(this, eventArgs);

        /// <summary>
        ///     Invokes the <see cref="TrackException"/> event asynchronously. (Can be override for
        ///     event catching)
        /// </summary>
        /// <param name="eventArgs">the event arguments</param>
        /// <returns>a task that represents the asynchronous operation</returns>
        protected virtual Task OnTrackExceptionAsync(TrackExceptionEventArgs eventArgs)
            => TrackException.InvokeAsync(this, eventArgs);

        /// <summary>
        ///     Dispatches the <see cref="TrackStarted"/> event asynchronously.
        /// </summary>
        /// <param name="eventArgs">the event arguments passed with the event</param>
        /// <returns>a task that represents the asynchronous operation</returns>
        protected virtual Task OnTrackStartedAsync(TrackStartedEventArgs eventArgs)
            => TrackStarted.InvokeAsync(this, eventArgs);

        /// <summary>
        ///     Invokes the <see cref="TrackStuck"/> event asynchronously. (Can be override for
        ///     event catching)
        /// </summary>
        /// <param name="eventArgs">the event arguments</param>
        /// <returns>a task that represents the asynchronous operation</returns>
        protected virtual Task OnTrackStuckAsync(TrackStuckEventArgs eventArgs)
            => TrackStuck.InvokeAsync(this, eventArgs);

        /// <summary>
        ///     Dispatches the <see cref="WebSocketClosed"/> event asynchronously.
        /// </summary>
        /// <param name="eventArgs">the event arguments passed with the event</param>
        /// <returns>a task that represents the asynchronous operation</returns>
        protected virtual Task OnWebSocketClosedAsync(WebSocketClosedEventArgs eventArgs)
            => WebSocketClosed.InvokeAsync(this, eventArgs);

        /// <summary>
        ///     The asynchronous method which is triggered when a voice server updated was received
        ///     from the discord gateway.
        /// </summary>
        /// <param name="sender">the event sender (unused here, but may be override)</param>
        /// <param name="voiceServer">the voice server update data</param>
        /// <returns>a task that represents the asynchronous operation</returns>
        /// <exception cref="ObjectDisposedException">thrown if the instance is disposed</exception>
        protected virtual Task VoiceServerUpdated(object sender, VoiceServer voiceServer)
        {
            EnsureNotDisposed();

            var player = GetPlayer<LavalinkPlayer>(voiceServer.GuildId);

            if (player is null)
            {
                return Task.CompletedTask;
            }

            return player.UpdateAsync(voiceServer);
        }

        /// <summary>
        ///     The asynchronous method which is triggered when a voice state updated was received
        ///     from the discord gateway.
        /// </summary>
        /// <param name="sender">the event sender (unused here)</param>
        /// <param name="args">the event arguments</param>
        /// <returns>a task that represents the asynchronous operation</returns>
        protected virtual async Task VoiceStateUpdated(object sender, VoiceStateUpdateEventArgs args)
        {
            EnsureNotDisposed();

            // ignore other users except the bot
            if (args.UserId != _discordClient.CurrentUserId)
            {
                return;
            }

            // try getting affected player
            if (!Players.TryGetValue(args.VoiceState.GuildId, out var player))
            {
                return;
            }

            var voiceState = args.VoiceState;
            var oldVoiceState = player.VoiceState;

            // connect to a voice channel
            if (oldVoiceState?.VoiceChannelId is null && voiceState.VoiceChannelId != null)
            {
                await player.UpdateAsync(voiceState);
                await OnPlayerConnectedAsync(new PlayerConnectedEventArgs(player, voiceState.VoiceChannelId.Value));
            }

            // disconnect from a voice channel
            else if (oldVoiceState?.VoiceChannelId != null && voiceState.VoiceChannelId is null)
            {
                // dispose the player
                await player.DisconnectAsync(PlayerDisconnectCause.Disconnected);
                player.Dispose();
                Players.Remove(voiceState.GuildId);
            }

            // reconnected to a voice channel
            else if (oldVoiceState?.VoiceChannelId != null && voiceState.VoiceChannelId != null)
            {
                await player.UpdateAsync(voiceState);
            }
        }

        private static T CreateDefaultFactory<T>() where T : LavalinkPlayer, new() => new T();

        /// <summary>
        ///     Throws an exception if the <see cref="LavalinkNode"/> instance is disposed.
        /// </summary>
        /// <exception cref="ObjectDisposedException">thrown if the instance is disposed</exception>
        private void EnsureNotDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(LavalinkNode));
            }
        }

        private async Task MovePlayerInternalAsync(LavalinkPlayer player, LavalinkNode node)
        {
            EnsureNotDisposed();

            // capture previous player state
            var previousTrack = player.CurrentTrack;
            var trackPosition = player.TrackPosition;
            var previousVolume = player.Volume;
            var previousBands = player.Bands;

            // destroy (NOT DISCONNECT) the player
            await player.DestroyAsync();

            // update the communication node
            player.LavalinkSocket = node;

            // resend voice update to the new node
            await player.UpdateAsync();

            // play track if one is playing
            if (previousTrack != null)
            {
                // restart track
                await player.PlayAsync(previousTrack, trackPosition);
            }

            // apply previous volume
            if (previousVolume != 1F)
            {
                await player.SetVolumeAsync(previousVolume, normalize: false, force: true);
            }

            // apply previous equalizer bands
            if (previousBands.Any())
            {
                await player.UpdateEqualizerAsync(previousBands, reset: false, force: true);
            }

            // add player to the new node
            node.Players[player.GuildId] = player;
        }
    }
}
