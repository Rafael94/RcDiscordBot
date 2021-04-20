
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Lavalink4NET;
using Lavalink4NET.Player;
using Lavalink4NET.Rest;
using Microsoft.Extensions.Options;
using Rc.DiscordBot.Handlers;
using Rc.DiscordBot.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rc.DiscordBot.Modules
{

    public class AudioModule : BaseCommandModule
    {
        private readonly Lazy<IAudioService> _audioService;
        private readonly AudioConfig _audioConfig;

        public AudioModule(Lazy<IAudioService> audioService, IOptions<AudioConfig> audioConfig)
        {
            _audioService = audioService;
            _audioConfig = audioConfig.Value;
        }

        [Command("join")]
        public async Task JoinAsync(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();
            if (ctx.Member.VoiceState == null)
            {
                await new DiscordMessageBuilder()
                   .WithEmbed(EmbedHandler.CreateErrorEmbed("Music, Join", "You must be connected to a voice channel!"))
                   .WithReply(ctx.Message.Id, true)
                   .SendAsync(ctx.Channel);

                return;
            }

            var channel = ctx.Member.VoiceState.Channel;

            if (channel.Type != ChannelType.Voice)
            {
                await new DiscordMessageBuilder()
                    .WithEmbed(EmbedHandler.CreateErrorEmbed("Music, Join", "You must be connected to a voice channel!"))
                    .WithReply(ctx.Message.Id, true)
                    .SendAsync(ctx.Channel);
                return;
            }

            await _audioService.Value.JoinAsync<QueuedLavalinkPlayer>(ctx.Guild.Id, channel.Id, false, false);
            await new DiscordMessageBuilder()
                     .WithEmbed(EmbedHandler.CreateBasicEmbed("Music, Join", $"Joined {channel.Name}.", DiscordColor.Green))
                     .WithReply(ctx.Message.Id, true)
                     .SendAsync(ctx.Channel);
        }

        [Command("join")]
        [Description("Den Bot zum aktuellen Channel")]
        public async Task JoinAsync(CommandContext ctx, DiscordChannel channel)
        {
            await ctx.TriggerTypingAsync();
            if (channel.Type != ChannelType.Voice)
            {
                await new DiscordMessageBuilder()
                    .WithEmbed(EmbedHandler.CreateErrorEmbed("Music", "Not a valid voice channel."))
                    .WithReply(ctx.Message.Id, true)
                    .SendAsync(ctx.Channel);

                return;
            }
            await _audioService.Value.JoinAsync<QueuedLavalinkPlayer>(ctx.Guild.Id, channel.Id, false, false);

            await new DiscordMessageBuilder()
                    .WithEmbed(EmbedHandler.CreateBasicEmbed("Music, Join", $"Joined {channel.Name}.", DiscordColor.Green))
                    .WithReply(ctx.Message.Id, true)
                    .SendAsync(ctx.Channel);
        }

        [Command("leave")]
        public async Task LeaveAsync(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();
            var player = _audioService.Value.GetPlayer(ctx.Guild.Id);

            if (player == null)
            {
                await new DiscordMessageBuilder()
                .WithEmbed(EmbedHandler.CreateErrorEmbed("Music, List", $"Could not aquire player.\nAre you using the bot right now?"))
                .WithReply(ctx.Message.Id, true)
                .SendAsync(ctx.Channel);
                return;
            }

            await player.DisconnectAsync();
            await new DiscordMessageBuilder()
                 .WithEmbed(EmbedHandler.CreateBasicEmbed("Music", $"I've left. Thank you for playing moosik.", DiscordColor.Blue))
                 .WithReply(ctx.Message.Id, true)
                 .SendAsync(ctx.Channel);
        }

        [Command("stop")]
        public async Task StopAsync(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();
            var player = _audioService.Value.GetPlayer(ctx.Guild.Id);

            if (player == null)
            {
                await new DiscordMessageBuilder()
                .WithEmbed(EmbedHandler.CreateErrorEmbed("Music, List", $"Could not aquire player.\nAre you using the bot right now?"))
                .WithReply(ctx.Message.Id, true)
                .SendAsync(ctx.Channel);
                return;
            }

            await player.DisconnectAsync();
            await new DiscordMessageBuilder()
                 .WithEmbed(EmbedHandler.CreateBasicEmbed("Music", $"I Have stopped playback & the playlist has been cleared.", DiscordColor.Blue))
                 .WithReply(ctx.Message.Id, true)
                 .SendAsync(ctx.Channel);
        }

        [Command("play")]
        public async Task PlayAsync(CommandContext ctx, [RemainingText] string search)
        {
            await ctx.TriggerTypingAsync();
            var player = _audioService.Value.GetPlayer<QueuedLavalinkPlayer>(ctx.Guild.Id);

            if (player == null)
            {
                await new DiscordMessageBuilder()
                    .WithEmbed(EmbedHandler.CreateErrorEmbed("Music", $"There is nothing to pause."))
                    .WithReply(ctx.Message.Id, true)
                    .SendAsync(ctx.Channel);
                return;
            }

            List<LavalinkTrack>? tracks;

            if (search.StartsWith("http"))
            {
                tracks = (await _audioService.Value.GetTracksAsync(search, SearchMode.None)).ToList();
            }
            else
            {
                tracks = (await _audioService.Value.GetTracksAsync(search, SearchMode.YouTube)).ToList();

                if (tracks == null)
                {
                    tracks = (await _audioService.Value.GetTracksAsync(search, SearchMode.SoundCloud)).ToList();
                }
            }

            if (tracks == null || tracks.Count == 0)
            {
                await new DiscordMessageBuilder()
                    .WithEmbed(EmbedHandler.CreateErrorEmbed("Music", $"No Track with the Name {search} was found"))
                    .WithReply(ctx.Message.Id, true)
                    .SendAsync(ctx.Channel);

                return;
            }

            var track = tracks.First();
            List<DiscordField> fields = new()
            {
                new DiscordField("Channel", ctx.Guild.Channels[player.VoiceChannelId!.Value].Name, true),
                new DiscordField("Duration", track.Duration.ToString(@"hh\:mm\:ss"), true),
            };

            DiscordAuthor author = new(track.Author);
            var isPlayList = tracks.Count > 1 && search.StartsWith("http");
            if (isPlayList)
            {
                fields.Add(new DiscordField("Playlist", "Yes"));
                fields.Add(new DiscordField("Tracks", tracks.Count.ToString()));
            }

            var trackQueue = await player.PlayAsync(track);

            var mgsBUilder = new DiscordMessageBuilder()
                   .WithReply(ctx.Message.Id, true);

            if (trackQueue == 0)
            {
                mgsBUilder.WithEmbed(EmbedHandler.CreateBasicEmbed("Music", $"Now Playing: {track.Title}\nUrl: {track.Source}", DiscordColor.Blue, fields, author));
            }
            else
            {
                TimeSpan estimatedTime = player.CurrentTrack!.Duration - player.CurrentTrack.Position;

                foreach (var queuedTrack in player.Queue)
                {
                    estimatedTime += queuedTrack.Duration;
                }

                fields.Add(new DiscordField("Position in queue", player.Queue.Count.ToString(), true));

                if (isPlayList)
                {
                    mgsBUilder.WithEmbed(EmbedHandler.CreateBasicEmbed("Music", $"Tracks has been added to queue.", DiscordColor.Blue, fields, author));
                }
                else
                {
                    mgsBUilder.WithEmbed(EmbedHandler.CreateBasicEmbed("Music", $"{track.Title} has been added to queue.", DiscordColor.Blue, fields, author));
                }
            }

            for (var i = 1; i < tracks.Count; i++)
            {
                await player.PlayAsync(tracks[i]);
            }

            await mgsBUilder.SendAsync(ctx.Channel);

            // Wenn sich keiner außer der Bot im Channel befindet, den Player pausieren
            if (ctx.Guild.GetChannel(player.VoiceChannelId.Value).Users.Count() == 1)
            {
                await player.PauseAsync();
            }
        }

        [Command("pause")]
        public async Task PauseAsync(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();
            var player = _audioService.Value.GetPlayer<QueuedLavalinkPlayer>(ctx.Guild.Id);

            if (player == null)
            {
                await new DiscordMessageBuilder()
                     .WithEmbed(EmbedHandler.CreateErrorEmbed("Music", $"There is nothing to pause."))
                     .WithReply(ctx.Message.Id, true)
                     .SendAsync(ctx.Channel);
                return;
            }

            await player.PauseAsync();
            await new DiscordMessageBuilder()
                    .WithEmbed(EmbedHandler.CreateBasicEmbed("Music", $"**Paused:** {player.CurrentTrack!.Title}, what a bamboozle.", DiscordColor.Blue))
                    .WithReply(ctx.Message.Id, true)
                    .SendAsync(ctx.Channel);
        }

        [Command("resume")]
        public async Task ResumeAsync(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();
            var player = _audioService.Value.GetPlayer<QueuedLavalinkPlayer>(ctx.Guild.Id);

            if (player == null)
            {
                await new DiscordMessageBuilder()
                     .WithEmbed(EmbedHandler.CreateErrorEmbed("Music", $"There is nothing to resume."))
                     .WithReply(ctx.Message.Id, true)
                     .SendAsync(ctx.Channel);
                return;
            }

            await player.ResumeAsync();
            await new DiscordMessageBuilder()
                .WithEmbed(EmbedHandler.CreateBasicEmbed("Music", $"**Resumed:** {player.CurrentTrack!.Title}, what a bamboozle.", DiscordColor.Blue))
                .WithReply(ctx.Message.Id, true)
                .SendAsync(ctx.Channel);
        }

        [Command("volume")]
        public async Task VolumeAsync(CommandContext ctx, float volume)
        {
            await ctx.TriggerTypingAsync();
            var player = _audioService.Value.GetPlayer<QueuedLavalinkPlayer>(ctx.Guild.Id);

            if (player == null)
            {
                await new DiscordMessageBuilder()
                    .WithEmbed(EmbedHandler.CreateErrorEmbed("Music", "Bot is not connected to a Voice Channel"))
                    .WithReply(ctx.Message.Id, true)
                    .SendAsync(ctx.Channel);
                return;
            }

            if (volume < 0 || volume > 1)
            {
                await new DiscordMessageBuilder()
                   .WithEmbed(EmbedHandler.CreateErrorEmbed("Music", "Value must between 0.0 and 1.0"))
                   .WithReply(ctx.Message.Id, true)
                   .SendAsync(ctx.Channel);
                return;
            }

            await player.SetVolumeAsync(volume);
            await new DiscordMessageBuilder()
              .WithEmbed(EmbedHandler.CreateBasicEmbed("Music", $"Volume has been set to {volume}.", DiscordColor.Blue))
              .WithReply(ctx.Message.Id, true)
              .SendAsync(ctx.Channel);
        }

        [Command("ListStreams")]
        public async Task ListStreamsAsync(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();
            DiscordEmbedBuilder builder = new();

            builder
                .WithColor(DiscordColor.Blue)
                .WithCurrentTimestamp()
                .WithTitle("Verfügbare Streams");

            foreach (StreamConfig? stream in _audioConfig.Streams)
            {

                string? key = stream.Name;
                if (string.IsNullOrEmpty(stream.DisplayName) == false)
                {
                    key = key + " - " + stream.DisplayName;
                }

                builder.AddField(key, stream.Url, false);
            }

            await new DiscordMessageBuilder()
              .WithEmbed(builder.Build())
              .WithReply(ctx.Message.Id, true)
              .SendAsync(ctx.Channel);
        }

        [Command("Stream")]
        public async Task PlayStreamAsync(CommandContext ctx, string stream)
        {
            await ctx.TriggerTypingAsync();
            var player = _audioService.Value.GetPlayer<QueuedLavalinkPlayer>(ctx.Guild.Id);

            if (player == null)
            {
                await new DiscordMessageBuilder()
                   .WithEmbed(EmbedHandler.CreateErrorEmbed("Music", "You Must First Join a Voice Channel."))
                   .WithReply(ctx.Message.Id, true)
                   .SendAsync(ctx.Channel);

                return;
            }

            if (stream.StartsWith("http"))
            {
                await new DiscordMessageBuilder()
                  .WithEmbed(EmbedHandler.CreateErrorEmbed("Music", "Please specify the stream name"))
                  .WithReply(ctx.Message.Id, true)
                  .SendAsync(ctx.Channel);

                return;
            }

            StreamConfig? audioStream = null;
            for (int i = 0; i < _audioConfig.Streams.Count; i++)
            {
                if (string.Equals(stream, _audioConfig.Streams[i].Name, StringComparison.OrdinalIgnoreCase))
                {
                    audioStream = _audioConfig.Streams[i];
                    break;
                }
            }

            if (audioStream == null)
            {
                await new DiscordMessageBuilder()
                .WithEmbed(EmbedHandler.CreateErrorEmbed("Music", $"No Stream with the name {stream} found"))
                .WithReply(ctx.Message.Id, true)
                .SendAsync(ctx.Channel);
                return;
            }

            var track = await _audioService.Value.GetTrackAsync(audioStream.Url, SearchMode.None);

            //If we couldn't find anything, tell the user.
            if (track == null)
            {
                await new DiscordMessageBuilder()
               .WithEmbed(EmbedHandler.CreateErrorEmbed("Music", $"I wasn't able to find anything for {audioStream.Url}."))
               .WithReply(ctx.Message.Id, true)
               .SendAsync(ctx.Channel);

                return;
            }

            var queueCount = await player.PlayAsync(track);

            if (queueCount > 0)
            {
                await new DiscordMessageBuilder()
                  .WithEmbed(EmbedHandler.CreateBasicEmbed("Music", $"{track!.Title} has been added to queue.", DiscordColor.Blue))
                  .WithReply(ctx.Message.Id, true)
                  .SendAsync(ctx.Channel);
            }
            else
            {
                List<DiscordField> fields = new()
                {
                    new DiscordField("Channel", ctx.Guild.GetChannel(player.VoiceChannelId!.Value).Name)
                };

                DiscordAuthor author = new(ctx.User.Username, IconUrl: ctx.User.AvatarUrl);

                await new DiscordMessageBuilder()
                 .WithEmbed(EmbedHandler.CreateBasicEmbed("Music", $"Now Playing: {track!.Title}\nUrl: {track.Source}", DiscordColor.Blue, fields, author))
                 .WithReply(ctx.Message.Id, true)
                 .SendAsync(ctx.Channel);
            }
        }

        [Command("List")]
        public async Task ListAsync(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();
            /* Create a string builder we can use to format how we want our list to be displayed. */
            StringBuilder? descriptionBuilder = new();

            /* Get The Player and make sure it isn't null. */
            var player = _audioService.Value.GetPlayer<QueuedLavalinkPlayer>(ctx.Guild.Id);

            if (player == null)
            {
                await new DiscordMessageBuilder()
                   .WithEmbed(EmbedHandler.CreateErrorEmbed("Music", "You Must First Join a Voice Channel."))
                   .WithReply(ctx.Message.Id, true)
                   .SendAsync(ctx.Channel);

                return;
            }

            if (player.State == PlayerState.Playing)
            {
                /*If the queue count is less than 1 and the current track IS NOT null then we wont have a list to reply with.
                    In this situation we simply return an embed that displays the current track instead. */
                if (player.Queue.Count < 1)
                {
                    await new DiscordMessageBuilder()
                       .WithEmbed(EmbedHandler.CreateBasicEmbed($"Now Playing: {player.CurrentTrack!.Title}", "Nothing Else Is Queued.", DiscordColor.Blue))
                       .WithReply(ctx.Message.Id, true)
                       .SendAsync(ctx.Channel);

                    return;
                }
                else
                {
                    /* Now we know if we have something in the queue worth replying with, so we itterate through all the Tracks in the queue.
                     *  Next Add the Track title and the url however make use of Discords Markdown feature to display everything neatly.
                        This trackNum variable is used to display the number in which the song is in place. (Start at 2 because we're including the current song.*/
                    int trackNum = 2;
                    foreach (var track in player.Queue)
                    {
                        descriptionBuilder.Append($"{trackNum}: [{track.Title}]({track.Source})\n");
                        trackNum++;
                    }

                    await new DiscordMessageBuilder()
                          .WithEmbed(EmbedHandler.CreateBasicEmbed("Music Playlist", $"Now Playing: [{player.CurrentTrack!.Title}]({player.CurrentTrack.Source}) \n{descriptionBuilder}", DiscordColor.Blue))
                          .WithReply(ctx.Message.Id, true)
                          .SendAsync(ctx.Channel);
                }
            }
            else
            {
                await new DiscordMessageBuilder()
                 .WithEmbed(EmbedHandler.CreateErrorEmbed("Music", "Player doesn't seem to be playing anything right now. If this is an error, Please Contact Draxis."))
                 .WithReply(ctx.Message.Id, true)
                 .SendAsync(ctx.Channel);
            }
        }

        [Command("Skip")]
        public async Task SkipTrackAsync(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();
            var player = _audioService.Value.GetPlayer<QueuedLavalinkPlayer>(ctx.Guild.Id);

            if (player == null)
            {
                await new DiscordMessageBuilder()
                   .WithEmbed(EmbedHandler.CreateErrorEmbed("Music", "You Must First Join a Voice Channel."))
                   .WithReply(ctx.Message.Id, true)
                   .SendAsync(ctx.Channel);

                return;
            }

            /* Check The queue, if it is less than one (meaning we only have the current song available to skip) it wont allow the user to skip. User is expected to use the Stop command if they're only wanting to skip the current song. */
            if (player.Queue.Count < 1)
            {
                await new DiscordMessageBuilder()
                  .WithEmbed(EmbedHandler.CreateErrorEmbed("Music, SkipTrack", $"Unable To skip a track as there is only One or No songs currently playing."))
                  .WithReply(ctx.Message.Id, true)
                  .SendAsync(ctx.Channel);
            }
            else
            {
                var title = player.CurrentTrack!.Title;
                await player.SkipAsync(1);

                await new DiscordMessageBuilder()
                         .WithEmbed(EmbedHandler.CreateBasicEmbed("Music Skip", $"I have successfully skiped {title}", DiscordColor.Blue))
                         .WithReply(ctx.Message.Id, true)
                         .SendAsync(ctx.Channel);
            }
        }

        [Command("ClearQueue")]
        public async Task ClearPlaylistAsync(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();
            var player = _audioService.Value.GetPlayer<QueuedLavalinkPlayer>(ctx.Guild.Id);

            if (player == null)
            {
                await new DiscordMessageBuilder()
                   .WithEmbed(EmbedHandler.CreateErrorEmbed("Music", "You Must First Join a Voice Channel."))
                   .WithReply(ctx.Message.Id, true)
                   .SendAsync(ctx.Channel);

                return;
            }

            if (player.State is PlayerState.Playing)
            {
                player.Queue.Clear();
            }

            await new DiscordMessageBuilder()
                    .WithEmbed(EmbedHandler.CreateBasicEmbed("Music", "I Have cleared the playlist.", DiscordColor.Blue))
                    .WithReply(ctx.Message.Id, true)
                    .SendAsync(ctx.Channel);
        }

    }
}
