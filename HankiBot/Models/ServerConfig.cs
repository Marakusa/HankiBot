﻿namespace HankiBot.Models
{
    public class ServerConfig
    {
        public string? ServerId { get; set; }
        public string? TwitchNotificationChannel { get; set; }
        public List<string>? TwitchChannels { get; set; }
        public string? ConfigChannel { get; set; }
    }
}
