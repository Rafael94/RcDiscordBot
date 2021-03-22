namespace Rc.DiscordBot.Models
{
    public record StreamConfig
    {
        public string Name { get; set; } = default!;
        public string Url { get; set; } = default!;
        public string? DisplayName { get; set; }
        public string? Volume { get; set; }
        public StreamNormalization Normalization { get; set; }
    }
}
