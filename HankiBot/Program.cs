using Discord;
using Discord.Commands;
using Discord.WebSocket;
using HankiBot.Services;
using Microsoft.Extensions.DependencyInjection;

namespace HankiBot;

public class Program
{
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

        DiscordSocketClient client = services.GetRequiredService<DiscordSocketClient>();

        client.Log += LogAsync;
        services.GetRequiredService<CommandService>().Log += LogAsync;
        
        await client.LoginAsync(TokenType.Bot, Globals.Token);
        await client.StartAsync();

        await client.SetCustomStatusAsync($"With the rubber rats | {Globals.CommandPrefix}h");

        // Here we initialize the logic required to register our commands.
        await services.GetRequiredService<CommandHandlingService>().InitializeAsync();

        // Initialize Twitch service for now live notifications
        services.GetRequiredService<TwitchService>();

        await Task.Delay(Timeout.Infinite);
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
            .AddSingleton<TwitchService>()
            .AddSingleton<HttpClient>()
            .BuildServiceProvider();
    }

    private Task LogAsync(LogMessage log)
    {
        Console.WriteLine(log.ToString());
        return Task.CompletedTask;
    }
}