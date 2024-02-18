using System.Globalization;
using Discord.WebSocket;
using HankiBot.Models;
using Newtonsoft.Json;

namespace HankiBot;

public static class Configs
{
    private static Dictionary<string, ServerConfig>? _serverConfigs;
    private static GeneralConfig? _generalConfig;

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
            DailyFoxChannel = "",
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

    public static DateTime GetNextFoxTime()
    {
        CheckConfigs();
        return DateTime.Parse(_generalConfig?.NextFoxTime!, CultureInfo.InvariantCulture);
    }

    public static void SetNextFoxTime(DateTime time)
    {
        _generalConfig ??= new GeneralConfig
        {
            NextFoxTime = DateTime.Parse("1970-01-01T00:00:00.000Z").ToString("O", CultureInfo.InvariantCulture)
        };
        _generalConfig.NextFoxTime = time.ToString("O", CultureInfo.InvariantCulture);
        SaveConfigs();
    }

    private static void SaveConfigs()
    {
        var configs = new List<ServerConfig>();

        _serverConfigs ??= new Dictionary<string, ServerConfig>();
        foreach ((string? _, ServerConfig? c) in _serverConfigs)
            configs.Add(c);

        File.WriteAllText("servers.json", JsonConvert.SerializeObject(configs));
        File.WriteAllText("generalConfigs.json", JsonConvert.SerializeObject(_generalConfig));
    }

    private static void CheckConfigs()
    {
        _serverConfigs ??= new Dictionary<string, ServerConfig>();
        _generalConfig ??= null;

        ServerConfig[]? configs = null;

        if (File.Exists("servers.json"))
            configs = JsonConvert.DeserializeObject<ServerConfig[]>(File.ReadAllText("servers.json"));

        if (File.Exists("generalConfigs.json"))
            _generalConfig = JsonConvert.DeserializeObject<GeneralConfig>(File.ReadAllText("generalConfigs.json"));

        _serverConfigs = new Dictionary<string, ServerConfig>();

        if (configs == null)
        {
            File.WriteAllText("servers.json", "[]");
            return;
        }

        if (_generalConfig == null)
        {
            _generalConfig = new GeneralConfig
            {
                NextFoxTime = DateTime.Parse("1970-01-01T00:00:00.000Z").ToString("O", CultureInfo.InvariantCulture)
            };
            File.WriteAllText("generalConfigs.json", JsonConvert.SerializeObject(_generalConfig));
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