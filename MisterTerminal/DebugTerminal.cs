namespace ToolBX.MisterTerminal;

public interface IDebugTerminal : ITerminalWriter
{

}

#if DEBUG
[AutoInject(ServiceLifetime.Scoped)]
public class DebugTerminal : TerminalWriter, IDebugTerminal
{
    public DebugTerminal(IConsole console, IOptions<TerminalSettings> settings, IDmlAnsiConverter dmlAnsiConverter) : base(console, settings, dmlAnsiConverter)
    {
        DefaultBackgroundColor = settings.Value.Debug?.Color?.Background ?? DefaultColors.Debug.Background!.Value;
        DefaultForegroundColor = settings.Value.Debug?.Color?.Foreground ?? DefaultColors.Debug.Foreground!.Value;

        Wrote += OnWrite;
    }

    private void OnWrite(object sender, WriteEventArgs args) => Debug.WriteLine(args.Text);

}
#else
[AutoInject(ServiceLifetime.Scoped)]
public class DebugTerminal :  IDebugTerminal
{
    public Color BackgroundColor { get; set; }
    public Color ForegroundColor { get; set; }
    public event WriteEventHandler? Wrote;

    public void Write(string text, params object[]? args)
    {
        
    }

    public void TryWrite(string text, params object[]? args)
    {

    }

    public Task WriteAsync(string text, params object[] args) => Task.CompletedTask;

    public Task TryWriteAsync(string text, params object[] args) => Task.CompletedTask;

    public void ResetColor()
    {

    }
}
#endif

