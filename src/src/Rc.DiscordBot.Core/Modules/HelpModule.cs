using Discord;
using Discord.Commands;
using Microsoft.Extensions.Options;
using Rc.DiscordBot.Models;
using System.Linq;
using System.Threading.Tasks;

namespace Rc.DiscordBot.Modules
{
    [Name("Help")]
    public class HelpModule : ModuleBase<SocketCommandContext>
    {
        private readonly CommandService _service;
        private readonly BotConfig _botConfig;

        public HelpModule(CommandService service, IOptions<BotConfig> botConfig)
        {
            _service = service;
            _botConfig = botConfig.Value;
        }


        [Command("help")]
        public async Task HelpAsync()
        {
            string prefix = _botConfig.Prefix;
            var builder = new EmbedBuilder()
            {
                Color = new Color(114, 137, 218),
                Description = "Verfügbare Kommandos. Befehle zwischen {} sind Aliases. Parameter mit ? am Ende sind optional"
            };

            foreach (var module in _service.Modules)
            {
                string? description = null;
                foreach (var cmd in module.Commands)
                {
                    var result = await cmd.CheckPreconditionsAsync(Context);
                    if (result.IsSuccess)
                    {
                        description += $"{prefix}{cmd.Aliases[0]}";

                        if (cmd.Parameters.Count > 0)
                        {
                            description += ' ' + string.Join(' ', cmd.Parameters.Select(x => $"{x.Name}{(x.IsOptional ? '?' : "")}:{x.Type.Name}").ToList());
                        }

                        if (cmd.Aliases.Count > 1)
                        {
                            description += " | {" + string.Join(',', cmd.Aliases[^1]) + '}';
                        }

                        description += '\n';
                    }

                }

                if (!string.IsNullOrWhiteSpace(description))
                {
                    builder.AddField(x =>
                    {
                        x.Name = module.Name;
                        x.Value = description;
                        x.IsInline = false;
                    });
                }
            }

            await ReplyAsync("", false, builder.Build());
        }

        [Command("help")]
        public async Task HelpAsync(string command)
        {
            var result = _service.Search(Context, command);

            if (!result.IsSuccess)
            {
                await ReplyAsync($"Sorry, I couldn't find a command like **{command}**.");
                return;
            }

            string prefix = _botConfig.Prefix;
            var builder = new EmbedBuilder()
            {
                Color = new Color(114, 137, 218),
                Description = $"Here are some commands like **{command}**"
            };

            foreach (var match in result.Commands)
            {
                var cmd = match.Command;

                builder.AddField(x =>
                {
                    x.Name = string.Join(", ", cmd.Aliases);

                    if (string.IsNullOrEmpty(cmd.Summary) == false)
                    {
                        x.Value = $"Summary: {cmd.Summary}\n\n";
                    }

                    x.Value += $"Befehl: {prefix}{cmd.Aliases[0]} {string.Join(' ', cmd.Parameters.Select(x => $"{x.Name}{(x.IsOptional ? '?' : "")}").ToList())}\n\n";

                    x.Value += $"Parameters:\n";

                    foreach (var para in cmd.Parameters)
                    {
                        x.Value += $"{para.Name}{(para.IsOptional ? '?' : "")}:{para.Type.Name} {para.Summary}\n";
                    }

                    x.IsInline = false;
                });
            }

            await ReplyAsync("", false, builder.Build());
        }
    }
}
