using CodeHollow.FeedReader;
using DSharpPlus.Entities;
using Rc.DiscordBot.Handlers;

namespace Rc.DiscordBot
{
    public static class RssEmbedHelper
    {
        public static DiscordEmbed CreateEmbed(string feedName, FeedItem item)
        {
            return new DiscordEmbedBuilder()
                               .WithTitle($"RSS - {feedName}: {item.Title}")
                               .WithCustomDescription(item.Description)
                               .WithUrl(item.Link)
                               .WithAuthor(item.Author)
                               .WithColor(DiscordColor.Blue)
                               .WithBotFooter()
                               .Build();
        }
    }
}
