namespace ToolBX.MisterTerminal;

public interface ITerminalWriter
{
    Color BackgroundColor { get; set; }
    Color ForegroundColor { get; set; }

    /// <summary>
    /// Triggers every time the terminal writes text.
    /// </summary>
    event WriteEventHandler Wrote;

    void Write(string text, params object[]? args);
    void TryWrite(string text, params object[]? args);

    /// <summary>
    /// Resets colors to their default values defined in the appsettings.json file or to good old white on black if there is no configuration file.
    /// </summary>
    void ResetColor();
}

public abstract class TerminalWriter : ITerminalWriter
{
    private readonly IConsole _console;
    private readonly IDmlAnsiConverter _dmlAnsiConverter;

    public Color BackgroundColor
    {
        get => _overridenBackgroundColor ?? DefaultBackgroundColor;
        set => _overridenBackgroundColor = value;
    }
    private Color? _overridenBackgroundColor;
    public Color ForegroundColor
    {
        get => _overridenForegroundColor ?? DefaultForegroundColor;
        set => _overridenForegroundColor = value;
    }
    private Color? _overridenForegroundColor;

    protected Color DefaultBackgroundColor { get; init; }
    protected Color DefaultForegroundColor { get; init; }

    public event WriteEventHandler? Wrote;

    private readonly TerminalSettings _settings;

    private string TimeStamp => string.Format($"[{TimeProvider.TimeProvider.Now.ToString(_settings.TimeStamps.Format)}]");

    protected TerminalWriter(IConsole console, IOptions<TerminalSettings> settings, IDmlAnsiConverter dmlAnsiConverter)
    {
        _console = console;
        _settings = settings.Value;
        _dmlAnsiConverter = dmlAnsiConverter;
        _console.ForegroundColor = ConsoleColor.Gray;
    }

    public void Write(string text, params object[]? args)
    {
        WriteWithoutBreakingLine(text, args);

        _console.WriteLine();
    }

    protected internal void WriteWithoutBreakingLine(string text, params object[]? args)
    {
        if (string.IsNullOrWhiteSpace(text)) throw new ArgumentNullException(nameof(text));
        if (_settings.TimeStamps.Use) text = $"{TimeStamp} {text}";

        if (args != null && args.Any())
            text = string.Format(text, args);

        text = text.Color(ForegroundColor);
        var formattedText = _dmlAnsiConverter.Convert(text);

        _console.Write(formattedText);

        Wrote?.Invoke(this, new WriteEventArgs
        {
            Text = formattedText
        });
    }

    public void TryWrite(string text, params object[]? args)
    {
        if (!string.IsNullOrWhiteSpace(text))
            Write(text, args);
    }

    public void ResetColor()
    {
        _overridenBackgroundColor = null;
        _overridenForegroundColor = null;
    }
}