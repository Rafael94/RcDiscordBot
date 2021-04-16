
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

    //    public static async Task TrackEnded(TrackEndedEventArgs args)

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