using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Rc.DiscordBot.Handlers;
using Rc.DiscordBot.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rc.DiscordBot.Modules
{
    public class BotModule
    {
        [Command("info")]
        [Aliases("about")]
        [Description("Infos über diesen Bot")]
        public async Task BotInfoAsync(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();

            var uptime = DateTime.UtcNow - Process.GetCurrentProcess().StartTime.ToUniversalTime();
            List<DiscordField> fields = new()
            {
                new(":clock1: Laufzeit", $"{uptime.TotalDays:00} days {uptime.Hours:00}:{uptime.Minutes:00}:{uptime.Seconds:00}", true),
                new(":link: Links", $"[Commands](https://github.com/Rafael94/RcDiscordBot/commands.md) **|** [GitHub](https://github.com/Rafael94/RcDiscordBot)", true),
            };

            await new DiscordMessageBuilder()
                    .WithEmbed(EmbedHandler.CreateBasicEmbed("Bot Infos", "Ein in C# geschriebener Discord Bot", DiscordColor.Black, fields, url: "https://github.com/Rafael94/RcDiscordBot"))
                    .WithReply(ctx.Message.Id, true)
                    .SendAsync(ctx.Channel);
        }

        [Command("ping")]
        [Aliases("pong")]
        public async Task PingBotAsync(CommandContext ctx)
        {
            await ctx.TriggerTypingAsync();
            await new DiscordMessageBuilder()
                   .WithContent($":ping_pong: Pong! Ping: **{ctx.Client.Ping}**ms")
                   .WithReply(ctx.Message.Id, true)
                   .SendAsync(ctx.Channel);

        }

        [RequireOwner]
        [Command("username")]
        [Aliases("setusername", "name", "setname", "nickname", "nick")]
        [Description("Set Bots's username.")]
        public async Task SetBotUsernameAsync(CommandContext ctx,
          [Description("Der neue Name für den Bot.")] [RemainingText]
            string name)
        {
            await ctx.TriggerTypingAsync();
            if (string.IsNullOrWhiteSpace(name))
            {
                await new DiscordMessageBuilder()
                    .WithEmbed(EmbedHandler.CreateErrorEmbed("Bot", $"Der Name darf nicht leer sein"))
                    .WithReply(ctx.Message.Id, true)
                    .SendAsync(ctx.Channel);
                return;
            }

            var oldName = ctx.Client.CurrentUser.Username;
            await ctx.Client.UpdateCurrentUserAsync(name);

            await new DiscordMessageBuilder()
                  .WithContent($"Der Name wurde von {oldName} zu {name} geändert")
                  .WithReply(ctx.Message.Id, true)
                  .SendAsync(ctx.Channel);
        }
    }
}
