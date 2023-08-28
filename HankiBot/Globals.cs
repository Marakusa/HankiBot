using HankiBot.Models;
using Newtonsoft.Json;

namespace HankiBot;

public static class Globals
{
    public static string? CommandPrefix => JsonConvert.DeserializeObject<Configurations>(File.ReadAllText("config.json"))?.CommandPrefix;
    public static string? Token => JsonConvert.DeserializeObject<Configurations>(File.ReadAllText("config.json"))?.Token;
    public static string[]? BonkImages => JsonConvert.DeserializeObject<Configurations>(File.ReadAllText("config.json"))?.BonkImages;
    public static string? TwitchClientId => JsonConvert.DeserializeObject<Configurations>(File.ReadAllText("config.json"))?.TwitchClientId;
    public static string? TwitchClientSecret => JsonConvert.DeserializeObject<Configurations>(File.ReadAllText("config.json"))?.TwitchClientSecret;
    public static string[]? TwitchChannels => JsonConvert.DeserializeObject<Configurations>(File.ReadAllText("config.json"))?.TwitchChannels;
    public static int? TwitchNotificationsChannel => JsonConvert.DeserializeObject<Configurations>(File.ReadAllText("config.json"))?.TwitchNotificationsChannel;
}