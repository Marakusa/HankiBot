using Discord.Commands;
using Discord.WebSocket;
using HankiBot.Models;

namespace HankiBot.Commands;

[CommandGroup("Config")]
[Ignore]
public class ConfigModule : ModuleBase<SocketCommandContext>
{
    [Command("config channel twitter")]
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

    [Command("config channel twitter")]
    public async Task NotificationsChannelGetAsync()
    {
        if (!ValidateCommand()) return;

        ServerConfig? config = Configs.GetServerConfig(Context.Guild.Id);
        if (string.IsNullOrEmpty(config!.TwitterNotificationChannel))
        {
            await ReplyAsync("Tweet notifications are not sent anywhere");
            return;
        }
        await ReplyAsync($"Tweet notifications are sent to the channel <#{config!.TwitterNotificationChannel}>`");
    }

    [Command("config channel fox")]
    public async Task FoxChannelAsync(
        [Summary("The channel daily foxes are sent in")] SocketGuildChannel channel)
    {
        if (!ValidateCommand()) return;

        ServerConfig? config = Configs.GetServerConfig(Context.Guild.Id);
        config!.DailyFoxChannel = channel.Id.ToString();
        Configs.Save(config);

        await ReplyAsync(
            $"Daily foxes are now sent to the channel <#{channel.Id}>`");
    }

    [Command("config channel fox")]
    public async Task FoxChannelGetAsync()
    {
        if (!ValidateCommand()) return;

        ServerConfig? config = Configs.GetServerConfig(Context.Guild.Id);
        if (string.IsNullOrEmpty(config!.DailyFoxChannel))
        {
            await ReplyAsync("Daily foxes are not sent anywhere");
            return;
        }
        await ReplyAsync($"Daily foxes are sent to the channel <#{config!.DailyFoxChannel}>`");
    }

    [Command("config channel suggestions")]
    public async Task SuggestionsChannelAsync(
        [Summary("The channel suggestions are handled in")] SocketGuildChannel channel)
    {
        if (!ValidateCommand()) return;

        ServerConfig? config = Configs.GetServerConfig(Context.Guild.Id);
        config!.SuggestionChannel = channel.Id.ToString();
        Configs.Save(config);

        await ReplyAsync(
            $"Suggestions are now handled in the channel <#{channel.Id}>`");
    }

    [Command("config channel fox")]
    public async Task SuggestionsChannelGetAsync()
    {
        if (!ValidateCommand()) return;

        ServerConfig? config = Configs.GetServerConfig(Context.Guild.Id);
        if (string.IsNullOrEmpty(config!.SuggestionChannel))
        {
            await ReplyAsync("Suggestions are not handled on any channel");
            return;
        }
        await ReplyAsync($"Suggestions are handled in the channel <#{config!.SuggestionChannel}>`");
    }

    private bool ValidateCommand()
    {
        bool owner = Context.Guild.OwnerId == Context.User.Id;
        string configChannel = Configs.GetServerConfig(Context.Guild.Id)!.ConfigChannel!;
        return owner && Context.Channel.Id.ToString() == configChannel;
    }
}