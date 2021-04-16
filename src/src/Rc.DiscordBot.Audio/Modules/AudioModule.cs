
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Lavalink4NET;
using Lavalink4NET.Player;
using Lavalink4NET.Rest;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Rc.DiscordBot.Handlers;
using Rc.DiscordBot.Models;
using System;
using System.Collections.Generic;
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
            if (channel.Type != ChannelType.Voice)
            {
                await ctx.RespondAsync("Not a valid voice channel.");
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
            var player = _audioService.Value.GetPlayer<QueuedLavalinkPlayer>(ctx.Guild.Id);

            if (player == null)
            {
                return;
            }
            LavalinkTrack? track;

            if (search.StartsWith("http"))
            {
                track = await _audioService.Value.GetTrackAsync(search, SearchMode.None);
            }
            else
            {
                track = await _audioService.Value.GetTrackAsync(search, SearchMode.YouTube);

                if (track == null)
                {
                    track = await _audioService.Value.GetTrackAsync(search, SearchMode.SoundCloud);
                }
            }

            if (track == null)
            {
                return;
            }

            await player.PlayAsync(track);
        }

        [Command("pause")]
        public async Task PauseAsync(CommandContext ctx)
        {
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
    }


    /*[Name("Audio")]
    public class AudioModule : ModuleBase<SocketCommandContext>
    {
        [Command("List")]
        public async Task List()
        {
            await ReplyAsync(embed: await _audioService.ListAsync(Context.Guild));
        }

        [Command("Skip")]
        public async Task Skip()
        {
            await ReplyAsync(embed: await _audioService.SkipTrackAsync(Context.Guild));
        }


        [Command("ClearPlaylist")]
        public async Task ClearPlaylist()
        {
            await ReplyAsync(embed: await _audioService.ClearPlaylistAsync(Context.Guild));
        }

        [Command("Genius", RunMode = RunMode.Async)]
        public async Task Genius()
        {
            await ReplyAsync(embed: await _audioService.ShowGeniusLyrics(Context.Guild));
        }
    }*/
}
