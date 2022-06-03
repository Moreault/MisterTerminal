namespace ToolBX.MisterTerminal.Settings;

public record TerminalWriterSettings
{
    public record ColorScheme
    {
        public Color? Foreground { get; init; }
        public Color? Background { get; init; }
    }

    public ColorScheme? Color { get; init; }
}