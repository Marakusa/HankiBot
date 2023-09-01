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
    [Command("config twitch notificationChannel")]
    public async Task NotificationsChannelAsync(
        [Summary("The channel Twitch notifications are sent in")] SocketGuildChannel channel)
    {
        if (!ValidateCommand()) return;

        ServerConfig? config = Configs.GetServerConfig(Context.Guild.Id);
        config!.TwitchNotificationChannel = channel.Id.ToString();
        Configs.Save(config);

        await ReplyAsync(
            $"Twitch live notifications are now sent to the channel <#{channel.Id}>\nConfigure the channels using the command `{Globals.CommandPrefix}config twitch channels [add|remove] [channelName]`");
    }

    [Command("config twitch channels")]
    public async Task ChannelAsync([Summary("add|remove|list")] string action,
        [Summary("The channel that is added/removed from the channel list")] string channelName)
    {
        if (!ValidateCommand()) return;

        ServerConfig? config = Configs.GetServerConfig(Context.Guild.Id);

        switch (action.ToLower())
        {
            case "add":
                if (config!.TwitchChannels!.Contains(channelName))
                {
                    await ReplyAsync($"FAILED: Channel {channelName} has already been added to the channel list");
                    return;
                }

                config!.TwitchChannels?.Add(channelName);
                await ReplyAsync($"Channel {channelName} added to the channel list");
                break;
            case "remove":
                if (!config!.TwitchChannels!.Contains(channelName))
                {
                    await ReplyAsync($"FAILED: Channel {channelName} is not in the channel list");
                    return;
                }

                config!.TwitchChannels?.Remove(channelName);
                await ReplyAsync($"Channel {channelName} removed from the channel list");
                break;
            default:
                await ReplyAsync("FAILED: Invalid action parameter. Use add|remove|list instead.");
                return;
        }

        Configs.Save(config);
    }

    [Command("config twitch channels list")]
    [Ignore]
    public async Task ChannelListAsync()
    {
        if (!ValidateCommand()) return;

        ServerConfig? config = Configs.GetServerConfig(Context.Guild.Id);

        string channels = config?.TwitchChannels?.Aggregate("", (current, channel) => current + (channel + "\n")) ?? "";

        if (channels.Length <= 0)
        {
            await ReplyAsync(
                $"No channels added to the live notifications list. Configure the channels using the command `{Globals.CommandPrefix}config twitch channels [add|remove] [channelName]`");
            return;
        }

        channels = channels[..^1];
        await ReplyAsync($"Here's the list of the current channels with live notifications enabled: \n{channels}");
    }

    private bool ValidateCommand()
    {
        bool owner = Context.Guild.OwnerId == Context.User.Id;
        string configChannel = Configs.GetServerConfig(Context.Guild.Id)!.ConfigChannel!;
        return owner && Context.Channel.Id.ToString() == configChannel;
    }
}