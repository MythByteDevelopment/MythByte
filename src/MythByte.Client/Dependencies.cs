using Microsoft.Extensions.DependencyInjection;

namespace MythByte.Client;

public static class Dependencies
{
    public static IServiceCollection RegisterClient(this IServiceCollection services)
    {
        services.RegisterScreens();

        return services;
    }

    private static IServiceCollection RegisterScreens(this IServiceCollection services)
    {
        return services;
    }
}
