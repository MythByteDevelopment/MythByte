using Microsoft.Extensions.DependencyInjection;
using MythByte.Networking.Transport;

namespace MythByte.Networking;

public static class Dependencies
{
    public static IServiceCollection RegisterNetworking(this IServiceCollection services)
    {
        services.RegisterTransport();

        return services;
    }

    private static IServiceCollection RegisterTransport(this IServiceCollection services)
    {
        services.AddSingleton<IServerTransport, LiteNetServerTransport>();
        services.AddTransient<IClientTransport, LiteNetClientTransport>();

        return services;
    }
}
