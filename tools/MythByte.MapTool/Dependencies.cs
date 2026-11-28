using Microsoft.Extensions.DependencyInjection;

namespace MythByte.MapTool;

public static class Dependencies
{
    public static IServiceCollection RegisterMapTool(this IServiceCollection services)
    {
        services.RegisterCommands();

        return services;
    }

    private static IServiceCollection RegisterCommands(this IServiceCollection services)
    {
        services.AddSingleton<SyncNamesCommand>();

        return services;
    }
}
