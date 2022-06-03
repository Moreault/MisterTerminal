namespace ToolBX.MisterTerminal;

public delegate void WriteEventHandler(object sender, WriteEventArgs args);

public record WriteEventArgs
{
    public string Text { get; init; } = string.Empty;
}