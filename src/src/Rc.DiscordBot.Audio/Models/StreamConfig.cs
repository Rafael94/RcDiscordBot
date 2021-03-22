namespace Rc.DiscordBot.Models
{
    public record StreamConfig
    {
        public string? Name { get; set; }
        public string Url { get; set; } = default!;
        public string? Volume { get; set; }
        public StreamNormalization Normalization { get; set; }
    }
}
