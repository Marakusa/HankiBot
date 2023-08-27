namespace HankiBot.Models;

public class CommandGroupAttribute : Attribute
{
    public CommandGroupAttribute(string title, int order = -1)
    {
        Title = title;
        Order = order;
    }

    public string Title { get; private set; }
    public int Order { get; set; }
}