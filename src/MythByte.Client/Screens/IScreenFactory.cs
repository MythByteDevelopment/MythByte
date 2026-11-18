namespace MythByte.Client.Screens;

public interface IScreenFactory
{
    IScreen CreateTitle();
    IScreen CreateWorld();
    IScreen CreatePauseMenu();
}
