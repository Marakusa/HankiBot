using System;
using System.Globalization;
using System.IO;
using System.Net.Http.Headers;
using Discord;
using Discord.WebSocket;
using HankiBot.Models;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Timer = System.Timers.Timer;

namespace HankiBot.Services;

public class TwitchService
{
    private readonly DiscordSocketClient _discord;

    private readonly Timer _authTimer;
    private readonly Timer _timer;
    private string? _accessToken;

    private readonly Dictionary<string, string> _streams = new();

    public TwitchService(IServiceProvider services)
    {
        Console.WriteLine("Starting Twitch service...");

        _discord = services.GetRequiredService<DiscordSocketClient>();

        _streams = new Dictionary<string, string>();

        _authTimer = new Timer(60000);
        _authTimer.Elapsed += (_, _) =>
        {
            _ = Task.Run(async () => await UpdateAuthAsync());
        };
        _authTimer.Start();

        _timer = new Timer(5000);
        _timer.Elapsed += (_, _) =>
        {
            _ = Task.Run(async () => await CheckChannelsAsync());
        };
        _timer.Start();

        Console.WriteLine("Started Twitch service!");
    }

    private async Task CheckChannelsAsync()
    {
        if (_accessToken == null) await UpdateAuthAsync();

        foreach (string channel in Globals.TwitchChannels!)
        {
            TwitchSearchChannel? channelData =
                await ApiGetRequestAsync<TwitchSearchChannel>(
                    $"https://api.twitch.tv/helix/search/channels?query={channel}");

            TwitchSearchChannelData? c = channelData?.Data.Find(f =>
                string.Equals(f.BroadcasterLogin, channel, StringComparison.CurrentCultureIgnoreCase));

            if (c == null)
            {
                continue;
            }

            TwitchStreams? streamsData =
                await ApiGetRequestAsync<TwitchStreams>(
                    $"https://api.twitch.tv/helix/streams?user_login={c.BroadcasterLogin}");
            
            if (streamsData == null || streamsData.Data.Count <= 0)
            {
                continue;
            }

            TwitchStreamsData stream = streamsData.Data[0];

            // If the stream has been mentioned before, don't notify again
            _streams.TryAdd(c.BroadcasterLogin, "");
            if (_streams[c.BroadcasterLogin] == stream.Id) return;
            _streams[c.BroadcasterLogin] = stream.Id;

            // Create an embed for Discord
            EmbedBuilder embedBuilder = new()
            {
                Title = stream.UserName,
                Description = $"[{stream.Title}](https://www.twitch.tv/{stream.UserName})",
                Url = $"https://www.twitch.tv/{stream.UserName}",
                Color = new Color(149, 99, 245),
                Fields = new List<EmbedFieldBuilder>
                {
                    new()
                    {
                        Name = "Playing:",
                        Value = stream.GameName,
                        IsInline = true
                    },
                    new()
                    {
                        Name = "Viewers:",
                        Value = stream.ViewerCount.ToString(),
                        IsInline = true
                    }
                },
                ImageUrl = $"https://static-cdn.jtvnw.net/previews-ttv/live_user_{stream.UserName}-640x360.jpg?cacheBypass={Guid.NewGuid()}",
                ThumbnailUrl = c.ThumbnailUrl
            };

            IMessageChannel? discordChannel = await _discord.GetChannelAsync((ulong) Globals.TwitchNotificationsChannel!) as IMessageChannel;
            await discordChannel?.SendMessageAsync(embed: embedBuilder.Build())!;
        }
    }

    private async Task<T?> ApiGetRequestAsync<T>(string url)
    {
        HttpClient client = new();
        HttpRequestMessage request = new(HttpMethod.Get, url);
        request.Headers.Add("client-id", Globals.TwitchClientId ?? "");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
        
        HttpResponseMessage response = await client.SendAsync(request);
        string content = await response.Content.ReadAsStringAsync();
        
        if (response.IsSuccessStatusCode)
        {
            return JsonConvert.DeserializeObject<T>(content);
        }

        Console.WriteLine($"Failed to fetch access token: {content}");
        throw new Exception($"Failed to fetch access token: {content}");
    }

    private async Task UpdateAuthAsync()
    {
        const string apiAddress = "https://id.twitch.tv/oauth2/token";

        HttpClient client = new();
        HttpRequestMessage request = new(HttpMethod.Post, apiAddress)
        {
            Content = new FormUrlEncodedContent(new KeyValuePair<string, string>[]
            {
                new("client_id", Globals.TwitchClientId!),
                new("client_secret", Globals.TwitchClientSecret!),
                new("grant_type", "client_credentials")
            })
        };
        request.Headers.Add("client-id", Globals.TwitchClientId);

        HttpResponseMessage response = await client.SendAsync(request);
        string content = await response.Content.ReadAsStringAsync();
        
        if (response.IsSuccessStatusCode)
        {
            _accessToken = JsonConvert.DeserializeObject<TwitchApiAccessResponse>(content)!.AccessToken!;
            return;
        }

        Console.WriteLine($"Failed to fetch access token ({apiAddress}): {content}");
    }

    private class TwitchApiAccessResponse
    {
        [JsonProperty("access_token")] public string? AccessToken { get; set; }
    }
}
