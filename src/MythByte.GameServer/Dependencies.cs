using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MythByte.GameServer.Connections;
using MythByte.GameServer.Options;

namespace MythByte.GameServer;

public static class Dependencies
{
    public static IServiceCollection RegisterGameServer(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .RegisterOptions(configuration)
            .RegisterConnections()
            .RegisterSimulation();

        return services;
    }

    private static IServiceCollection RegisterOptions(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddOptions<GameServerOptions>()
            .Bind(configuration.GetSection(GameServerOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        return services;
    }

    private static IServiceCollection RegisterConnections(this IServiceCollection services)
    {
        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<IConnectionManager, ConnectionManager>();

        return services;
    }

    private static IServiceCollection RegisterSimulation(this IServiceCollection services)
    {
        services.AddHostedService<GameServerWorker>();

        return services;
    }
}
