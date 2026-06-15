namespace MisterTerminal.Sample;

public class AssemblyInitializer : IAssemblyInitializer
{
    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddMisterTerminal();
        services.AddSingleton<ISampleConsole, SampleConsole>();
    }

    public void Configure(IInitializerContext context)
    {

    }
}