using CodeHollow.FeedReader;
using Discord;
using Rc.DiscordBot.Handlers;

namespace Rc.DiscordBot
{
    public static class RssEmbedHelper
    {
        public static Embed CreateEmbed(string feedName, FeedItem item)
        {
            return new EmbedBuilder()
                               .WithTitle($"RSS - {feedName}: {item.Title}")
                               .WithCustomDescription(item.Description)
                               .WithUrl(item.Link)
                               .WithAuthor(item.Author)
                               .WithColor(Color.Blue)
                               .WithBotFooter()
                               .Build();
        }
    }
}
