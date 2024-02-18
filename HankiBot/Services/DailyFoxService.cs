using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Http.Headers;
using System.Threading.Channels;
using Discord;
using Discord.WebSocket;
using HankiBot.Models;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using TwitterSharp.Client;
using TwitterSharp.Request.AdvancedSearch;
using TwitterSharp.Request.Option;
using TwitterSharp.Response.RTweet;
using static System.Net.Mime.MediaTypeNames;
using Timer = System.Timers.Timer;

namespace HankiBot.Services;

public class DailyFoxService
{
    private readonly DiscordSocketClient _discord;
    
    private readonly HttpClient _client;
    private readonly Timer _timer;
    private const string Url = "https://api.tinyfox.dev/img?animal=fox";
    
    public DailyFoxService(IServiceProvider services)
    {
        Console.WriteLine("Starting daily fox service...");

        _client = services.GetRequiredService<HttpClient>();
        _discord = services.GetRequiredService<DiscordSocketClient>();
        
        _timer = new Timer(1000);
        _timer.Elapsed += (_, _) =>
        {
            if (Configs.GetNextFoxTime() > DateTime.Now)
            {
                return;
            }

            Configs.SetNextFoxTime(DateTime.Now.AddDays(1));
            _ = Task.Run(async () => await FopAsync());
        };
        _timer.Start();
        
        Console.WriteLine("Started daily fox service!");
    }

    private async Task FopAsync()
    {
        try
        {
            // Send request and get response
            HttpResponseMessage response = await _client.GetAsync(Url);

            // Ensure success status code
            response.EnsureSuccessStatusCode();

            // Get content as stream
            await using Stream contentStream = await response.Content.ReadAsStreamAsync();

            // Specify where to save the file
            const string filePath = "foxTemp.jpg";
            
            // Save content stream to file
            await using (FileStream fileStream = File.Create(filePath))
            {
                await contentStream.CopyToAsync(fileStream);
            }

            // Create an embed for Discord
            EmbedBuilder embedBuilder = new()
            {
                Title = "Daily fox 🦊",
                Color = new Color(236, 88, 0),
                Footer = new EmbedFooterBuilder
                {
                    Text = $"Daily foxes provided by {Url}"
                },
                ImageUrl = "attachment://foxTemp.jpg"
            };

            List<ServerConfig> configs = Configs.GetAllServerConfig();
            
            foreach (ServerConfig serverConfig in configs.Where(serverConfig =>
                         !string.IsNullOrEmpty(serverConfig.DailyFoxChannel)))
            {
                try
                {
                    if (string.IsNullOrEmpty(serverConfig.DailyFoxChannel))
                    {
                        continue;
                    }

                    IMessageChannel? discordChannel =
                        (IMessageChannel?)await _discord.GetChannelAsync(ulong.Parse(serverConfig.DailyFoxChannel));
                    await using FileStream fileStream = File.OpenRead(filePath);
                    await discordChannel?.SendFileAsync(fileStream, "foxTemp.jpg", embed: embedBuilder.Build())!;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to send daily fox to server: {ex}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to run daily fox: {ex}");
        }
    }
}
