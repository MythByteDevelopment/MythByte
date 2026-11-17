using Microsoft.Extensions.DependencyInjection;

namespace MythByte.Infrastructure;

public static class Dependencies
{
    public static IServiceCollection RegisterInfrastructure(this IServiceCollection services)
    {
        services
            .RegisterPersistence()
            .RegisterRepositories();

        return services;
    }

    private static IServiceCollection RegisterPersistence(this IServiceCollection services)
    {
        return services;
    }

    private static IServiceCollection RegisterRepositories(this IServiceCollection services)
    {
        return services;
    }
}
