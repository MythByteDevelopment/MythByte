using MythByte.Client.Input;
using MythByte.Client.Rendering;

namespace MythByte.Client.Screens;

public interface IScreenManager
{
    event Action? ExitRequested;
    IReadOnlyList<IScreen> Screens { get; }
    bool IsTransitioning { get; }
    float FadeAlpha { get; }
    void Push(IScreen screen);
    void Pop();
    void Replace(IScreen screen);
    void RequestExit();
    void Update(float elapsedSeconds, IInputService input);
    void Draw(ICanvas canvas);
}
