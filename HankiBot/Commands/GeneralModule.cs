using System.Reflection;
using Discord;
using Discord.Commands;
using HankiBot.Models;

namespace HankiBot.Commands;

[CommandGroup("General", 1000)]
public class GeneralModule : ModuleBase<SocketCommandContext>
{
    [Command("h")]
    [Summary("List all the commands for the bot")]
    public async Task HelpShortAsync()
    {
        await HelpAsync();
    }

    [Command("help")]
    [Summary("List all the commands for the bot")]
    public async Task HelpAsync()
    {
        Dictionary<string, List<CommandInfo>> groupedCommands = GetGroupedCommands(Context);

        EmbedBuilder builder = new()
        {
            Title = "List of available commands"
        };

        foreach (KeyValuePair<string, List<CommandInfo>> group in groupedCommands)
        {
            IEnumerable<string> commandsInfo = group.Value.Select(cmd =>
            {
                string parameters = string.Join(" ", cmd.Parameters.Select(param => $"[{param.Key}]"));
                string parametersList = string.Join("\n", cmd.Parameters.Select(param => $"*{param.Key}*: {param.Value}"));
                return
                    $"**{Globals.CommandPrefix}{cmd.Command}{(parameters.Length > 0 ? " " + parameters : "")}**: {cmd.Summary}{(parametersList.Length > 0 ? "\n " + parametersList : "")}";
            });

            builder.AddField(group.Key, string.Join(Environment.NewLine, commandsInfo));
        }

        Embed? embed = builder.Build();
        await ReplyAsync(embed: embed);
    }

    private static Dictionary<string, List<CommandInfo>> GetGroupedCommands(SocketCommandContext context)
    {
        var groupedCommands = new Dictionary<string, List<CommandInfo>>();

        List<Type> moduleClasses = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(type => type is { IsClass: true, IsAbstract: false } &&
                           type.IsSubclassOf(typeof(ModuleBase<SocketCommandContext>)))
            .OrderByDescending(o => o.GetCustomAttribute<CommandGroupAttribute>()?.Order)
            .ToList();

        foreach (Type moduleClass in moduleClasses)
        {
            bool configChannel = ValidateConfigCommand(context);
            if (moduleClass.GetCustomAttributes(typeof(IgnoreAttribute), false).Length > 0 && !configChannel)
                continue;

            CommandGroupAttribute? groupAttribute = moduleClass.GetCustomAttribute<CommandGroupAttribute>();
            List<CommandInfo> commandMethods = moduleClass.GetMethods()
                .Where(method => method.GetCustomAttributes(typeof(CommandAttribute), false).Length > 0 &&
                                 method.GetCustomAttributes(typeof(IgnoreAttribute), false).Length <= 0)
                .Select(method =>
                {
                    string commandName = method.GetCustomAttribute<CommandAttribute>()?.Text ?? "Unknown";
                    string summary = method.GetCustomAttribute<SummaryAttribute>()?.Text ?? "No summary available";
                    Dictionary<string, string> parameters = GetCommandParameters(method);
                    return new CommandInfo { Command = commandName, Summary = summary, Parameters = parameters };
                })
                .ToList();

            string groupName = groupAttribute?.Title ?? "General";

            if (!groupedCommands.ContainsKey(groupName))
                groupedCommands[groupName] = new List<CommandInfo>();

            groupedCommands[groupName].AddRange(commandMethods);
        }

        return groupedCommands;
    }

    private static bool ValidateConfigCommand(SocketCommandContext context)
    {
        bool owner = context.Guild.OwnerId == context.User.Id;
        string configChannel = Configs.GetServerConfig(context.Guild.Id)!.ConfigChannel!;
        return owner && context.Channel.Id.ToString() == configChannel;
    }

    private static Dictionary<string, string> GetCommandParameters(MethodInfo method)
    {
        return method.GetParameters().ToDictionary(parameter => parameter.Name ?? "",
            parameter => parameter.GetCustomAttribute<SummaryAttribute>()?.Text ?? "Parameter");
    }

    private class CommandInfo
    {
        public string Command { get; init; }
        public string Summary { get; init; }
        public Dictionary<string, string> Parameters { get; init; }
    }

    [Command("ping")]
    [Summary("Pong")]
    public async Task PingAsync()
    {
        await Context.Channel.SendMessageAsync("Pong");
    }
}