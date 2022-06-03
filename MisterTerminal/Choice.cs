namespace ToolBX.MisterTerminal;

public record Choice
{
    public string Identifier { get; init; } = string.Empty;
    public string Text { get; init; }
    public Action Action { get; init; }

    public Choice()
    {
        Text = string.Empty;
        Action = () => { };
    }

    public Choice(string text, Action action)
    {
        if (string.IsNullOrWhiteSpace(text)) throw new ArgumentNullException(nameof(text));
        Text = text;
        Action = action ?? throw new ArgumentNullException(nameof(action));
    }
}