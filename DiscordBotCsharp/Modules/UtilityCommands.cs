using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace DiscordBotCsharp.Modules
{
    public class UtilityCommands : ModuleBase<SocketCommandContext>
    {
        [Command("ping")]
        [Alias("latency")]
        [Summary("Show current bot latency.")]
        public async Task Ping()
        {
            EmbedBuilder embed = new EmbedBuilder()
                .WithColor(Color.Green)
                .WithTitle("Pong!")
                .WithDescription($"Bot Latency: {Context.Client.Latency} ms.");
            await ReplyAsync(embed: embed.Build());
        }

        [Command("avatar")]
        [Alias("useravatar")]
        [Summary("Get a user's avatar.")]
        public async Task GetAvatar([Remainder] SocketGuildUser user = null)
        {
            EmbedBuilder embed = new EmbedBuilder()
                .WithColor(Color.Green)
                .WithTitle($"{(user ?? Context.User as SocketGuildUser).Username}'s avatar")
                .WithImageUrl($"{Context.User.GetAvatarUrl(size: 1024) ?? Context.User.GetDefaultAvatarUrl()}");
            await ReplyAsync(embed: embed.Build());
        }
    }
}