namespace ToolBX.MisterTerminal;

internal static class DefaultColors
{
    internal static readonly TerminalWriterSettings.ColorScheme Debug = new()
    {
        Background = new Color(0, 0, 0),
        Foreground = new Color(150, 150, 150)
    };

    internal static readonly TerminalWriterSettings.ColorScheme Main = new()
    {
        Background = new Color(0, 0, 0),
        Foreground = new Color(204, 204, 204)
    };

    internal static readonly TerminalWriterSettings.ColorScheme Error = new()
    {
        Background = new Color(0, 0, 0),
        Foreground = new Color(255, 118, 68)
    };

    internal static readonly TerminalWriterSettings.ColorScheme Warning = new()
    {
        Background = new Color(0, 0, 0),
        Foreground = new Color(255, 241, 124)
    };

    internal static readonly TerminalWriterSettings.ColorScheme Notification = new()
    {
        Background = new Color(0, 0, 0),
        Foreground = new Color(135, 255, 124)
    };
}