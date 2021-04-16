
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
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
    [Group("Steam")]
    public class SteamModule : BaseCommandModule
    {

        [Group("News")]
        public class News : BaseCommandModule
        {
            private readonly SteamConfig _steamConfig;

            public News(IOptions<SteamConfig> steamConfig)
            {
                _steamConfig = steamConfig.Value;
            }

            [Command("list")]
            [Description("Listet die hinterlege News auf")]
            public async Task GetNewsListAsync(CommandContext ctx)
            {
                List<DiscordField>? fileds = new();

                for (int i = 0; i < _steamConfig.News.Count; i++)
                {
                    var newsConfig = _steamConfig.News[i];

                    MessageSendToDiscordServer? discordServer = newsConfig.DiscordServers.Where(x => string.Equals(x.Name, ctx.Guild.Name, System.StringComparison.OrdinalIgnoreCase)).FirstOrDefault();

                    fileds.Add(new DiscordField($"{newsConfig.Name}  { (discordServer == null ? "" : " - " + discordServer.Channel + "")}", newsConfig.AppId.ToString(), false) );
                }

                await new DiscordMessageBuilder()
                  .WithEmbed(EmbedHandler.CreateBasicEmbed("News", $"Hinterlegte RSS Feeds", DiscordColor.Blue))
                  .WithReply(ctx.Message.Id, true)
                  .SendAsync(ctx.Channel);
            }
        }

        [Group("Game")]
        public class Game : BaseCommandModule
        {
            private readonly SteamStore _steamStore;
            private readonly SteamWebInterfaceFactory _steamWebInterfaceFactory;

            public Game(SteamStore steamStore, SteamWebInterfaceFactory steamWebInterfaceFactory)
            {
                _steamStore = steamStore;
                _steamWebInterfaceFactory = steamWebInterfaceFactory;
            }

            [Command("info")]
            [Description("Listet Spieleinformationen auf")]
            public async Task GetNewsListAsync(CommandContext ctx, uint gameId)
            {
                var gameDetails = await _steamStore.GetStoreAppDetailsAsync(gameId);

                if (gameDetails == null)
                {
                    await new DiscordMessageBuilder()
                     .WithEmbed(EmbedHandler.CreateErrorEmbed("Steam game info", "Spiel wurde nicht gefunden"))
                     .WithReply(ctx.Message.Id, true)
                     .SendAsync(ctx.Channel);

                    return;
                }

                var steamInterface = _steamWebInterfaceFactory.CreateSteamWebInterface<SteamUserStats>(new HttpClient());

                var curentPlayerCount = await steamInterface.GetNumberOfCurrentPlayersForGameAsync(gameId);
      

                DiscordEmbed embed = new DiscordEmbedBuilder()
                .WithTitle("Informationen zum Spiel " + gameDetails.Name)
                .WithCustomDescription(gameDetails.AboutTheGame)
                .WithUrl(gameDetails.Website)
                .WithImageUrl(gameDetails.HeaderImage)
                .AddField("Preis", gameDetails.PriceOverview.FinalFormatted)
                .AddField("Aktuelle Anzahl Spieler", curentPlayerCount.Data.ToString())
                .AddField("Anzahl Bewertungen", gameDetails.Recommendations.Total.ToString())
                .AddField("Kategorien", string.Join(',', gameDetails.Categories.Select(x => x.Description)))
                .AddField("Entwickler", string.Join(',', gameDetails.Developers))
                .AddField("Genres", string.Join(',', gameDetails.Genres.Select(x => x.Description)))
                .AddField("Kostenlos", gameDetails.IsFree.ToString())
                .AddField("Publisher", string.Join(',', gameDetails.Publishers))
                .AddField("Veröffentlichungsdatum", gameDetails.ReleaseDate.Date)
                .WithColor(DiscordColor.Blue)
                .WithBotFooter()
                .Build();

                await new DiscordMessageBuilder()
                 .WithEmbed(embed)
                 .WithReply(ctx.Message.Id, true)
                 .SendAsync(ctx.Channel);
            }

            [Command("find")]
            [Description("Gibt die AppId des Spiels zurück (Maximal 15 Einträge)")]
            public async Task GetFindAppIdAsync(CommandContext ctx, [RemainingText] string name)
            {

                var steamInterface = _steamWebInterfaceFactory.CreateSteamWebInterface<SteamApps>(new HttpClient());
                var games = await steamInterface.GetAppListAsync();
                List<DiscordField> fileds = new();

                foreach(var game in games.Data)
                {
                    if(game.Name.Contains(name, StringComparison.CurrentCultureIgnoreCase))
                    {
                        fileds.Add(new(game.Name, game.AppId.ToString()));

                        if (fileds.Count >= 15)
                        {
                            break;
                        }
                    }
                }

                await new DiscordMessageBuilder()
                   .WithEmbed(EmbedHandler.CreateBasicEmbed("Folgende Spiele wurden gefunden", "Es werden nur maximal 15 Einträge zurückgegeben", DiscordColor.Blue, fileds))
                   .WithReply(ctx.Message.Id, true)
                   .SendAsync(ctx.Channel);
            }
        }

        [Group("Player")]
        public class Player : BaseCommandModule
        {
            private readonly SteamWebInterfaceFactory _steamWebInterfaceFactory;

            public Player( SteamWebInterfaceFactory steamWebInterfaceFactory)
            {
                _steamWebInterfaceFactory = steamWebInterfaceFactory;
            }

            [Command("info")]
            public async Task GetNewsListAsync(CommandContext ctx, ulong userId)
            {
                var steamInterface = _steamWebInterfaceFactory.CreateSteamWebInterface<SteamUser>(new HttpClient());
                var playerSummary = await steamInterface.GetPlayerSummaryAsync(userId);

                if (playerSummary?.Data == null)
                {
                    await new DiscordMessageBuilder()
                     .WithEmbed(EmbedHandler.CreateErrorEmbed("steam player info", "Benutzer wurde nicht gefunden"))
                     .WithReply(ctx.Message.Id, true)
                     .SendAsync(ctx.Channel);

                    return;
                }

                List<DiscordField>? fields = new()
                {                        
                    new DiscordField("Benutzer Status", playerSummary.Data.UserStatus.ToString()),
                    new DiscordField("Bei Steam seit",playerSummary.Data.AccountCreatedDate.ToString())
                };

                if(string.IsNullOrWhiteSpace(playerSummary.Data.PlayingGameName) == false)
                {
                    fields.Add(new DiscordField("Aktuelles Spiel", playerSummary.Data.PlayingGameName));
                }

                await new DiscordMessageBuilder()
                  .WithEmbed(EmbedHandler.CreateBasicEmbed("Spieler " + playerSummary.Data.Nickname, $"Informationen zum Benutzer", DiscordColor.Blue, fields, url: playerSummary.Data.ProfileUrl, imageUrl: playerSummary.Data.AvatarFullUrl))
                  .WithReply(ctx.Message.Id, true)
                  .SendAsync(ctx.Channel);
          
            }
        }
    }
}
