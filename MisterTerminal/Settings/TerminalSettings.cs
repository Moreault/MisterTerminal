namespace ToolBX.MisterTerminal.Settings;

[AutoConfig("Terminal")]
public record TerminalSettings
{
    public record TimeStampSettings
    {
        public bool Use { get; init; } = true;
        public string Format { get; init; } = "yyyy-MM-dd hh:mm:ss";
    }

    public TerminalWriterSettings? Main { get; init; } = new();
    public TerminalWriterSettings? Notification { get; init; } = new();
    public TerminalWriterSettings? Debug { get; init; } = new();
    public TerminalWriterSettings? Error { get; init; } = new();
    public TerminalWriterSettings? Warning { get; init; } = new();

    public TimeStampSettings TimeStamps { get; init; } = new();
}

