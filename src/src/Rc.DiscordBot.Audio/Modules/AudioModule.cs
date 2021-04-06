
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Lavalink;
using Rc.DiscordBot.Services;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Rc.DiscordBot.Modules
{

    public class AudioModule : DSharpPlus.CommandsNext.BaseCommandModule
    {
        [Command("join")]
        public async Task JoinAsync(CommandContext ctx)
        {
            var lava = ctx.Client.GetLavalink();
            if (!lava.ConnectedNodes.Any())
            {
                await ctx.RespondAsync("The Lavalink connection is not established");
                return;
            }
            var channel = ctx.Member.VoiceState.Channel;

            if (channel.Type != ChannelType.Voice)
            {
                await ctx.RespondAsync("Not a valid voice channel.");
                return;
            }
            var node = lava.ConnectedNodes.Values.First();
            await node.ConnectAsync(channel);
            await ctx.RespondAsync($"Joined {channel.Name}!");
        }

        [Command("leave")]
        public async Task LeaveAsync(CommandContext ctx)
        {
            var lava = ctx.Client.GetLavalink();
            if (!lava.ConnectedNodes.Any())
            {
                await ctx.RespondAsync("The Lavalink connection is not established");
                return;
            }
            var channel = ctx.Member.VoiceState.Channel;
            if (channel.Type != ChannelType.Voice)
            {
                await ctx.RespondAsync("Not a valid voice channel.");
                return;
            }

            var node = lava.ConnectedNodes.Values.First();
            var conn = node.GetGuildConnection(channel.Guild);

            if (conn == null)
            {
                await ctx.RespondAsync("Lavalink is not connected.");
                return;
            }

            await conn.DisconnectAsync();
            await ctx.RespondAsync($"Left {channel.Name}!");
        }

        [Command]
        public async Task Play(CommandContext ctx, [RemainingText] string search)
        {
            //Important to check the voice state itself first, 
            //as it may throw a NullReferenceException if they don't have a voice state.
            if (ctx.Member.VoiceState == null || ctx.Member.VoiceState.Channel == null)
            {
                await ctx.RespondAsync("You are not in a voice channel.");
                return;
            }

            var lava = ctx.Client.GetLavalink();
            var node = lava.ConnectedNodes.Values.First();
            var conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);

            if (conn == null)
            {
                await ctx.RespondAsync("Lavalink is not connected.");
                return;
            }
            LavalinkLoadResult? loadResult;

            //We don't need to specify the search type here
            //since it is YouTube by default.
            if (search.StartsWith("http"))
            {
                loadResult = await node.Rest.GetTracksAsync(new Uri(search));
            }
            else
            {
                loadResult = await node.Rest.GetTracksAsync(search);
            }


            //If something went wrong on Lavalink's end                          
            if (loadResult.LoadResultType == LavalinkLoadResultType.LoadFailed

                //or it just couldn't find anything.
                || loadResult.LoadResultType == LavalinkLoadResultType.NoMatches)
            {
                await ctx.RespondAsync($"Track search failed for {search}.");
                return;
            }

            var track = loadResult.Tracks.First();

            await conn.PlayAsync(track);

            await ctx.RespondAsync($"Now playing {track.Title}!");
        }

        [Command]
        public async Task Pause(CommandContext ctx)
        {
            if (ctx.Member.VoiceState == null || ctx.Member.VoiceState.Channel == null)
            {
                await ctx.RespondAsync("You are not in a voice channel.");
                return;
            }

            var lava = ctx.Client.GetLavalink();
            var node = lava.ConnectedNodes.Values.First();
            var conn = node.GetGuildConnection(ctx.Member.VoiceState.Guild);

            if (conn == null)
            {
                await ctx.RespondAsync("Lavalink is not connected.");
                return;
            }

            if (conn.CurrentState.CurrentTrack == null)
            {
                await ctx.RespondAsync("There are no tracks loaded.");
                return;
            }

            await conn.PauseAsync();
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

        [Command("Join")]
        [Summary("Den Bot zum aktuellen Channel")]
        public async Task JoinAndPlay()
        {
            await ReplyAsync(embed: await _audioService.JoinAsync(Context.Guild, (Context.User as IVoiceState)!, (Context.Channel as ITextChannel)!));
        }

        [Command("Leave")]
        public async Task Leave()
        {
            await ReplyAsync(embed: await _audioService.LeaveAsync(Context.Guild));
        }

        [Command("Play")]
        public async Task Play([Remainder] string search)
        {
            await ReplyAsync(embed: await _audioService.PlayAsync((Context.User as SocketGuildUser)!, Context.Guild, search));
        }

        [Command("SC"), Alias("SoundCloud"), Summary("Such den Track in SoundCloud")]
        public async Task PlaySoundCloud([Remainder] string search)
        {
            await ReplyAsync(embed: await _audioService.PlayAsync((Context.User as SocketGuildUser)!, Context.Guild, search, LavaLinkAudio.FallbackSearch.SoundCloud));
        }

        [Command("Stop")]
        public async Task Stop()
        {
            await ReplyAsync(embed: await _audioService.StopAsync(Context.Guild));
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

        [Command("Volume")]
        public async Task Volume(int volume)
        {
            await ReplyAsync(await _audioService.SetVolumeAsync(Context.Guild, volume));
        }

        [Command("Pause")]
        public async Task Pause()
        {
            await ReplyAsync(await _audioService.PauseAsync(Context.Guild));
        }

        [Command("Resume")]
        public async Task Resume()
        {
            await ReplyAsync(await _audioService.ResumeAsync(Context.Guild));
        }

        [Command("ListStreams")]
        public async Task ListStreams()
        {
            await ReplyAsync(embed: await _audioService.ListAvailableStreamsAsync());
        }

        [Command("Stream")]
        public async Task PlayStream(string stream)
        {
            await ReplyAsync(embed: await _audioService.PlayStreamAsync((Context.User as SocketGuildUser)!, Context.Guild, stream));
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
