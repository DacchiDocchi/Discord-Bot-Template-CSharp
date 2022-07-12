using System;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Discord.Commands;
using Discord.WebSocket;
using Discord;
using System.Linq;
using Newtonsoft.Json;

namespace DiscordBotCsharp.Services
{
    public class CommandHandler
    {
        private readonly CommandService _commands;
        private readonly DiscordSocketClient _client;
        private readonly IServiceProvider _services;

        public CommandHandler(IServiceProvider services)
        {
            _commands = services.GetRequiredService<CommandService>();
            _client = services.GetRequiredService<DiscordSocketClient>();
            _services = services;

            _client.Ready += ClientReadyAsync;
            _client.MessageReceived += HandleCommandAsync;
        }

        private async Task HandleCommandAsync(SocketMessage socketMessage)
        {
            if (socketMessage.Author.IsBot || !(socketMessage is SocketUserMessage message) ||
                message.Channel is IDMChannel)
                return;

            var context = new SocketCommandContext(_client, message);
            var prefix = JsonConvert.DeserializeObject<string[]>(Program.Config()["prefix"].ToString());
            var argPos = 0;

            if (prefix.Any(x => message.HasStringPrefix(x, ref argPos)) ||
                message.HasMentionPrefix(_client.CurrentUser, ref argPos))
            {
                var result = await _commands.ExecuteAsync(context, argPos, _services);

                if (!result.IsSuccess && result.Error.HasValue)
                {
                    EmbedBuilder embed = new EmbedBuilder()
                        .WithColor(Color.Red)
                        .WithDescription($":warning: {result.ErrorReason}");
                    await context.Channel.SendMessageAsync(embed: embed.Build());
                }
            }
        }

        private async Task ClientReadyAsync()
        {
            await Program.SetBotStatus(_client);
        }

        public async Task InitializeAsync()
        {
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
        }
    }
}