using Microsoft.Xna.Framework.Input;

namespace MythByte.Client.Input;

public class InputService(InputMap map) : IInputService
{
    private KeyboardState _previousKeyboard;
    private KeyboardState _currentKeyboard;
    private GamePadState _previousGamePad;
    private GamePadState _currentGamePad;

    public void Update(KeyboardState keyboard, GamePadState gamePad)
    {
        _previousKeyboard = _currentKeyboard;
        _previousGamePad = _currentGamePad;
        _currentKeyboard = keyboard;
        _currentGamePad = gamePad;
    }

    public bool IsDown(GameAction action) => IsActive(action, _currentKeyboard, _currentGamePad);

    public bool IsPressed(GameAction action) =>
        IsActive(action, _currentKeyboard, _currentGamePad) && !IsActive(action, _previousKeyboard, _previousGamePad);

    private bool IsActive(GameAction action, KeyboardState keyboard, GamePadState gamePad)
    {
        if (map.KeyBindings.TryGetValue(action, out var keys) && keys.Any(keyboard.IsKeyDown))
            return true;

        if (map.ButtonBindings.TryGetValue(action, out var buttons) && buttons.Any(gamePad.IsButtonDown))
            return true;

        var stick = gamePad.ThumbSticks.Left;
        return action switch
        {
            GameAction.Up => stick.Y > map.StickThreshold,
            GameAction.Down => stick.Y < -map.StickThreshold,
            GameAction.Left => stick.X < -map.StickThreshold,
            GameAction.Right => stick.X > map.StickThreshold,
            _ => false
        };
    }
}
