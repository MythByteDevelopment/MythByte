using Microsoft.Extensions.DependencyInjection;
using MythByte.Application;
using MythByte.Infrastructure;
using MythByte.Networking;

namespace MythByte.IntegrationTests;

public class DependenciesTests
{
    [Fact]
    public void RegisterLayers_AllEntryPoints_BuildServiceProvider()
    {
        var services = new ServiceCollection();

        services
            .RegisterApplication()
            .RegisterInfrastructure()
            .RegisterNetworking();
        using var provider = services.BuildServiceProvider(new ServiceProviderOptions { ValidateOnBuild = true });

        Assert.NotNull(provider);
    }
}
