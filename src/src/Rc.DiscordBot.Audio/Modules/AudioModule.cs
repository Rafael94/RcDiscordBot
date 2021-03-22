using Discord;
using Discord.Commands;
using Rc.DiscordBot.Services;
using System.Threading.Tasks;

namespace Rc.DiscordBot.Modules
{
    [Name("Audio")]
    public class AudioModule : ModuleBase<SocketCommandContext>
    {
        private readonly AudioService _audioService;

        public AudioModule(AudioService audioService)
        {
            _audioService = audioService;
        }

        [Command("Join", RunMode = RunMode.Async)]
        [Summary("Den Bot zum aktuellen Channel")]
        public async Task JoinCmd(IVoiceChannel? channel = null)
        {
            // Get the audio channel
            channel ??= (Context.User as IGuildUser)?.VoiceChannel;
            if (channel == null)
            {
                await ReplyAsync("User must be in a voice channel, or a voice channel must be passed as an argument.");
                return;
            }

            await _audioService.JoinAudio(Context.Guild, channel);
        }

        // Remember to add preconditions to your commands,
        // this is merely the minimal amount necessary.
        // Adding more commands of your own is also encouraged.
        [Command("Leave", RunMode = RunMode.Async)]
        [Alias("Stop")]
        [Summary("Stopt die Musik und der Bot wird aus dem Channel entfernt")]
        public async Task LeaveCmd()
        {
            await _audioService.LeaveAudioAsync(Context.Guild);
        }

        [Command("PlayStream", RunMode = RunMode.Async)]
        [Summary("Spielt den übergebenen Stream ab")]
        public async Task PlayCmd([Summary("Einen im Bot hinterlegtem Stream (ListStreams) oder einen Stream")] string streamUrlOrStreamName, [Summary("Zahl{db}(ENG Format), Loudnorm, Dynaudnorm")] string? volume = null)
        {
            await _audioService.PlayStreamAsync(Context, streamUrlOrStreamName, volume);
        }

        [Command("ListStreams", RunMode = RunMode.Async)]
        public async Task ListStreamsCmd()
        {
            await _audioService.PrintAvailableStreamsAsync(Context);
        }

    }
}
