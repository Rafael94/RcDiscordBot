using Discord;
using System;

namespace Rc.DiscordBot.Extensions
{
    public static class EmbedBuilderExtensions
    {
        public static EmbedBuilder AddField(this EmbedBuilder builder, bool condition, string name, string value, bool inline = false)
        {
            if (condition)
            {
                return builder.AddField(name, value, inline);
            }
            else
            {
                return builder;
            }
        }

        public static EmbedBuilder AddField(this EmbedBuilder builder, bool condition, string name, Func<string> value, bool inline = false)
        {
            if (condition)
            {
                return builder.AddField(name, value(), inline);
            }
            else
            {
                return builder;
            }
        }
    }
}
