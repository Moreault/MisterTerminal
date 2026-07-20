namespace ToolBX.MisterTerminal
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Registers MisterTerminal and all of its dependencies. Registration is fully explicit and reflection-free, making it trimming/NativeAOT-safe.
        /// </summary>
        public static IServiceCollection AddMisterTerminal(this IServiceCollection services)
        {
            ArgumentNullException.ThrowIfNull(services);

            services.AddNetAbstractions();
            services.AddDml();

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
