namespace ToolBX.MisterTerminal;

public interface INotificationTerminal : ITerminalWriter
{

}

[AutoInject(ServiceLifetime.Scoped)]
public class NotificationTerminal : TerminalWriter, INotificationTerminal
{
    public NotificationTerminal(IConsole console, IOptions<TerminalSettings> settings, IDmlAnsiConverter dmlAnsiConverter) : base(console, settings, dmlAnsiConverter)
    {
        DefaultBackgroundColor = settings.Value.Notification?.Color?.Background ?? DefaultColors.Notification.Background!.Value;
        DefaultForegroundColor = settings.Value.Notification?.Color?.Foreground ?? DefaultColors.Notification.Foreground!.Value;
    }
}