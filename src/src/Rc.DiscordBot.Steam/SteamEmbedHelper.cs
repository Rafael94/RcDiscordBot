
using DSharpPlus.Entities;
using Rc.DiscordBot.Extensions;
using Rc.DiscordBot.Handlers;
using Steam.Models;
using Steam.Models.SteamStore;

namespace Rc.DiscordBot
{
    public static class SteamEmbedHelper
    {
        public static DiscordEmbed CreateEmbed(StoreAppDetailsDataModel app, NewsItemModel item, System.DateTimeOffset date)
        {
            return new DiscordEmbedBuilder()
                               .WithTitle($"Game News - {app.Name}: {item.Title}")
                               .WithImageUrl(app.HeaderImage)
                               .WithTimestamp(date)
                               .WithCustomDescription(item.Contents)
                               //.WithUrl(item.Url)
                               .WithAuthor(item.Author)
                               .WithColor(DiscordColor.Blue)
                               .WithBotFooter()
                               .AddField("Feed Name", item.Feedname, true)
                               .AddField("Feed Label", item.FeedLabel, true)
                               .AddField(item.Tags.Length > 0, "Tags", () => string.Join(',', item.Tags), true)
                               .Build();
        }
    }
}
