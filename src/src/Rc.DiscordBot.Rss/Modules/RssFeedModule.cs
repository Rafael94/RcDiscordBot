using Discord;
using Discord.Commands;
using Microsoft.Extensions.Options;
using Rc.DiscordBot.Handlers;
using Rc.DiscordBot.Models;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
        public async Task GetVersionAsync()
        {
            List<EmbedFieldBuilder>? fileds = new();

            for (var i = 0; i < _rssConfig.Feeds.Count; i++)
            {
                var feedConfig = _rssConfig.Feeds[i];

                var discordServer = feedConfig.DiscordServers.Where(x => string.Equals(x.Name, Context.Guild.Name, System.StringComparison.OrdinalIgnoreCase)).FirstOrDefault();

                fileds.Add(
                    new EmbedFieldBuilder()
                    .WithIsInline(false)
                    .WithName($"{feedConfig.Name}  { (discordServer == null? "" : " - "  + discordServer.Channel + "")}")
                    .WithValue(feedConfig.Url)
                    );
            }

            await ReplyAsync(embed: await EmbedHandler.CreateBasicEmbed("Feeds", $"Hinterlegte RSS Feeds", Color.Blue, fileds));
        }

    }
}
