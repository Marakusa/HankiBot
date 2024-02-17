using Discord;
using Discord.Commands;
using Discord.WebSocket;
using HankiBot.Models;
using Newtonsoft.Json;
using System.Threading.Channels;

namespace HankiBot.Commands;

[CommandGroup("Config")]
[Ignore]
public class ConfigModule : ModuleBase<SocketCommandContext>
{
    [Command("config twitter channel")]
    public async Task NotificationsChannelAsync(
        [Summary("The channel Twitch notifications are sent in")] SocketGuildChannel channel)
    {
        if (!ValidateCommand()) return;

        ServerConfig? config = Configs.GetServerConfig(Context.Guild.Id);
        config!.TwitterNotificationChannel = channel.Id.ToString();
        Configs.Save(config);

        await ReplyAsync(
            $"Tweet notifications are now sent to the channel <#{channel.Id}>`");
    }

    private bool ValidateCommand()
    {
        bool owner = Context.Guild.OwnerId == Context.User.Id;
        string configChannel = Configs.GetServerConfig(Context.Guild.Id)!.ConfigChannel!;
        return owner && Context.Channel.Id.ToString() == configChannel;
    }
}