using Microsoft.Extensions.DependencyInjection;

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
        return services;
    }
}
