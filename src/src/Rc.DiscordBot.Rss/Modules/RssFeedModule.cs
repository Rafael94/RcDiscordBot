using CodeHollow.FeedReader;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Microsoft.Extensions.Options;
using Rc.DiscordBot.Handlers;
using Rc.DiscordBot.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rc.DiscordBot.Modules
{
    
    [Group("rss")]
    public class RssFeedModule : BaseCommandModule
    {
        private readonly RssConfig _rssConfig;

        public RssFeedModule(IOptions<RssConfig> botConfig)
        {
            _rssConfig = botConfig.Value;
        }

        [Command("list")]
        [Description("Listet die hinterlegten RSS Feeds auf")]
        public async Task GetFeedListAsync(CommandContext ctx)
        {
            List<DiscordField>? fileds = new();

            for (int i = 0; i < _rssConfig.Feeds.Count; i++)
            {
                Models.Feed? feedConfig = _rssConfig.Feeds[i];

                MessageSendToDiscordServer? discordServer = feedConfig.DiscordServers.Where(x => string.Equals(x.Name ,ctx.Guild.Name, System.StringComparison.OrdinalIgnoreCase)).FirstOrDefault();

                fileds.Add(
                    new DiscordField($"{feedConfig.Name}  { (discordServer == null ? "" : " - " + discordServer.Channel + "")}", feedConfig.Url, false));
            }

            await new DiscordMessageBuilder()
                 .WithEmbed(EmbedHandler.CreateBasicEmbed("Feeds", $"Hinterlegte RSS Feeds", DiscordColor.Green, fileds))
                 .WithReply(ctx.Message.Id, true)
                 .SendAsync(ctx.Channel);
        }

        [Command("read")]
        public async Task ReadAsync(CommandContext ctx, string name, int anzahl = 1)
        {
            List<DiscordField>? fileds = new();

            Models.Feed? feedConfig = _rssConfig.Feeds.Where(x => string.Equals(x.Name, name, System.StringComparison.OrdinalIgnoreCase)).FirstOrDefault();

            if (feedConfig == null)
            {
                await new DiscordMessageBuilder()
                 .WithEmbed(EmbedHandler.CreateErrorEmbed("RSS Red", "Feed not found"))
                 .WithReply(ctx.Message.Id, true)
                 .SendAsync(ctx.Channel);

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

                await new DiscordMessageBuilder()
                    .WithEmbed(RssEmbedHelper.CreateEmbed(feedConfig.Name, feedItem))
                    .WithReply(ctx.Message.Id, true)
                    .SendAsync(ctx.Channel);

                ++i;
            }
        }
    }
}
