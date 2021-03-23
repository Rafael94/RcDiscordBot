using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Rc.DiscordBot.Services;
using System.Threading.Tasks;

namespace Rc.DiscordBot.Modules
{
    [Name("Audio")]
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
            => await ReplyAsync(embed: await _audioService.JoinAsync(Context.Guild, (Context.User as IVoiceState)!, (Context.Channel as ITextChannel)!));

        [Command("Leave")]
        public async Task Leave()
                 => await ReplyAsync(embed: await _audioService.LeaveAsync(Context.Guild));


        [Command("Play")]
        public async Task Play([Remainder] string search)
            => await ReplyAsync(embed: await _audioService.PlayAsync(Context.User as SocketGuildUser, Context.Guild, search));

        [Command("SC"), Alias("SoundCloud"), Summary("Such den Track in SoundCloud")]
        public async Task PlaySoundCloud([Remainder] string search)
           => await ReplyAsync(embed: await _audioService.PlayAsync(Context.User as SocketGuildUser, Context.Guild, search, LavaLinkAudio.FallbackSearch.SoundCloud));

        [Command("Stop")]
        public async Task Stop()
            => await ReplyAsync(embed: await _audioService.StopAsync(Context.Guild));

        [Command("List")]
        public async Task List()
            => await ReplyAsync(embed: await _audioService.ListAsync(Context.Guild));

        [Command("Skip")]
        public async Task Skip()
            => await ReplyAsync(embed: await _audioService.SkipTrackAsync(Context.Guild));

        [Command("Volume")]
        public async Task Volume(int volume)
            => await ReplyAsync(await _audioService.SetVolumeAsync(Context.Guild, volume));

        [Command("Pause")]
        public async Task Pause()
            => await ReplyAsync(await _audioService.PauseAsync(Context.Guild));

        [Command("Resume")]
        public async Task Resume()
            => await ReplyAsync(await _audioService.ResumeAsync(Context.Guild));

        [Command("ListStreams")]
        public async Task ListStreams()
           => await ReplyAsync(embed: await _audioService.ListAvailableStreamsAsync());

        [Command("Stream")]
        public async Task PlayStream(string stream)
          => await ReplyAsync(embed: await _audioService.PlayStreamAsync(Context.User as SocketGuildUser, Context.Guild, stream));

        [Command("ClearPlaylist")]
        public async Task ClearPlaylist()
          => await ReplyAsync(embed: await _audioService.ClearPlaylistAsync(Context.Guild));

        [Command("Genius", RunMode = RunMode.Async)]
        public async Task Genius()
          => await ReplyAsync(embed: await _audioService.ShowGeniusLyrics(Context.Guild));
    }
}
