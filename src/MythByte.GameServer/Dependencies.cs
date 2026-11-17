using Microsoft.Extensions.DependencyInjection;

namespace MythByte.GameServer;

public static class Dependencies
{
    public static IServiceCollection RegisterGameServer(this IServiceCollection services)
    {
        services.RegisterSimulation();

        return services;
    }

    private static IServiceCollection RegisterSimulation(this IServiceCollection services)
    {
        return services;
    }
}
