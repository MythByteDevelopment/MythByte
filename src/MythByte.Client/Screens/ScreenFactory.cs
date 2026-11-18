using Microsoft.Extensions.DependencyInjection;

namespace MythByte.Client.Screens;

public class ScreenFactory(IServiceProvider serviceProvider) : IScreenFactory
{
    public IScreen CreateTitle() => ActivatorUtilities.CreateInstance<TitleScreen>(serviceProvider);

    public IScreen CreateWorld() => ActivatorUtilities.CreateInstance<WorldScreen>(serviceProvider);

    public IScreen CreatePauseMenu() => ActivatorUtilities.CreateInstance<PauseMenuScreen>(serviceProvider);
}
