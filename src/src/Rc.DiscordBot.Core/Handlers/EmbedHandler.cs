using Discord;
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
            var builder = new EmbedBuilder()
                .WithTitle(title)
                .WithDescription(description)
                .WithColor(color)
                .WithCurrentTimestamp();

            if (fileds != null)
            {
                builder = builder.WithFields(fileds);
            }

            if(author != null)
            {
                builder.WithAuthor(author);
            }

            return Task.FromResult(builder.Build());
        }

        public static async Task<Embed> CreateErrorEmbed(string source, string error)
        {
            var embed = await Task.Run(() => new EmbedBuilder()
                .WithTitle($"ERROR OCCURED FROM - {source}")
                .WithDescription($"**Error Deaitls**: \n{error}")
                .WithColor(Color.DarkRed)
                .WithCurrentTimestamp().Build());
            return embed;
        }
    }
}
