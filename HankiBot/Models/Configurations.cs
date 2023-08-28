namespace HankiBot.Models;

public class Configurations
{
    public string? CommandPrefix { get; set; }
    public string? Token { get; set; }
    public string[]? BonkImages { get; set; }
    public string? TwitchClientId { get; set; }
    public string? TwitchClientSecret { get; set; }
    public string[]? TwitchChannels { get; set; }
    public int? TwitchNotificationsChannel { get; set; }
}