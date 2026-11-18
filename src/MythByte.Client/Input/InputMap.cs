using Microsoft.Xna.Framework.Input;

namespace MythByte.Client.Input;

public class InputMap
{
    public required IReadOnlyDictionary<GameAction, Keys[]> KeyBindings { get; init; }
    public required IReadOnlyDictionary<GameAction, Buttons[]> ButtonBindings { get; init; }
    public float StickThreshold { get; init; } = 0.5f;

    public static InputMap Default => new()
    {
        KeyBindings = new Dictionary<GameAction, Keys[]>
        {
            [GameAction.Up] = [Keys.Up, Keys.W],
            [GameAction.Down] = [Keys.Down, Keys.S],
            [GameAction.Left] = [Keys.Left, Keys.A],
            [GameAction.Right] = [Keys.Right, Keys.D],
            [GameAction.Confirm] = [Keys.Z, Keys.Enter],
            [GameAction.Back] = [Keys.X, Keys.Escape]
        },
        ButtonBindings = new Dictionary<GameAction, Buttons[]>
        {
            [GameAction.Up] = [Buttons.DPadUp],
            [GameAction.Down] = [Buttons.DPadDown],
            [GameAction.Left] = [Buttons.DPadLeft],
            [GameAction.Right] = [Buttons.DPadRight],
            [GameAction.Confirm] = [Buttons.A],
            [GameAction.Back] = [Buttons.B, Buttons.Start]
        }
    };
}
