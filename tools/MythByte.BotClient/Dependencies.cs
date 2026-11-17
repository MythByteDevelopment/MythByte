using Microsoft.Extensions.DependencyInjection;

namespace MythByte.BotClient;

public static class Dependencies
{
    public static IServiceCollection RegisterBotClient(this IServiceCollection services)
    {
        services.RegisterBots();

        return services;
    }

    private static IServiceCollection RegisterBots(this IServiceCollection services)
    {
        return services;
    }
}
