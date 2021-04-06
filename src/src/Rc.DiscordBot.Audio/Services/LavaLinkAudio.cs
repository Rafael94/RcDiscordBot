﻿
using Microsoft.Extensions.Options;
using Rc.DiscordBot.Handlers;
using Rc.DiscordBot.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;


namespace Rc.DiscordBot.Services
{
    public class LavaLinkAudio
    {

    //   public enum FallbackSearch
    //    {
    //        All,
    //        Youtube,
    //        SoundCloud
    //    }

    //    private readonly LavaNode _lavaNode;
    //    private readonly AudioConfig _audioConfig;
    //    private readonly BotConfig _botConfig;

    //    public LavaLinkAudio(LavaNode lavaNode, DiscordSocketClient discordClient, IOptions<AudioConfig> audioConfig, IOptions<BotConfig> botConfig)
    //    {
    //        discordClient.Ready += ReadyAsync;

    //        _lavaNode = lavaNode;
    //        _audioConfig = audioConfig.Value;
    //        _botConfig = botConfig.Value;
    //    }

    //    private async Task ReadyAsync()
    //    {
    //        await _lavaNode.ConnectAsync();
    //        _lavaNode.OnTrackEnded += TrackEnded;
    //    }

    //    public async Task<Embed> JoinAsync(IGuild guild, IVoiceState voiceState, ITextChannel textChannel)
    //    {
    //        if (_lavaNode.HasPlayer(guild))
    //        {
    //            return await EmbedHandler.CreateErrorEmbed("Music, Join", "I'm already connected to a voice channel!");
    //        }

    //        if (voiceState.VoiceChannel is null)
    //        {
    //            return await EmbedHandler.CreateErrorEmbed("Music, Join", "You must be connected to a voice channel!");
    //        }

    //        try
    //        {
    //            LavaPlayer? player = await _lavaNode.JoinAsync(voiceState.VoiceChannel, textChannel);

    //            return await EmbedHandler.CreateBasicEmbed("Music, Join", $"Joined {voiceState.VoiceChannel.Name}.", Color.Green);
    //        }
    //        catch (Exception ex)
    //        {
    //            return await EmbedHandler.CreateErrorEmbed("Music, Join", ex.Message);
    //        }
    //    }

    //    /*This is ran when a user uses the command Leave.
    //        Task Returns an Embed which is used in the command call. */
    //    public async Task<Embed> LeaveAsync(IGuild guild)
    //    {
    //        try
    //        {
    //            //Get The Player Via GuildID.
    //            LavaPlayer? player = _lavaNode.GetPlayer(guild);

    //            //if The Player is playing, Stop it.
    //            if (player.PlayerState is PlayerState.Playing)
    //            {
    //                await player.StopAsync();
    //            }

    //            //Leave the voice channel.
    //            await _lavaNode.LeaveAsync(player.VoiceChannel);

    //            //await LoggingService.LogInformationAsync("Music", $"Bot has left.");
    //            return await EmbedHandler.CreateBasicEmbed("Music", $"I've left. Thank you for playing moosik.", Color.Blue);
    //        }
    //        //Tell the user about the error so they can report it back to us.
    //        catch (InvalidOperationException ex)
    //        {
    //            return await EmbedHandler.CreateErrorEmbed("Music, Leave", ex.Message);
    //        }
    //    }

    //    /*This is ran when a user uses either the command Join or Play
    //       I decided to put these two commands as one, will probably change it in future. 
    //       Task Returns an Embed which is used in the command call.. */
    //    public async Task<Embed> PlayAsync(SocketGuildUser user,
    //        IGuild guild,
    //        string query,
    //        FallbackSearch fallbackSearch = FallbackSearch.Youtube)
    //    {
    //        //Check If User Is Connected To Voice Cahnnel.
    //        if (user.VoiceChannel == null)
    //        {
    //            return await EmbedHandler.CreateErrorEmbed("Music, Join/Play", "You Must First Join a Voice Channel.");
    //        }

    //        //Check the guild has a player available.
    //        if (!_lavaNode.HasPlayer(guild))
    //        {
    //            return await EmbedHandler.CreateErrorEmbed("Music, Play", "I'm not connected to a voice channel.");
    //        }

    //        try
    //        {
    //            //Get the player for that guild.
    //            LavaPlayer? player = _lavaNode.GetPlayer(guild);

    //            SearchResponse search;

    //            if (Uri.IsWellFormedUriString(query, UriKind.Absolute))
    //            {
    //                search = await _lavaNode.SearchAsync(query);
    //            }
    //            else if (fallbackSearch == FallbackSearch.Youtube)
    //            {
    //                search = await _lavaNode.SearchYouTubeAsync(query);
    //            }
    //            else
    //            {
    //                search = await _lavaNode.SearchSoundCloudAsync(query);
    //            }

    //            //If we couldn't find anything, tell the user.
    //            if (search.LoadStatus == LoadStatus.NoMatches)
    //            {
    //                return await EmbedHandler.CreateErrorEmbed("Music", $"I wasn't able to find anything for {query}.");
    //            }

    //            //Get the first track from the search results.
    //            //TODO: Add a 1-5 list for the user to pick from. (Like Fredboat)

    //            if (search.Tracks?.Count == 0)
    //            {
    //                return await EmbedHandler.CreateErrorEmbed("Music", $"I wasn't able to find the Stream.");
    //            }
    //            LavaTrack? track = search.Tracks?[0];

    //            bool isPlayList = string.IsNullOrWhiteSpace(search.Playlist.Name) == false;

    //            void AddPlayList()
    //            {
    //                for (int i = 1; i < search.Tracks!.Count; i++)
    //                {
    //                    player.Queue.Enqueue(search.Tracks[i]);
    //                }
    //            };

    //            List<EmbedFieldBuilder> fields = new()
    //            {
    //                new EmbedFieldBuilder().WithIsInline(true).WithName("Channel").WithValue(player.VoiceChannel.Name),
    //                new EmbedFieldBuilder().WithIsInline(true).WithName("Duration").WithValue(track!.Duration.ToString(@"hh\:mm\:ss"))
    //            };

    //            EmbedAuthorBuilder author = new EmbedAuthorBuilder()
    //                .WithName(user.Nickname)
    //                .WithIconUrl(user.GetAvatarUrl());

    //            if (isPlayList)
    //            {
    //                fields.Add(new EmbedFieldBuilder().WithIsInline(true).WithName("Playlist").WithValue("Yes"));
    //                fields.Add(new EmbedFieldBuilder().WithIsInline(true).WithName("Tracks").WithValue(search.Tracks!.Count));
    //            }

    //            //If the Bot is already playing music, or if it is paused but still has music in the playlist, Add the requested track to the queue.
    //            if (player.Track != null && player.PlayerState is PlayerState.Playing || player.PlayerState is PlayerState.Paused)
    //            {
    //                TimeSpan estimatedTime = player.Track!.Duration - player.Track.Position;

    //                foreach (LavaTrack? queuedTrack in player.Queue)
    //                {
    //                    estimatedTime += queuedTrack.Duration;
    //                }

    //                player.Queue.Enqueue(track);

    //                fields.Add(new EmbedFieldBuilder().WithIsInline(true).WithName("Position in queue").WithValue(player.Queue.Count));

    //                if (isPlayList)
    //                {
    //                    AddPlayList();
    //                }

    //                //await LoggingService.LogInformationAsync("Music", $"{track.Title} has been added to the music queue.");
    //                return await EmbedHandler.CreateBasicEmbed("Music", $"{track.Title} has been added to queue.", Color.Blue, fields, author);
    //            }

    //            //Player was not playing anything, so lets play the requested track.
    //            await player.PlayAsync(track);

    //            if (isPlayList)
    //            {
    //                AddPlayList();
    //            }

    //            //await LoggingService.LogInformationAsync("Music", $"Bot Now Playing: {track.Title}\nUrl: {track.Url}");

    //            return await EmbedHandler.CreateBasicEmbed("Music", $"Now Playing: {track.Title}\nUrl: {track.Url}", Color.Blue, fields, author);
    //        }

    //        //If after all the checks we did, something still goes wrong. Tell the user about it so they can report it back to us.
    //        catch (Exception ex)
    //        {
    //            return await EmbedHandler.CreateErrorEmbed("Music, Play", ex.Message);
    //        }

    //    }

    //    /*This is ran when a user uses either the command Join or Play
    //      I decided to put these two commands as one, will probably change it in future. 
    //      Task Returns an Embed which is used in the command call.. */
    //    public async Task<Embed> PlayStreamAsync(SocketGuildUser user, IGuild guild, string query)
    //    {
    //        //Check If User Is Connected To Voice Cahnnel.
    //        if (user.VoiceChannel == null)
    //        {
    //            return await EmbedHandler.CreateErrorEmbed("Music, Join/Play", "You Must First Join a Voice Channel.");
    //        }

    //        //Check the guild has a player available.
    //        if (!_lavaNode.HasPlayer(guild))
    //        {
    //            return await EmbedHandler.CreateErrorEmbed("Music, Play", "I'm not connected to a voice channel.");
    //        }


    //        if (query.StartsWith("http"))
    //        {
    //            return await EmbedHandler.CreateErrorEmbed("Music, Play", "Please specify the stream name");
    //        }

    //        StreamConfig? audioStream = null;
    //        for (int i = 0; i < _audioConfig.Streams.Count; i++)
    //        {
    //            if (string.Equals(query, _audioConfig.Streams[i].Name, StringComparison.OrdinalIgnoreCase))
    //            {
    //                audioStream = _audioConfig.Streams[i];
    //                break;
    //            }
    //        }

    //        if (audioStream == null)
    //        {
    //            return await EmbedHandler.CreateErrorEmbed("Music, Play", $"No Stream with the name {query} found");
    //        }

    //        query = audioStream.Url;

    //        try
    //        {
    //            //Get the player for that guild.
    //            LavaPlayer? player = _lavaNode.GetPlayer(guild);

    //            SearchResponse search = await _lavaNode.SearchAsync(query);

    //            //If we couldn't find anything, tell the user.
    //            if (search.LoadStatus == LoadStatus.NoMatches)
    //            {
    //                return await EmbedHandler.CreateErrorEmbed("Music", $"I wasn't able to find anything for {query}.");
    //            }

    //            //Get the first track from the search results.
    //            if (search.Tracks?.Count == 0)
    //            {
    //                return await EmbedHandler.CreateErrorEmbed("Music", $"I wasn't able to find the Stream.");
    //            }
    //            LavaTrack? track = search.Tracks?[0];

    //            //If the Bot is already playing music, or if it is paused but still has music in the playlist, Add the requested track to the queue.
    //            if (player.Track != null && player.PlayerState is PlayerState.Playing || player.PlayerState is PlayerState.Paused)
    //            {
    //                player.Queue.Enqueue(track);

    //                //await LoggingService.LogInformationAsync("Music", $"{track.Title} has been added to the music queue.");
    //                return await EmbedHandler.CreateBasicEmbed("Music", $"{track!.Title} has been added to queue.", Color.Blue);
    //            }

    //            //Player was not playing anything, so lets play the requested track.
    //            await player.PlayAsync(track);

    //            List<EmbedFieldBuilder> fields = new()
    //            {
    //                new EmbedFieldBuilder().WithIsInline(true).WithName("Channel").WithValue(player.VoiceChannel.Name),
    //            };

    //            EmbedAuthorBuilder author = new EmbedAuthorBuilder()
    //                .WithName(user.Nickname)
    //                .WithIconUrl(user.GetAvatarUrl());

    //            //await LoggingService.LogInformationAsync("Music", $"Bot Now Playing: {track.Title}\nUrl: {track.Url}");
    //            return await EmbedHandler.CreateBasicEmbed("Music", $"Now Playing: {track!.Title}\nUrl: {track.Url}", Color.Blue, fields, author);
    //        }

    //        //If after all the checks we did, something still goes wrong. Tell the user about it so they can report it back to us.
    //        catch (Exception ex)
    //        {
    //            return await EmbedHandler.CreateErrorEmbed("Music, Play", ex.Message);
    //        }

    //    }

    //    public Task<Embed> ListAvailableStreamsAsync()
    //    {
    //        EmbedBuilder builder = new();

    //        builder
    //            .WithColor(Color.Blue)
    //            .WithCurrentTimestamp()
    //            .WithTitle("Verfügbare Streams");

    //        foreach (StreamConfig? stream in _audioConfig.Streams)
    //        {
    //            string? key = stream.Name;
    //            if (string.IsNullOrEmpty(stream.DisplayName) == false)
    //            {
    //                key = key + " - " + stream.DisplayName;
    //            }

    //            builder.AddField(key, stream.Url, false);
    //        }

    //        return Task.FromResult(builder.Build());
    //    }

    //    /*This is ran when a user uses the command List 
    //        Task Returns an Embed which is used in the command call. */
    //    public async Task<Embed> ListAsync(IGuild guild)
    //    {
    //        try
    //        {
    //            /* Create a string builder we can use to format how we want our list to be displayed. */
    //            StringBuilder? descriptionBuilder = new();

    //            /* Get The Player and make sure it isn't null. */
    //            LavaPlayer? player = _lavaNode.GetPlayer(guild);
    //            if (player == null)
    //            {
    //                return await EmbedHandler.CreateErrorEmbed("Music, List", $"Could not aquire player.\nAre you using the bot right now? check {_botConfig.Prefix}Help for info on how to use the bot.");
    //            }

    //            if (player.PlayerState is PlayerState.Playing)
    //            {
    //                /*If the queue count is less than 1 and the current track IS NOT null then we wont have a list to reply with.
    //                    In this situation we simply return an embed that displays the current track instead. */
    //                if (player.Queue.Count < 1 && player.Track != null)
    //                {
    //                    return await EmbedHandler.CreateBasicEmbed($"Now Playing: {player.Track.Title}", "Nothing Else Is Queued.", Color.Blue);
    //                }
    //                else
    //                {
    //                    /* Now we know if we have something in the queue worth replying with, so we itterate through all the Tracks in the queue.
    //                     *  Next Add the Track title and the url however make use of Discords Markdown feature to display everything neatly.
    //                        This trackNum variable is used to display the number in which the song is in place. (Start at 2 because we're including the current song.*/
    //                    int trackNum = 2;
    //                    foreach (LavaTrack track in player.Queue)
    //                    {
    //                        descriptionBuilder.Append($"{trackNum}: [{track.Title}]({track.Url}) - {track.Id}\n");
    //                        trackNum++;
    //                    }
    //                    return await EmbedHandler.CreateBasicEmbed("Music Playlist", $"Now Playing: [{player.Track!.Title}]({player.Track.Url}) \n{descriptionBuilder}", Color.Blue);
    //                }
    //            }
    //            else
    //            {
    //                return await EmbedHandler.CreateErrorEmbed("Music, List", "Player doesn't seem to be playing anything right now. If this is an error, Please Contact Draxis.");
    //            }
    //        }
    //        catch (Exception ex)
    //        {
    //            return await EmbedHandler.CreateErrorEmbed("Music, List", ex.Message);
    //        }

    //    }

    //    /*This is ran when a user uses the command Skip 
    //        Task Returns an Embed which is used in the command call. */
    //    public async Task<Embed> SkipTrackAsync(IGuild guild)
    //    {
    //        try
    //        {
    //            LavaPlayer? player = _lavaNode.GetPlayer(guild);
    //            /* Check if the player exists */
    //            if (player == null)
    //            {
    //                return await EmbedHandler.CreateErrorEmbed("Music, List", $"Could not aquire player.\nAre you using the bot right now? check {_botConfig.Prefix}Help for info on how to use the bot.");
    //            }
    //            /* Check The queue, if it is less than one (meaning we only have the current song available to skip) it wont allow the user to skip.
    //User is expected to use the Stop command if they're only wanting to skip the current song. */
    //            if (player.Queue.Count < 1)
    //            {
    //                return await EmbedHandler.CreateErrorEmbed("Music, SkipTrack", $"Unable To skip a track as there is only One or No songs currently playing." +
    //                    $"\n\nDid you mean {_botConfig.Prefix}Stop?");
    //            }
    //            else
    //            {
    //                try
    //                {
    //                    /* Save the current song for use after we skip it. */
    //                    LavaTrack? currentTrack = player.Track;
    //                    /* Skip the current song. */
    //                    await player.SkipAsync();
    //                    // await LoggingService.LogInformationAsync("Music", $"Bot skipped: {currentTrack.Title}");
    //                    return await EmbedHandler.CreateBasicEmbed("Music Skip", $"I have successfully skiped {currentTrack.Title}", Color.Blue);
    //                }
    //                catch (Exception ex)
    //                {
    //                    return await EmbedHandler.CreateErrorEmbed("Music, Skip", ex.Message);
    //                }

    //            }
    //        }
    //        catch (Exception ex)
    //        {
    //            return await EmbedHandler.CreateErrorEmbed("Music, Skip", ex.Message);
    //        }
    //    }

    //    /*This is ran when a user uses the command Stop 
    //        Task Returns an Embed which is used in the command call. */
    //    public async Task<Embed> StopAsync(IGuild guild)
    //    {
    //        try
    //        {
    //            LavaPlayer? player = _lavaNode.GetPlayer(guild);

    //            if (player == null)
    //            {
    //                return await EmbedHandler.CreateErrorEmbed("Music, List", $"Could not aquire player.\nAre you using the bot right now? check {_botConfig.Prefix}Help for info on how to use the bot.");
    //            }

    //            /* Check if the player exists, if it does, check if it is playing.
    //                 If it is playing, we can stop.*/
    //            if (player.PlayerState is PlayerState.Playing)
    //            {
    //                await player.StopAsync();
    //            }

    //            //await LoggingService.LogInformationAsync("Music", $"Bot has stopped playback.");
    //            return await EmbedHandler.CreateBasicEmbed("Music Stop", "I Have stopped playback & the playlist has been cleared.", Color.Blue);
    //        }
    //        catch (Exception ex)
    //        {
    //            return await EmbedHandler.CreateErrorEmbed("Music, Stop", ex.Message);
    //        }
    //    }

    //    public async Task<Embed> ClearPlaylistAsync(IGuild guild)
    //    {
    //        try
    //        {
    //            LavaPlayer? player = _lavaNode.GetPlayer(guild);

    //            if (player == null)
    //            {
    //                return await EmbedHandler.CreateErrorEmbed("Music, List", $"Could not aquire player.\nAre you using the bot right now? check {_botConfig.Prefix}Help for info on how to use the bot.");
    //            }

    //            /* Check if the player exists, if it does, check if it is playing.
    //                 If it is playing, we can stop.*/
    //            if (player.PlayerState is PlayerState.Playing)
    //            {
    //                player.Queue.Clear();
    //            }

    //            //await LoggingService.LogInformationAsync("Music", $"Bot has stopped playback.");
    //            return await EmbedHandler.CreateBasicEmbed("Music Stop", "I Have cleared the playlist.", Color.Blue);
    //        }
    //        catch (Exception ex)
    //        {
    //            return await EmbedHandler.CreateErrorEmbed("Music, Stop", ex.Message);
    //        }
    //    }

    //    /*This is ran when a user uses the command Volume 
    //        Task Returns a String which is used in the command call. */
    //    public async Task<string> SetVolumeAsync(IGuild guild, int volume)
    //    {
    //        if (volume > 150 || volume <= 0)
    //        {
    //            return $"Volume must be between 1 and 150.";
    //        }
    //        try
    //        {
    //            LavaPlayer? player = _lavaNode.GetPlayer(guild);
    //            await player.UpdateVolumeAsync((ushort)volume);
    //            //await LoggingService.LogInformationAsync("Music", $"Bot Volume set to: {volume}");
    //            return $"Volume has been set to {volume}.";
    //        }
    //        catch (InvalidOperationException ex)
    //        {
    //            return ex.Message;
    //        }
    //    }

    //    public async Task<string> PauseAsync(IGuild guild)
    //    {
    //        try
    //        {
    //            LavaPlayer? player = _lavaNode.GetPlayer(guild);
    //            if (!(player.PlayerState is PlayerState.Playing))
    //            {
    //                await player.PauseAsync();
    //                return $"There is nothing to pause.";
    //            }

    //            await player.PauseAsync();
    //            return $"**Paused:** {player.Track.Title}, what a bamboozle.";
    //        }
    //        catch (InvalidOperationException ex)
    //        {
    //            return ex.Message;
    //        }
    //    }

    //    public async Task<string> ResumeAsync(IGuild guild)
    //    {
    //        try
    //        {
    //            LavaPlayer? player = _lavaNode.GetPlayer(guild);

    //            if (player.PlayerState is PlayerState.Paused)
    //            {
    //                await player.ResumeAsync();
    //            }

    //            return $"**Resumed:** {player.Track.Title}";
    //        }
    //        catch (InvalidOperationException ex)
    //        {
    //            return ex.Message;
    //        }
    //    }

    //    public static async Task TrackEnded(TrackEndedEventArgs args)
    //    {
    //        if (!args.Reason.ShouldPlayNext())
    //        {
    //            return;
    //        }

    //        if (!args.Player.Queue.TryDequeue(out LavaTrack? track))
    //        {
    //            await args.Player.TextChannel.SendMessageAsync("Playback Finished.");
    //        }

    //        await args.Player.PlayAsync(track);

    //        List<EmbedFieldBuilder> fields = new()
    //        {
    //            new EmbedFieldBuilder().WithIsInline(true).WithName("Channel").WithValue(args.Player.VoiceChannel.Name),
    //            new EmbedFieldBuilder().WithIsInline(true).WithName("Duration").WithValue(track.Duration.ToString(@"hh\:mm\:ss")),
    //            new EmbedFieldBuilder().WithIsInline(true).WithName("Remaining Tracks in Playlist").WithValue(args.Player.Queue.Count),
    //        };

    //        await args.Player.TextChannel.SendMessageAsync(
    //            embed: await EmbedHandler.CreateBasicEmbed("Now Playing", $"[{track.Title}]({track.Url})", Color.Blue, fields));
    //    }

    //    public async Task<Embed> ShowGeniusLyrics(IGuild guild)
    //    {
    //        if (!_lavaNode.TryGetPlayer(guild, out LavaPlayer? player))
    //        {
    //            return await EmbedHandler.CreateErrorEmbed("Genius", "I'm not connected to a voice channel.");
    //        }

    //        if (player.PlayerState != PlayerState.Playing)
    //        {
    //            return await EmbedHandler.CreateErrorEmbed("Genius", "Woaaah there, I'm not playing any tracks.");
    //        }

    //        string? lyrics = await player.Track.FetchLyricsFromGeniusAsync() ?? await player.Track.FetchLyricsFromOVHAsync();
    //        if (string.IsNullOrWhiteSpace(lyrics))
    //        {
    //            return await EmbedHandler.CreateErrorEmbed("Genius", $"No lyrics found for {player.Track.Title}");
    //        }

    //        string[]? splitLyrics = lyrics.Split('\n');
    //        StringBuilder? stringBuilder = new();
    //        foreach (string? line in splitLyrics)
    //        {

    //            stringBuilder.AppendLine(line);
    //        }

    //        return await EmbedHandler.CreateBasicEmbed("Music Stop", $"```{stringBuilder}```", Color.Blue);
    //    }
    }
}