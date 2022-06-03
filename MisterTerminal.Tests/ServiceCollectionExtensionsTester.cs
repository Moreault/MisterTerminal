using Microsoft.Extensions.DependencyInjection;

namespace MisterTerminal.Tests;

[TestClass]
public class ServiceCollectionExtensionsTester
{
    [TestMethod]
    public void Always_AddServices()
    {
        //Arrange
        var instance = new FakeServiceCollection();

        //Act
        instance.AddMisterTerminal();

        //Assert
        instance.Should().BeEquivalentTo(new List<ServiceDescriptor>
        {
            new(typeof(IDmlAnsiConverter), typeof(DmlAnsiConverter), ServiceLifetime.Singleton),
            new(typeof(IDebugTerminal), typeof(DebugTerminal), ServiceLifetime.Singleton),
            new(typeof(INotificationTerminal), typeof(NotificationTerminal), ServiceLifetime.Singleton),
            new(typeof(IErrorTerminal), typeof(ErrorTerminal), ServiceLifetime.Singleton),
            new(typeof(IWarningTerminal), typeof(WarningTerminal), ServiceLifetime.Singleton),
            new(typeof(ITerminal), typeof(Terminal), ServiceLifetime.Singleton),
        });
    }
}