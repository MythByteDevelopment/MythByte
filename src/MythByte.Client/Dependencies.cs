using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MythByte.Client.Input;
using MythByte.Client.Options;
using MythByte.Client.Screens;

namespace MythByte.Client;

public static class Dependencies
{
    public static IServiceCollection RegisterClient(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .RegisterOptions(configuration)
            .RegisterInput()
            .RegisterScreens()
            .RegisterGame();

        return services;
    }

    private static IServiceCollection RegisterOptions(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddOptions<ClientOptions>()
            .Bind(configuration.GetSection(ClientOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        return services;
    }

    private static IServiceCollection RegisterInput(this IServiceCollection services)
    {
        services.AddSingleton(InputMap.Default);
        services.AddSingleton<IInputService, InputService>();

        return services;
    }

    private static IServiceCollection RegisterScreens(this IServiceCollection services)
    {
        services.AddSingleton<IScreenManager, ScreenManager>();
        services.AddSingleton<IScreenFactory, ScreenFactory>();

        return services;
    }

    private static IServiceCollection RegisterGame(this IServiceCollection services)
    {
        services.AddSingleton<MythByteGame>();

        return services;
    }
}
