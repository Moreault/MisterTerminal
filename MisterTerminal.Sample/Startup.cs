namespace MisterTerminal.Sample;

public class Startup(IConfiguration configuration) : ConsoleStartup(configuration)
{
    public override void Run(IServiceProvider serviceProvider) => serviceProvider.GetRequiredService<ISampleConsole>().Start();
}