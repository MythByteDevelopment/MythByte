using Microsoft.Xna.Framework.Input;

namespace MythByte.Client.Input;

public interface IInputService
{
    void Update(KeyboardState keyboard, GamePadState gamePad);
    bool IsDown(GameAction action);
    bool IsPressed(GameAction action);
}
