using CodeHollow.FeedReader;
using Discord;
using Discord.Commands;
using Microsoft.Extensions.Options;
using Rc.DiscordBot.Handlers;
using Rc.DiscordBot.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rc.DiscordBot.Modules
{
    [Name("Rss Feed")]
    [Group("Rss")]
    public class RssFeedModule : ModuleBase<SocketCommandContext>
    {
        private readonly RssConfig _rssConfig;

        public RssFeedModule(IOptions<RssConfig> botConfig)
        {
            _rssConfig = botConfig.Value;
        }

        [Command("list")]
        public async Task GetFeedListAsync()
        {
            List<EmbedFieldBuilder>? fileds = new();

            for (int i = 0; i < _rssConfig.Feeds.Count; i++)
            {
                Models.Feed? feedConfig = _rssConfig.Feeds[i];

                MessageSendToDiscordServer? discordServer = feedConfig.DiscordServers.Where(x => string.Equals(x.Name, Context.Guild.Name, System.StringComparison.OrdinalIgnoreCase)).FirstOrDefault();

                fileds.Add(
                    new EmbedFieldBuilder()
                    .WithIsInline(false)
                    .WithName($"{feedConfig.Name}  { (discordServer == null ? "" : " - " + discordServer.Channel + "")}")
                    .WithValue(feedConfig.Url)
                    );
            }

            await ReplyAsync(embed: await EmbedHandler.CreateBasicEmbed("Feeds", $"Hinterlegte RSS Feeds", Color.Blue, fileds));
        }

        [Command("read")]
        public async Task ReadAsync(string name, int anzahl = 1)
        {
            List<EmbedFieldBuilder>? fileds = new();

            Models.Feed? feedConfig = _rssConfig.Feeds.Where(x => string.Equals(x.Name, name, System.StringComparison.OrdinalIgnoreCase)).FirstOrDefault();

            if (feedConfig == null)
            {
                await ReplyAsync(embed: await EmbedHandler.CreateErrorEmbed("RSS Red", "Feed not found"));
                return;
            }

            CodeHollow.FeedReader.Feed? feedList = await FeedReader.ReadAsync(feedConfig.Url);

            int i = 0;
            foreach (FeedItem? feedItem in feedList.Items)
            {
                if (i >= anzahl || i >= feedList.Items.Count)
                {
                    break;
                }

                await ReplyAsync(embed: RssEmbedHelper.CreateEmbed(feedConfig.Name, feedItem));

                ++i;
            }
        }

    }
}
