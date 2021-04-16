﻿/*
 *  File:   LavalinkPlayer.cs
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

namespace Lavalink4NET.Tracking
{
    using System;
    using Lavalink4NET.Player;

    /// <summary>
    ///     The event arguments for the
    ///     <see cref="InactivityTrackingService.PlayerTrackingStatusUpdated"/> event.
    /// </summary>
    public sealed class PlayerTrackingStatusUpdateEventArgs : EventArgs
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="PlayerTrackingStatusUpdateEventArgs"/> class.
        /// </summary>
        /// <param name="audioService">the audio service</param>
        /// <param name="player">the affected player (may be <see langword="null"/>)</param>
        /// <param name="trackingStatus">the new tracking status of the player</param>
        /// <exception cref="ArgumentNullException">
        ///     thrown if the specified <paramref name="audioService"/> is <see langword="null"/>.
        /// </exception>
        public PlayerTrackingStatusUpdateEventArgs(IAudioService audioService,
            LavalinkPlayer player, InactivityTrackingStatus trackingStatus)
        {
            AudioService = audioService ?? throw new ArgumentNullException(nameof(audioService));
            Player = player;
            TrackingStatus = trackingStatus;
        }

        /// <summary>
        ///     Gets the audio service.
        /// </summary>
        public IAudioService AudioService { get; }

        /// <summary>
        ///     Gets the affected player (may be <see langword="null"/>).
        /// </summary>
        public LavalinkPlayer Player { get; }

        /// <summary>
        ///     Gets the new tracking status of the player.
        /// </summary>
        public InactivityTrackingStatus TrackingStatus { get; }
    }
}