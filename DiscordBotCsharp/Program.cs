using System;
using System.IO;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Discord.Commands;
using DiscordBotCsharp.Services;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DiscordBotCsharp
{
    public static class Program
    {
        private static void Main(string[] args)
        {
            MainAsync().GetAwaiter().GetResult();
        }

        private static async Task MainAsync()
        {
            await using var services = ConfigureServices();

            var client = services.GetRequiredService<DiscordSocketClient>();

            client.Log += ConsoleLogs;
            services.GetRequiredService<CommandService>().Log += ConsoleLogs;

            var token = Config()["token"].Value<string>();

            await client.LoginAsync(TokenType.Bot, token);
            await client.StartAsync();

            await services.GetRequiredService<CommandHandler>().InitializeAsync();

            await Task.Delay(-1);
        }

        private static ServiceProvider ConfigureServices()
        {
            return new ServiceCollection()
                .AddSingleton(new DiscordSocketClient(new DiscordSocketConfig
                {
                    MessageCacheSize = 500,
                    LogLevel = LogSeverity.Info
                }))
                .AddSingleton(new CommandService(new CommandServiceConfig
                {
                    LogLevel = LogSeverity.Info,
                    DefaultRunMode = RunMode.Async,
                    CaseSensitiveCommands = false
                }))
                .AddSingleton<CommandHandler>()
                .BuildServiceProvider();
        }

        public static async Task SetBotStatus(DiscordSocketClient client)
        {
            var statusType = Config()["playing_status_type"]?.Value<string>().ToLower();
            var statusText = Config()["playing_status_text"]?.Value<string>();
            var onlineStatus = Config()["online_bot_status"]?.Value<string>().ToLower();

            if (!string.IsNullOrEmpty(onlineStatus))
            {
                UserStatus userStatus = onlineStatus switch
                {
                    "idle" => UserStatus.Idle,
                    "invisible" => UserStatus.Invisible,
                    "offline" => UserStatus.Offline,
                    "online" => UserStatus.Online,
                    "afk" => UserStatus.AFK,
                    "dnd" => UserStatus.DoNotDisturb,
                    _ => UserStatus.Online
                };

                await client.SetStatusAsync(userStatus);
            }

            if (!string.IsNullOrEmpty(statusType) && !string.IsNullOrEmpty(statusText))
            {
                ActivityType activity = statusType switch
                {
                    "competing" => ActivityType.Competing,
                    "listening" => ActivityType.Listening,
                    "playing" => ActivityType.Playing,
                    "streaming" => ActivityType.Streaming,
                    "watching" => ActivityType.Watching,
                    "custom" => ActivityType.CustomStatus,
                    _ => ActivityType.Playing
                };

                await client.SetGameAsync(statusText, type: activity);
            }
        }

        private static Task ConsoleLogs(LogMessage logs)
        {
            Console.WriteLine(logs.ToString());
            return Task.CompletedTask;
        }

        public static JObject Config()
        {
            var configJson = new StreamReader("../../Config/Config.json");
            return (JObject) JsonConvert.DeserializeObject(configJson.ReadToEnd());
        }
    }
}