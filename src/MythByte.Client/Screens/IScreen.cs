using MythByte.Client.Input;
using MythByte.Client.Rendering;

namespace MythByte.Client.Screens;

public interface IScreen
{
    bool IsOverlay { get; }
    void Update(float elapsedSeconds, IInputService input);
    void Draw(ICanvas canvas);
}
