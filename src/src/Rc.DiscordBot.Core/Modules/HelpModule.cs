using Discord;
using Discord.Commands;
using Microsoft.Extensions.Options;
using Rc.DiscordBot.Handlers;
using Rc.DiscordBot.Models;
using System.Linq;
using System.Reflection;
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

        [Command("Version")]
        public async Task GetVersionAsync()
        {
            await ReplyAsync(embed: await EmbedHandler.CreateBasicEmbed("Version", Assembly.GetEntryAssembly()!.GetName().Version!.ToString(), Color.Blue));
        }

        [Command("help")]
        public async Task HelpAsync()
        {
            string prefix = _botConfig.Prefix;
            EmbedBuilder? builder = new()
            {
                Color = new Color(114, 137, 218),
                Description = "Verfügbare Kommandos. Befehle zwischen {} sind Aliases. Parameter mit ? am Ende sind optional. Parameter sind Fanken mit % an"
            };

            foreach (ModuleInfo? module in _service.Modules)
            {
                string? description = null;
                foreach (CommandInfo? cmd in module.Commands)
                {
                    PreconditionResult? result = await cmd.CheckPreconditionsAsync(Context);
                    if (result.IsSuccess)
                    {
                        description += $"{prefix}{cmd.Aliases[0]}";

                        if (cmd.Parameters.Count > 0)
                        {
                            description += ' ' + string.Join(' ', cmd.Parameters.Select(x => $"%{x.Name}{(x.IsOptional ? '?' : "")}").ToList());
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
            SearchResult result = _service.Search(Context, command);

            if (!result.IsSuccess)
            {
                await ReplyAsync($"Sorry, I couldn't find a command like **{command}**.");
                return;
            }

            string prefix = _botConfig.Prefix;
            EmbedBuilder? builder = new()
            {
                Color = new Color(114, 137, 218),
                Description = $"Here are some commands like **{command}**"
            };

            foreach (CommandMatch match in result.Commands)
            {
                CommandInfo? cmd = match.Command;

                builder.AddField(x =>
                {
                    x.Name = string.Join(", ", cmd.Aliases);

                    if (string.IsNullOrEmpty(cmd.Summary) == false)
                    {
                        x.Value = $"Summary: {cmd.Summary}\n\n";
                    }

                    x.Value += $"Befehl: {prefix}{cmd.Aliases[0]} {string.Join(' ', cmd.Parameters.Select(x => $"{x.Name}{(x.IsOptional ? '?' : "")}").ToList())}\n\n";

                    x.Value += $"Parameters:\n";

                    foreach (Discord.Commands.ParameterInfo? para in cmd.Parameters)
                    {
                        x.Value += $"{para.Name}{(para.IsOptional ? '?' : "")} {para.Summary}\n";
                    }

                    x.IsInline = false;
                });
            }

            await ReplyAsync("", false, builder.Build());
        }
    }
}
