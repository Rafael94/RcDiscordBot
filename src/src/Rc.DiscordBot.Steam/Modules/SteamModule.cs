using Discord;
using Discord.Commands;
using Microsoft.Extensions.Options;
using Rc.DiscordBot.Handlers;
using Rc.DiscordBot.Models;
using SteamWebAPI2.Interfaces;
using SteamWebAPI2.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace Rc.DiscordBot.Modules
{
    [Name("Rss Feed")]
    [Group("Steam")]
    public class SteamModule : ModuleBase<SocketCommandContext>
    {

        [Group("News")]
        public class News : ModuleBase<SocketCommandContext>
        {
            private readonly SteamConfig _steamConfig;

            public News(IOptions<SteamConfig> steamConfig)
            {
                _steamConfig = steamConfig.Value;
            }

            [Command("list")]
            [Summary("Listet die hinterlege News auf")]
            public async Task GetNewsListAsync()
            {
                List<EmbedFieldBuilder>? fileds = new();

                for (int i = 0; i < _steamConfig.News.Count; i++)
                {
                    var newsConfig = _steamConfig.News[i];

                    MessageSendToDiscordServer? discordServer = newsConfig.DiscordServers.Where(x => string.Equals(x.Name, Context.Guild.Name, System.StringComparison.OrdinalIgnoreCase)).FirstOrDefault();

                    fileds.Add(
                        new EmbedFieldBuilder()
                        .WithIsInline(false)
                        .WithName($"{newsConfig.Name}  { (discordServer == null ? "" : " - " + discordServer.Channel + "")}")
                        .WithValue(newsConfig.AppId)
                        );
                }

                await ReplyAsync(embed: await EmbedHandler.CreateBasicEmbed("News", $"Hinterlegte RSS Feeds", Color.Blue, fileds));
            }
        }

        [Group("Game")]
        public class Game : ModuleBase<SocketCommandContext>
        {
            private readonly SteamStore _steamStore;
            private readonly SteamWebInterfaceFactory _steamWebInterfaceFactory;

            public Game(SteamStore steamStore, SteamWebInterfaceFactory steamWebInterfaceFactory)
            {
                _steamStore = steamStore;
                _steamWebInterfaceFactory = steamWebInterfaceFactory;
            }

            [Command("info")]
            [Summary("Listet Spieleinformationen auf")]
            public async Task GetNewsListAsync(uint gameId)
            {
                var gameDetails = await _steamStore.GetStoreAppDetailsAsync(gameId);

                if (gameDetails == null)
                {
                    await ReplyAsync(embed: await EmbedHandler.CreateErrorEmbed("steam game info", "Spiel wurde nicht gefunden"));
                    return;
                }

                var steamInterface = _steamWebInterfaceFactory.CreateSteamWebInterface<SteamUserStats>(new HttpClient());

                var curentPlayerCount = await steamInterface.GetNumberOfCurrentPlayersForGameAsync(gameId);
      

                Embed embed = new EmbedBuilder()
                .WithTitle("Informationen zum Spiel " + gameDetails.Name)
                .WithCustomDescription(gameDetails.AboutTheGame)
                .WithUrl(gameDetails.Website)
                .WithImageUrl(gameDetails.HeaderImage)
                .AddField("Preis", gameDetails.PriceOverview.FinalFormatted)
                .AddField("Aktuelle Anzahl Spieler", curentPlayerCount.Data)
                .AddField("Anzahl Bewertungen", gameDetails.Recommendations.Total)
                .AddField("Kategorien", string.Join(',', gameDetails.Categories.Select(x => x.Description)))
                .AddField("Entwickler", string.Join(',', gameDetails.Developers))
                .AddField("Genres", string.Join(',', gameDetails.Genres.Select(x => x.Description)))
                .AddField("Kostenlos", gameDetails.IsFree)
                .AddField("Publisher", string.Join(',', gameDetails.Publishers))
                .AddField("Veröffentlichungsdatum", gameDetails.ReleaseDate.Date)
                .WithColor(Color.Blue)
                .WithBotFooter()
                .Build();

                await ReplyAsync(embed: embed);
            }

            [Command("find")]
            [Summary("Gibt die AppId des Spiels zurück (Maximal 15 Einträge)")]
            public async Task GetFindAppIdAsync([Remainder] string name)
            {

                var steamInterface = _steamWebInterfaceFactory.CreateSteamWebInterface<SteamApps>(new HttpClient());
                var games = await steamInterface.GetAppListAsync();
                List<EmbedFieldBuilder> fileds = new();

                foreach(var game in games.Data)
                {
                    if(game.Name.Contains(name, StringComparison.CurrentCultureIgnoreCase))
                    {
                        fileds.Add(new()
                        {
                            Name = game.Name,
                            Value = game.AppId
                        });

                        if (fileds.Count >= 15)
                        {
                            break;
                        }
                    }
                }

                await ReplyAsync(embed: await EmbedHandler.CreateBasicEmbed("Folgende Spiele wurden gefunden", "Es werden nur maximal 15 Einträge zurückgegeben", Color.Blue, fileds));
            }
        }

        [Group("Player")]
        public class Player : ModuleBase<SocketCommandContext>
        {
            private readonly SteamWebInterfaceFactory _steamWebInterfaceFactory;

            public Player( SteamWebInterfaceFactory steamWebInterfaceFactory)
            {
                _steamWebInterfaceFactory = steamWebInterfaceFactory;
            }

            [Command("info")]
            public async Task GetNewsListAsync(ulong userId)
            {
                var steamInterface = _steamWebInterfaceFactory.CreateSteamWebInterface<SteamUser>(new HttpClient());
                var playerSummary = await steamInterface.GetPlayerSummaryAsync(userId);

                if (playerSummary?.Data == null)
                {
                    await ReplyAsync(embed: await EmbedHandler.CreateErrorEmbed("steam player info", "Benutzer wurde nicht gefunden"));
                    return;
                }

                List<EmbedFieldBuilder> fields = new()
                {                        
                    new EmbedFieldBuilder().WithName("Benutzer Status").WithValue(playerSummary.Data.UserStatus.ToString()),
                    new EmbedFieldBuilder().WithName("Bei Steam seit").WithValue(playerSummary.Data.AccountCreatedDate),
                };

                if(string.IsNullOrWhiteSpace(playerSummary.Data.PlayingGameName) == false)
                {
                    fields.Add(new EmbedFieldBuilder().WithName("Aktuelles Spiel").WithValue(playerSummary.Data.PlayingGameName));
                }

                await ReplyAsync(embed: await EmbedHandler.CreateBasicEmbed("Spieler " + playerSummary.Data.Nickname, $"Informationen zum Benutzer", Color.Blue, fields, url: playerSummary.Data.ProfileUrl, imageUrl: playerSummary.Data.AvatarFullUrl));
            }
        }
    }
}
