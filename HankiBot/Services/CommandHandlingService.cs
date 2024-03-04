using System;
using System.Reflection;
using System.Threading.Channels;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using HankiBot.Models;
using Microsoft.Extensions.DependencyInjection;

namespace HankiBot.Services;

public class CommandHandlingService
{
    private readonly CommandService _commands;
    private readonly DiscordSocketClient _discord;
    private readonly IServiceProvider _services;

    public CommandHandlingService(IServiceProvider services)
    {
        _commands = services.GetRequiredService<CommandService>();
        _discord = services.GetRequiredService<DiscordSocketClient>();
        _services = services;

        // Hook CommandExecuted to handle post-command-execution logic.
        _commands.CommandExecuted += CommandExecutedAsync;
        // Hook MessageReceived so we can process each message to see
        // if it qualifies as a command.
        _discord.MessageReceived += MessageReceivedAsync;
    }

    public async Task InitializeAsync()
    {
        // Register modules that are public and inherit ModuleBase<T>.
        await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
    }

    public async Task MessageReceivedAsync(SocketMessage rawMessage)
    {
        // Ignore system messages, or messages from other bots
        if (rawMessage is not SocketUserMessage message)
            return;
        if (message.Source != MessageSource.User)
            return;

        // This value holds the offset where the prefix ends
        int argPos = 0;
        // Perform prefix check
        if (!message.HasStringPrefix(Globals.CommandPrefix, ref argPos))
        {
            List<ServerConfig> configs = Configs.GetAllServerConfig();

            foreach (ServerConfig config in configs.Where(config => !string.IsNullOrEmpty(config.SuggestionChannel)))
            {
                try
                {
                    ulong channelId = ulong.Parse(config.SuggestionChannel ?? "0");
                    if (channelId != rawMessage.Channel.Id)
                    {
                        continue;
                    }

                    // Create an embed for Discord
                    EmbedBuilder embedBuilder = new()
                    {
                        Title = $"Suggestion #{config.SuggestionCount + 1}",
                        Color = new Color(224, 31, 64),
                        Description = rawMessage.Content,
                        Author = new EmbedAuthorBuilder
                        {
                            Name = rawMessage.Author.Username,
                            IconUrl = rawMessage.Author.GetAvatarUrl()
                        }
                    };

                    IMessageChannel? discordChannel = (IMessageChannel?)await _discord.GetChannelAsync(channelId);
                    IUserMessage? suggestion = await discordChannel!.SendMessageAsync(embed: embedBuilder.Build());

                    try
                    {
                        await rawMessage.DeleteAsync();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Failed to delete original message: {ex}");
                    }

                    try
                    {
                        await suggestion.AddReactionAsync(Emoji.Parse(":arrow_up:"));
                        await suggestion.AddReactionAsync(Emoji.Parse(":arrow_down:"));
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Failed to add votes on suggestion: {ex}");
                    }
                    
                    config.SuggestionCount++;
                    Configs.Save(config);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to handle suggestion on server: {ex}");
                }
            }

            return;
        }

        SocketCommandContext context = new(_discord, message);
        // Perform the execution of the command. In this method,
        // the command service will perform precondition and parsing check
        // then execute the command if one is matched.
        await _commands.ExecuteAsync(context, argPos, _services);
        // Note that normally a result will be returned by this format, but here
        // we will handle the result in CommandExecutedAsync,
    }

    public async Task CommandExecutedAsync(Optional<CommandInfo> command, ICommandContext context, IResult result)
    {
        // command is unspecified when there was a search failure (command not found); we don't care about these errors
        if (!command.IsSpecified)
            return;

        // the command was successful, we don't care about this result, unless we want to log that a command succeeded.
        if (result.IsSuccess)
            return;

        // the command failed, let's notify the user that something happened.
        await context.Channel.SendMessageAsync($"error: {result}");
    }
}