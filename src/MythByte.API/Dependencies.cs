namespace MythByte.API;

public static class Dependencies
{
    public static IServiceCollection RegisterAPI(this IServiceCollection services)
    {
        services.RegisterControllers();

        return services;
    }

    private static IServiceCollection RegisterControllers(this IServiceCollection services)
    {
        services.AddControllers();

        return services;
    }
}
