namespace MisterTerminal.Sample;

public class Startup : ConsoleStartup
{
    public Startup(IConfiguration configuration) : base(configuration)
    {
    }

    public override void Run(IServiceProvider serviceProvider) => serviceProvider.GetRequiredService<ISampleConsole>().Start();
}