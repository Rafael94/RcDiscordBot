using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Options;
using Rc.DiscordBot.Models;
using Rc.DiscordBot.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwitchLib.Api;
using TwitchLib.Api.Helix.Models.Streams.GetStreams;
using TwitchLib.Api.Services;
using TwitchLib.Api.Services.Events.LiveStreamMonitor;

namespace Rc.DiscordBot.Services
{
    public class TwitchLiveMonitorService
    {
        private enum ChannelStatus
        {
            Online,
            Updated,
            Offline
        }

        private readonly TwitchConfig _twitchConfig;
        private readonly DiscordSocketClient _client;
        private readonly Dictionary<string, OnlineStreamValues> _onlineStreams = new();

        private LiveStreamMonitorService? _monitor;
        private TwitchAPI? _api;


        public TwitchLiveMonitorService(IOptions<TwitchConfig> twitchConfig, DiscordSocketClient client)
        {
            _twitchConfig = twitchConfig.Value;
            _client = client;
        }

        /// <summary>
        /// Aktiviert das Überwachen der Channels
        /// 
        /// </summary>
        /// <returns>True wenn einegrichtet</returns>
        public bool ConfigLiveMonitor()
        {
            if (string.IsNullOrWhiteSpace(_twitchConfig.Secret) || string.IsNullOrWhiteSpace(_twitchConfig.ClientId))
            {
                return false;
            }


            if (_twitchConfig.TwitchChannels?.Count == 0)
            {
                return false;
            }

            _api = new TwitchAPI();

            _api.Settings.ClientId = _twitchConfig.ClientId;
            _api.Settings.Secret = _twitchConfig.Secret;
            _api.Settings.AccessToken = _api.Helix.Channels.GetAccessToken();

            _monitor = new LiveStreamMonitorService(_api, _twitchConfig.OnlineCheckIntervall);

            _monitor.SetChannelsByName(_twitchConfig.TwitchChannels!.Select(x => x.Key).ToList());

            _monitor.OnStreamOnline += Monitor_OnStreamOnline;
            _monitor.OnStreamOffline += Monitor_OnStreamOffline;
            _monitor.OnStreamUpdate += Monitor_OnStreamUpdate;

            _monitor.Start();

            return true;
        }

        public void Stop()
        {
            _monitor?.Stop();
        }

        private void Monitor_OnStreamOnline(object? sender, OnStreamOnlineArgs e)
        {
            var embed = CreateEmbed(_twitchConfig.ChannelOnline, e.Channel, e.Stream);

            SendMessageAsync(embed, e.Channel, ChannelStatus.Online).GetAwaiter().GetResult();

            _onlineStreams.Add(e.Channel, new OnlineStreamValues(e.Stream.Title, e.Stream.GameName));
        }


        private void Monitor_OnStreamUpdate(object? sender, OnStreamUpdateArgs e)
        {
            OnlineStreamValues stream = _onlineStreams[e.Channel];

            if (stream.Title == e.Stream.Title && stream.Game == e.Stream.GameName)
            {
                return;
            }

            // Werte anpassen
            stream.Title = e.Stream.Title;
            stream.Game = e.Stream.GameName;

            var embed = CreateEmbed(_twitchConfig.ChannelUpdated, e.Channel, e.Stream);
            SendMessageAsync(embed, e.Channel, ChannelStatus.Updated).GetAwaiter().GetResult();
        }

        private void Monitor_OnStreamOffline(object? sender, OnStreamOfflineArgs e)
        {
            var embed = CreateEmbed(_twitchConfig.ChannelOffline, e.Channel, e.Stream);
            SendMessageAsync(embed, e.Channel, ChannelStatus.Offline).GetAwaiter().GetResult();
            _onlineStreams.Remove(e.Channel);
        }

        private Embed CreateEmbed(string titleTemplate, string channel, Stream stream)
        {
            var formatter = new StringFormatter(titleTemplate);
            formatter.Add("@ChannelName", channel);
            formatter.Add("@UserName", stream.UserName);
            formatter.Add("@Title", stream.Title);
            formatter.Add("@GameName", stream.GameName);
            formatter.Add("@Type", stream.Type);

            var notificationText = formatter.ToString();

            return new EmbedBuilder()
                               .WithTitle(notificationText)
                               .WithUrl("https://twitch.tv/" + channel)
                               .WithThumbnailUrl(stream.ThumbnailUrl.Replace("{width}", _twitchConfig.ThumbnailWidth.ToString()).Replace("{height}", _twitchConfig.ThumbnailHeight.ToString()))
                               .AddField("Titel", stream.Title)
                               .AddField("Game", stream.GameName)
                               .AddField("Zuschauer", stream.ViewerCount)
                               .AddField("Typ", stream.Type)
                               .WithAuthor(stream.UserName)
                               .WithFooter("Erstellt von RC Bot. https://twitch.tv/vincitorede")
                               .Build();
        }
        private async Task SendMessageAsync(Embed embed, string channelName, ChannelStatus status)
        {
            if (_twitchConfig.TwitchChannels!.TryGetValue(channelName, out TwitchChannel? twitchChannel) == false)
            {
                return;
            }

            if(status == ChannelStatus.Online && twitchChannel.NotificationWhenOnline == false)
            {
                return;
            } else if(status == ChannelStatus.Offline && twitchChannel.NotificationWhenOffline == false)
            {
                return;
            }
            else if (status == ChannelStatus.Updated && twitchChannel.NotificationWhenUpdated == false)
            {
                return;
            }

            var servers = _client.Guilds;

            // Server durchlaufen die Benachrichtigt werden sollen
            foreach (var discordServer in twitchChannel.DiscordServers)
            {
                SocketGuild? socketGuild = null;

                // Discord Server ID ermitteln
                foreach (var server in servers)
                {
                    if (string.Equals(server.Name, discordServer.Name, StringComparison.OrdinalIgnoreCase))
                    {
                        socketGuild = server;
                        break;
                    }
                }

                // Wenn der DiscordServer nicht gefunden worden ist, den nächsten Server abrufen
                if (socketGuild == null)
                {
                    continue;
                }

                foreach (var channel in socketGuild.Channels)
                {
                    // Nur Text Channels beachten
                    if ((channel is SocketTextChannel txtChannel))
                    {
                        // Prüfen ob es im Channel gepostet werden soll
                        if (string.Equals(channel.Name, discordServer.Channel, StringComparison.OrdinalIgnoreCase))
                        {
                            await txtChannel.SendMessageAsync(embed: embed);
                            break;
                        }
                    }
                }

            }
        }

    }
}
