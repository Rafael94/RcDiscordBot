
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
                await ctx.RespondAsync("Not a valid voice channel.");
                return;
            }

            await _audioService.Value.JoinAsync<QueuedLavalinkPlayer>(ctx.Guild.Id, channel.Id, false, false);
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
        }

        [Command("leave")]
        [Aliases("stop")]
        public async Task LeaveAsync(CommandContext ctx)
        {
            var player = _audioService.Value.GetPlayer(ctx.Guild.Id);

            if (player != null)
            {
                await player.DisconnectAsync();
            }
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
                return;
            }

            player.PauseAsync();
        }

        [Command("resume")]
        public async Task ResumeAsync(CommandContext ctx)
        {
            var player = _audioService.Value.GetPlayer<QueuedLavalinkPlayer>(ctx.Guild.Id);

            if (player == null)
            {
                return;
            }

            await player.ResumeAsync();
        }

        [Command("skip")]
        public async Task Volume(CommandContext ctx, float volume)
        {
            var player = _audioService.Value.GetPlayer<QueuedLavalinkPlayer>(ctx.Guild.Id);

            if (player == null)
            {
                return;
            }

            if (volume < 0 || volume > 1)
            {
                return;
            }


            player.SetVolumeAsync(volume);
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

            await ctx.RespondAsync(builder.Build());
        }

        [Command("Stream")]
        public async Task PlayStreamAsync(CommandContext ctx, string stream)
        {
            var player = _audioService.Value.GetPlayer<QueuedLavalinkPlayer>(ctx.Guild.Id);

            if (player == null)
            {
                await ctx.RespondAsync(await EmbedHandler.CreateErrorEmbed("Music, Play", "You Must First Join a Voice Channel."));
                return;
            }

            if (stream.StartsWith("http"))
            {
                await ctx.RespondAsync(await EmbedHandler.CreateErrorEmbed("Music, Play", "Please specify the stream name"));
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
                await ctx.RespondAsync(await EmbedHandler.CreateErrorEmbed("Music, Play", $"No Stream with the name {stream} found"));
                return;
            }

            var track = await _audioService.Value.GetTrackAsync(audioStream.Url, SearchMode.None);

            //If we couldn't find anything, tell the user.
            if (track == null)
            {
                await ctx.RespondAsync(await EmbedHandler.CreateErrorEmbed("Music", $"I wasn't able to find anything for {audioStream.Url}."));
                return;
            }

            var queueCount = await player.PlayAsync(track);

            if (queueCount > 0)
            {
                await ctx.RespondAsync(await EmbedHandler.CreateBasicEmbed("Music", $"{track!.Title} has been added to queue.", DiscordColor.Blue));
            }
            else
            {
                List<DiscordField> fields = new()
                {
                    new DiscordField("Channel", ctx.Guild.GetChannel(player.VoiceChannelId.Value).Name)
                };

                DiscordAuthor author = new DiscordAuthor(ctx.User.Username, IconUrl: ctx.User.AvatarUrl);

                await ctx.RespondAsync(await EmbedHandler.CreateBasicEmbed("Music", $"Now Playing: {track!.Title}\nUrl: {track.Source}", DiscordColor.Blue, fields, author));

            }
        }
    }


    /*[Name("Audio")]
    public class AudioModule : ModuleBase<SocketCommandContext>
    {
        private readonly LavaLinkAudio _audioService;

        public AudioModule(LavaLinkAudio lavaLinkAudio)
        {
            _audioService = lavaLinkAudio;
        }


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
