

namespace Rc.DiscordBot.Models
{
    public record MessageSendToDiscordServer
    {
        public string Name { get; set; } = default!;

        public string Channel { get; set; } = default!;
    }
}
