using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;

namespace HankiBot;

public class Program
{
    private DiscordSocketClient? _client;

    public static Task Main(string[] args) => new Program().MainAsync();

    public async Task MainAsync()
    {
        _client = new DiscordSocketClient();

        _client.Log += Log;
        
        string? token = JsonConvert.DeserializeObject<Configurations>(await File.ReadAllTextAsync("config.json"))?.Token;

        await _client.LoginAsync(TokenType.Bot, token);
        await _client.StartAsync();

        // Block this task until the program is closed.
        await Task.Delay(-1);
    }

    private Task Log(LogMessage msg)
    {
        Console.WriteLine(msg.ToString());
        return Task.CompletedTask;
    }
}