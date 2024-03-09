using Discord.Commands;
using Discord.WebSocket;
using HankiBot.Models;
using HankiBot.Utils;

namespace HankiBot.Commands;

[CommandGroup("Fun")]
public class FunModule : ModuleBase<SocketCommandContext>
{
    [Command("bonk")]
    [Ignore]
    public async Task BonkAsync()
    {
        await ReplyAsync(
            $"{Context.User.Mention} Who do you want to bonk? Include their name after {Globals.CommandPrefix}bonk [target].");
    }

    [Command("bonk")]
    [Summary("Bonk someone")]
    public async Task BonkAsync([Summary("Who is getting bonked")] SocketUser target)
    {
        await ReplyAsync($"{Context.User.Mention} bonks {target.Mention}!");
        await ReplyAsync(FunUtils.PickRandom(Globals.BonkImages!));
    }

    [Command("howcuteami")]
    [Summary("See how cute you are today")]
    public async Task CuteAsync()
    {
        await Context.Channel.SendMessageAsync($"{Context.User.Mention} You are {FunUtils.CalculateRandom(1, Context.User.Id, DateTime.Today.DayOfYear)}% cute today!");
    }

    [Command("howstinkyami")]
    [Summary("See how stinky you are today")]
    public async Task StinkyAsync()
    {
        int stinky = FunUtils.CalculateRandom(2, Context.User.Id, DateTime.Today.DayOfYear, 2);
        string comment = stinky switch
        {
            0 => "You smell wonderful today!",
            1 => "*sniffs* Sniff test passed.",
            _ => "You stimky bean!"
        };
        await Context.Channel.SendMessageAsync($"{Context.User.Mention} {comment}");
    }

    [Command("howfurryami")]
    [Summary("See how furry you are today")]
    public async Task FurryAsync()
    {
        await Context.Channel.SendMessageAsync($"{Context.User.Mention} You are {FunUtils.CalculateRandom(3, Context.User.Id, DateTime.Today.DayOfYear)}% furry today!");
    }

    [Command("howhornyami")]
    [Summary("See how horny you are today")]
    public async Task HornyAsync()
    {
        await Context.Channel.SendMessageAsync($"{Context.User.Mention} You are {FunUtils.CalculateRandom(4, Context.User.Id, DateTime.Today.DayOfYear)}% horny today!");
    }

    [Command("howvornyami")]
    [Summary("See how vorny you are today")]
    public async Task VornyAsync()
    {
        await Context.Channel.SendMessageAsync($"{Context.User.Mention} You are {FunUtils.CalculateRandom(5, Context.User.Id, DateTime.Today.DayOfYear)}% vorny today!");
    }
}