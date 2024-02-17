using Discord.WebSocket;
using HankiBot.Models;
using Newtonsoft.Json;

namespace HankiBot;

public static class Configs
{
    private static Dictionary<string, ServerConfig>? _serverConfigs;

    public static void AddServer(ServerConfig serverConfig)
    {
        CheckConfigs();

        if (_serverConfigs != null && _serverConfigs.ContainsKey(serverConfig.ServerId!))
            return;

        Console.WriteLine($"Adding server {serverConfig.ServerId}");

        _serverConfigs?.Add(serverConfig.ServerId!, serverConfig);

        SaveConfigs();
    }

    public static void AddServer(SocketGuild serverConfig)
    {
        ServerConfig config = new()
        {
            ServerId = serverConfig.Id.ToString(),
            TwitterNotificationChannel = "",
            TwitchChannels = new List<string>()
        };
        AddServer(config);
    }

    public static ServerConfig? GetServerConfig(ulong id)
    {
        CheckConfigs();
        return _serverConfigs?[id.ToString()] ?? null;
    }

    public static List<ServerConfig> GetAllServerConfig()
    {
        CheckConfigs();

        var configs = new List<ServerConfig>();

        _serverConfigs ??= new Dictionary<string, ServerConfig>();
        foreach ((string? _, ServerConfig? c) in _serverConfigs)
            configs.Add(c);

        return configs;
    }

    private static void SaveConfigs()
    {
        var configs = new List<ServerConfig>();

        _serverConfigs ??= new Dictionary<string, ServerConfig>();
        foreach ((string? _, ServerConfig? c) in _serverConfigs)
            configs.Add(c);

        File.WriteAllText("servers.json", JsonConvert.SerializeObject(configs));
    }

    private static void CheckConfigs()
    {
        _serverConfigs ??= new Dictionary<string, ServerConfig>();

        ServerConfig[]? configs = null;

        if (File.Exists("servers.json"))
            configs = JsonConvert.DeserializeObject<ServerConfig[]>(File.ReadAllText("servers.json"));

        _serverConfigs = new Dictionary<string, ServerConfig>();

        if (configs == null)
        {
            File.WriteAllText("servers.json", "[]");
            return;
        }

        foreach (ServerConfig config in configs)
        {
            _serverConfigs.Add(config.ServerId!, config);
        }
    }

    public static void Save(ServerConfig config)
    {
        CheckConfigs();

        _serverConfigs ??= new Dictionary<string, ServerConfig>();
        if (!_serverConfigs.ContainsKey(config.ServerId!))
            _serverConfigs.Add(config.ServerId!, config);
        _serverConfigs[config.ServerId!] = config;

        SaveConfigs();
    }
}