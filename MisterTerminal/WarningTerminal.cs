namespace ToolBX.MisterTerminal;

public interface IWarningTerminal : ITerminalWriter
{

}

[AutoInject(ServiceLifetime.Scoped)]
public class WarningTerminal : TerminalWriter, IWarningTerminal
{
    public WarningTerminal(IConsole console, IOptions<TerminalSettings> settings, IDmlAnsiConverter dmlAnsiConverter) : base(console, settings, dmlAnsiConverter)
    {
        DefaultBackgroundColor = settings.Value.Warning?.Color?.Background ?? DefaultColors.Warning.Background!.Value;
        DefaultForegroundColor = settings.Value.Warning?.Color?.Foreground ?? DefaultColors.Warning.Foreground!.Value;
    }
}