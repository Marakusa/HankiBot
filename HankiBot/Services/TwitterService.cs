using Discord;
using Discord.WebSocket;
using HankiBot.Models;
using Microsoft.Extensions.DependencyInjection;
using TwitterSharp.Client;
using TwitterSharp.Request.AdvancedSearch;
using TwitterSharp.Request.Option;
using TwitterSharp.Response.RTweet;
using Timer = System.Timers.Timer;

namespace HankiBot.Services;

public class TwitterService
{
    private readonly DiscordSocketClient _discord;
    
    private readonly Timer _timer;
    private readonly TwitterClient _client;

    public TwitterService(IServiceProvider services)
    {
        Console.WriteLine("Starting Twitter service...");

        _discord = services.GetRequiredService<DiscordSocketClient>();
        
        _client = new TwitterClient("AAAAAAAAAAAAAAAAAAAAAAmvsQEAAAAAj%2Bt4zXrCYLvpoIs%2B2oJbSB6mbdU%3DGzwxB8Fmnunlk7nr0e6jBp3EFuO1VQFU6Q6nN1KuFXYBiE0K28");
        
        _timer = new Timer(5000);
        _timer.Elapsed += (_, _) =>
        {
            _ = Task.Run(async () => await TweetIntervalAsync());
        };
        _timer.Start();

        Console.WriteLine("Started Twitter service!");
        _ = Task.Run(async () => await LogAsync("Started Twitter service!"));
    }

    private async Task LogAsync(string s)
    {
        IMessageChannel? discordChannel = (IMessageChannel?)await _discord.GetChannelAsync(556778120372420631);
        await discordChannel?.SendMessageAsync(s)!;
    }

    private async Task TweetIntervalAsync()
    {
        List<ServerConfig> configs = Configs.GetAllServerConfig();

        await LogAsync(configs.Count.ToString());
        foreach (ServerConfig config in configs)
        {
            try
            {
                if (config.TwitterNotificationChannel == null)
                    continue;

                Tweet[]? answer = await _client.GetTweetsFromUserIdAsync("932998397451231232", new TweetSearchOptions
                {
                    TweetOptions = new[] { TweetOption.Attachments },
                    MediaOptions = new[] { MediaOption.Preview_Image_Url },
                    StartTime = DateTime.Today.AddDays(-7)
                });
                for (int i = 0; i < answer.Length; i++)
                {
                    Tweet tweet = answer[i];
                    Console.WriteLine($"Tweet n°{i}");
                    Console.WriteLine(tweet.Text);
                    if (tweet.Attachments?.Media?.Any() ?? false)
                    {
                        Console.WriteLine("\nImages:");
                        Console.WriteLine(string.Join("\n", tweet.Attachments.Media.Select(x => x.Url)));
                    }

                    Console.WriteLine("\n");
                }

                /*IMessageChannel? discordChannel =
                    await _discord.GetChannelAsync(
                        ulong.Parse(config.TwitterNotificationChannel)) as IMessageChannel;
                await discordChannel?.SendMessageAsync(
                    $"Look at this new awesome tweet from Mara!\nhttps://www.twitter.com/{tweet}")!;*/
            }
            catch (Exception ex)
            {
                await LogAsync($"Failed to fetch Tweets: {ex}");
            }
        }
    }
}
