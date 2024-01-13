namespace ToolBX.MisterTerminal
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMisterTerminal(this IServiceCollection services)
        {
            return services
                .AddSingleton<IDmlAnsiConverter, DmlAnsiConverter>()
                .AddSingleton<IDebugTerminal, DebugTerminal>()
                .AddSingleton<INotificationTerminal, NotificationTerminal>()
                .AddSingleton<IErrorTerminal, ErrorTerminal>()
                .AddSingleton<IWarningTerminal, WarningTerminal>()
                .AddSingleton<ITerminal, Terminal>();
        }
    }
}
