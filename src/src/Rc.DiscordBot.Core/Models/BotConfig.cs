using System.Collections.Generic;


namespace Rc.DiscordBot.Models
{
    public record BotConfig
    {
        public BotConfig()
        {
            BlacklistedChannels = new List<ulong>();
        }

        public string BotToken { get; set; } = default!;
        public string Prefix { get; set; } = "!";
        public string GameStatus { get; set; } = default!;
        public List<ulong> BlacklistedChannels { get; set; }


    }
}
