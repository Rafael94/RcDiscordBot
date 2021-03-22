using Discord;
using Discord.Audio;
using Discord.Commands;
using Microsoft.Extensions.Options;
using Rc.DiscordBot.Models;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Rc.DiscordBot.Services
{
    public class AudioService
    {
        private readonly ConcurrentDictionary<ulong, IAudioClient> ConnectedChannels = new();
        private readonly AudioConfig _audioConfig;

        public AudioService(IOptions<AudioConfig> audioConfig)
        {
            _audioConfig = audioConfig.Value;
        }

        public async Task JoinAudio(IGuild guild, IVoiceChannel target)
        {
            if (ConnectedChannels.TryGetValue(guild.Id, out _))
            {
                return;
            }
            if (target.Guild.Id != guild.Id)
            {
                return;
            }

            var audioClient = await target.ConnectAsync();

            if (ConnectedChannels.TryAdd(guild.Id, audioClient))
            {
                // If you add a method to log happenings from this service,
                // you can uncomment these commented lines to make use of that.
                //await Log(LogSeverity.Info, $"Connected to voice on {guild.Name}.");
            }
        }

        public async Task LeaveAudioAsync(IGuild guild)
        {
            if (ConnectedChannels.TryRemove(guild.Id, out IAudioClient? client))
            {
                await client.StopAsync();
                //await Log(LogSeverity.Info, $"Disconnected from voice on {guild.Name}.");
            }
        }

        public async Task PrintAvailableStreamsAsync(SocketCommandContext context)
        {
            EmbedBuilder builder = new();

            builder.WithTitle("Verfügbare Streams");

            foreach (var stream in _audioConfig.Streams)
            {
                var key = stream.Name;
                if (string.IsNullOrEmpty(stream.DisplayName) == false)
                {
                    key = key + " - " + stream.DisplayName;
                }

                builder.AddField(key, stream.Url, false);
            }

            await context.Channel.SendMessageAsync("", false, builder.Build());
        }

        public async Task PlayStreamAsync(SocketCommandContext context, string url, string? volume)
        {
            if (ConnectedChannels.TryGetValue(context.Guild.Id, out IAudioClient? client) == false)
            {
                await context.Channel.SendMessageAsync($"The bot is not in any voice channel");
                return;
            }

            // Your task: Get a full path to the file if the value of 'path' is only a filename.
            if (!url.StartsWith("http"))
            {
                StreamConfig? audioStream = null;
                for(var i = 0;i<_audioConfig.Streams.Count;i++)
                {
                    if(string.Equals(url, _audioConfig.Streams[i].Name, StringComparison.OrdinalIgnoreCase))
                    {
                        audioStream = _audioConfig.Streams[i];
                        break;
                    }
                }

                if (audioStream == null)
                {
                    await context.Channel.SendMessageAsync($"Stream with the Name {url} not found");
                    return;
                }

                await context.Channel.SendMessageAsync("Now Playing: " + (audioStream.DisplayName ?? url));
                await SendAsync(client, audioStream.Url, audioStream.Normalization, audioStream.Volume);

            }
            else if (string.IsNullOrWhiteSpace(volume) == false)
            {
                await context.Channel.SendMessageAsync("Playing: " + url);
                if (volume.EndsWith("db", StringComparison.OrdinalIgnoreCase) || decimal.TryParse(volume, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out decimal _))
                {
                    await SendAsync(client, url, volume: volume);
                }
                else if (Enum.TryParse(volume, out StreamNormalization normalizator))
                {
                    await SendAsync(client, url, normalizator);
                }
                else
                {
                    await context.Channel.SendMessageAsync($"Parameter Volume({volume}) ist ungültig. Erlaubt: Zahl{{db}}(Eng Format), Loudnorm, Dynaudnorm");
                }
            }
            else
            {
                await context.Channel.SendMessageAsync("Playing: " + url);
                await SendAsync(client, url);
            }
        }

        private static Process CreateProcess(string path, StreamNormalization normalization, string? volume = null)
        {
            string? filter = null;

            if (string.IsNullOrWhiteSpace(volume) == false)
            {
                filter = $"-filter:a \"volume={volume}\"";
            }
            else if (normalization != StreamNormalization.None)
            {
                filter = $"-filter:a {normalization.ToString().ToLower()}";
            }


            return Process.Start(new ProcessStartInfo
            {
                FileName = "ffmpeg",
                Arguments = $"-hide_banner -loglevel panic -i \"{path}\" -ac 2 -f s16le -ar 48000 {filter} pipe:1",
                UseShellExecute = false,
                RedirectStandardOutput = true,
            })!;
        }

        private static async Task SendAsync(IAudioClient client, string path, StreamNormalization normalization = StreamNormalization.None, string? volume = null)
        {
            // Create FFmpeg using the previous example
            using var ffmpeg = CreateProcess(path, normalization, volume);
            using var output = ffmpeg.StandardOutput.BaseStream;
            using var discord = client.CreatePCMStream(AudioApplication.Music);
            client.Disconnected += (Exception ex) =>
            {
                discord?.Dispose();
                output?.Dispose();
                ffmpeg?.Dispose();

                return Task.CompletedTask;
            };

            try { await output.CopyToAsync(discord); }
            finally { await discord.FlushAsync(); }
        }
    }
}
