namespace ToolBX.MisterTerminal;

public static class ColorExtensions
{
    public static string ToAnsiTrueColor(this Color color)
    {
        return $"\x1b[38;2;{color.Red};{color.Green};{color.Blue}m";
    }

    public static string ToAnsiTrueColorHighlight(this Color color)
    {
        return $"\x1b[48;2;{color.Red};{color.Green};{color.Blue}m";
    }
}