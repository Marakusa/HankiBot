using Discord;
using Discord.Commands;
using Discord.WebSocket;
using HankiBot.Models;
using HankiBot.Services;
using Microsoft.Extensions.DependencyInjection;
using Discord.Rest;

namespace HankiBot;

public class Program
{
    private DiscordSocketClient? _client;
    
    private static void Main(string[] args)
    {
        new Program()
            .MainAsync()
            .GetAwaiter()
            .GetResult();
    }

    public async Task MainAsync()
    {
        await using ServiceProvider services = ConfigureServices();

        _client = services.GetRequiredService<DiscordSocketClient>();

        _client.Connected += Connected;
        _client.Log += LogAsync;
        _client.JoinedGuild += GuildJoined;
        _client.ChannelDestroyed += ChannelDeleted;
        services.GetRequiredService<CommandService>().Log += LogAsync;
        
        await _client.LoginAsync(TokenType.Bot, Globals.Token);
        await _client.StartAsync();

        await _client.SetCustomStatusAsync($"With the rubber rats | {Globals.CommandPrefix}h");

        // Here we initialize the logic required to register our commands.
        await services.GetRequiredService<CommandHandlingService>().InitializeAsync();
        
        services.GetRequiredService<DailyFoxService>();
        
        await Task.Delay(Timeout.Infinite);
    }

    private async Task Connected()
    {
        if (_client?.Guilds == null) return;
        foreach (SocketGuild guild in _client.Guilds)
        {
            Configs.AddServer(guild);
            ServerConfig? config = Configs.GetServerConfig(guild.Id);
            await InitServerAsync(config!, guild);
        }
    }

    private static async Task InitServerAsync(ServerConfig config, SocketGuild guild)
    {
        while (!guild.IsConnected) await Task.Delay(1000);

        SocketGuildChannel? configChannel = guild.TextChannels.FirstOrDefault(f => f.Id.ToString() == config.ConfigChannel);
        if (configChannel == null)
        {
            RestTextChannel? channel = await guild.CreateTextChannelAsync("hanki-bot-config");
            await channel.AddPermissionOverwriteAsync(guild.EveryoneRole, OverwritePermissions.DenyAll(channel));
            config.ConfigChannel = channel.Id.ToString();
        }
        else
        {
            config.ConfigChannel = configChannel.Id.ToString();
        }

        Configs.Save(config);
    }

    private static async Task GuildJoined(SocketGuild arg)
    {
        Configs.AddServer(arg);
        ServerConfig? config = Configs.GetServerConfig(arg.Id);
        await InitServerAsync(config!, arg);
    }

    private async Task ChannelDeleted(SocketChannel arg)
    {
        foreach (SocketGuild guild in _client?.Guilds ?? new List<SocketGuild>())
        {
            Configs.AddServer(guild);
            ServerConfig? config = Configs.GetServerConfig(guild.Id);
            await InitServerAsync(config!, guild);
        }
    }

    private static ServiceProvider ConfigureServices()
    {
        return new ServiceCollection()
            .AddSingleton(new DiscordSocketConfig
            {
                GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent
            })
            .AddSingleton<DiscordSocketClient>()
            .AddSingleton<CommandService>()
            .AddSingleton<CommandHandlingService>()
            //.AddSingleton<TwitterService>()
            .AddSingleton<DailyFoxService>()
            .AddSingleton<HttpClient>()
            .BuildServiceProvider();
    }

    private Task LogAsync(LogMessage log)
    {
        Console.WriteLine(log.ToString());
        return Task.CompletedTask;
    }
}