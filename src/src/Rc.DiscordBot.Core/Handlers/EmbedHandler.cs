
using DSharpPlus.Entities;
using Rc.DiscordBot.Models;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Rc.DiscordBot.Handlers
{
    public static class EmbedHandler
    {
        /* This file is where we can store all the Embed Helper Tasks (So to speak). 
             We wrap all the creations of new EmbedBuilder's in a Task.Run to allow us to stick with Async calls. 
             All the Tasks here are also static which means we can call them from anywhere in our program. */
        public static Task<DiscordEmbed> CreateBasicEmbed(string title,
            string description,
            DiscordColor color,
            IEnumerable<DiscordField>? fields = null,
            DiscordAuthor? author = null,
            string? url = null,
            string? imageUrl = null)
        {
           
            var builder = new DiscordEmbedBuilder()
                .WithTitle(title)
                .WithCustomDescription(description)
                .WithColor(color)
                .WithCurrentTimestamp()
                .WithBotFooter();

            if (fields != null)
            {
                builder = builder.AddFields(fields);
            }

            if (author != null)
            {
                builder.WithAuthor(author);
            }

            if(string.IsNullOrWhiteSpace(imageUrl) == false)
            {
                builder.WithImageUrl(imageUrl);
            }

            if (string.IsNullOrWhiteSpace(url) == false)
            {
                builder.WithUrl(url);
            }

            return Task.FromResult(builder.Build());
        }

        public static DiscordEmbedBuilder WithBotFooter(this DiscordEmbedBuilder embedBuilder)
        {
            return embedBuilder.WithFooter("Bot von Rafael Carnucci. https://github.com/Rafael94/DiscordBot");
        }

        public static DiscordEmbedBuilder WithAuthor(this DiscordEmbedBuilder embedBuilder, DiscordAuthor discordAuthor)
        {
            return embedBuilder.WithAuthor(discordAuthor.Name, discordAuthor.Url, discordAuthor.IconUrl);
        }

        public static DiscordEmbedBuilder AddField(this DiscordEmbedBuilder embedBuilder, DiscordField field)
        {
            return embedBuilder.AddField(field.Name, field.Value, field.Inline);
        }

        public static DiscordEmbedBuilder AddFields(this DiscordEmbedBuilder embedBuilder, IEnumerable< DiscordField> fields)
        {
            foreach(var field in fields)
            {
                embedBuilder.AddField(field.Name, field.Value, field.Inline);
            }

            return embedBuilder;
        }

        /// <summary>
        /// Kürzt automatisch zu lange Beschreibungen
        /// Ersetzt <br> durch Zeilenumbrüche
        /// </summary>
        /// <param name="embedBuilder"></param>
        /// <param name="description"></param>
        /// <returns></returns>
        public static DiscordEmbedBuilder WithCustomDescription(this DiscordEmbedBuilder embedBuilder, string description)
        {
            if (description == null)
            {
                return embedBuilder;
            }

            description = description.Replace("<br>", Environment.NewLine, StringComparison.OrdinalIgnoreCase)
                .Replace("<br/>", Environment.NewLine, StringComparison.OrdinalIgnoreCase)
                .Replace("<li>", "", StringComparison.OrdinalIgnoreCase)
                .Replace("<li/>", Environment.NewLine, StringComparison.OrdinalIgnoreCase);

            description = Regex.Replace(description, "<.*?>", string.Empty);

            if (description.Length > 2048)
            {
                description = description[0..2044] + "...";
            }

            return embedBuilder.WithDescription(description);
        }

        public static async Task<DiscordEmbed> CreateErrorEmbed(string source, string error)
        {
            DiscordEmbed? embed = await Task.Run(() => new DiscordEmbedBuilder()
                .WithTitle($"ERROR OCCURED FROM - {source}")
                .WithDescription($"**Error Deaitls**: \n{error}")
                .WithColor(DiscordColor.DarkRed)
                .WithCurrentTimestamp().Build());
            return embed;
        }

        public static DiscordEmbedBuilder WithCurrentTimestamp(this DiscordEmbedBuilder embedBuilder)
        {
            return embedBuilder.WithTimestamp(DateTimeOffset.Now);
        }
    }
}
