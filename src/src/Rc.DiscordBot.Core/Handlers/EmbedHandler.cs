using Discord;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Rc.DiscordBot.Handlers
{
    public static class EmbedHandler
    {
        /* This file is where we can store all the Embed Helper Tasks (So to speak). 
             We wrap all the creations of new EmbedBuilder's in a Task.Run to allow us to stick with Async calls. 
             All the Tasks here are also static which means we can call them from anywhere in our program. */
        public static Task<Embed> CreateBasicEmbed(string title,
            string description,
            Color color,
            IEnumerable<EmbedFieldBuilder>? fileds = null,
            EmbedAuthorBuilder? author = null)
        {
            EmbedBuilder? builder = new EmbedBuilder()
                .WithTitle(title)
                .WithCustomDescription(description)
                .WithColor(color)
                .WithCurrentTimestamp()
                .WithBotFooter();

            if (fileds != null)
            {
                builder = builder.WithFields(fileds);
            }

            if (author != null)
            {
                builder.WithAuthor(author);
            }

            return Task.FromResult(builder.Build());
        }

        public static EmbedBuilder WithBotFooter(this EmbedBuilder embedBuilder)
        {
            return embedBuilder.WithFooter("Bot von Rafael Carnucci. https://twitch.tv/vincitorede");
        }

        /// <summary>
        /// Kürzt automatisch zu lange Beschreibungen
        /// Ersetzt <br> durch Zeilenumbrüche
        /// </summary>
        /// <param name="embedBuilder"></param>
        /// <param name="description"></param>
        /// <returns></returns>
        public static EmbedBuilder WithCustomDescription(this EmbedBuilder embedBuilder, string description)
        {
            if (description == null)
            {
                return embedBuilder;
            }

            description = description.Replace("<br>", Environment.NewLine, StringComparison.OrdinalIgnoreCase).Replace("<br/>", Environment.NewLine, StringComparison.OrdinalIgnoreCase);

            if (description.Length > 2048)
            {
                description = description[0..2044] + "...";
            }

            return embedBuilder.WithDescription(description);
        }

        public static async Task<Embed> CreateErrorEmbed(string source, string error)
        {
            Embed? embed = await Task.Run(() => new EmbedBuilder()
                .WithTitle($"ERROR OCCURED FROM - {source}")
                .WithDescription($"**Error Deaitls**: \n{error}")
                .WithColor(Color.DarkRed)
                .WithCurrentTimestamp().Build());
            return embed;
        }
    }
}
