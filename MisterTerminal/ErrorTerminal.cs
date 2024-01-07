namespace ToolBX.MisterTerminal;

public interface IErrorTerminal : ITerminalWriter
{

}

//TODO Log error (and possibly warnings) automatically somewhere (perhaps support a few logging libraries?)
[AutoInject(ServiceLifetime.Scoped)]
public class ErrorTerminal : TerminalWriter, IErrorTerminal
{
    public ErrorTerminal(IConsole console, IOptions<TerminalSettings> settings, IDmlAnsiConverter dmlAnsiConverter) : base(console, settings, dmlAnsiConverter)
    {
        DefaultBackgroundColor = settings.Value.Error?.Color?.Background ?? DefaultColors.Error.Background!.Value;
        DefaultForegroundColor = settings.Value.Error?.Color?.Foreground ?? DefaultColors.Error.Foreground!.Value;
    }
}