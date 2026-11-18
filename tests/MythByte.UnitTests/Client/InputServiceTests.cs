using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MythByte.Client.Input;

namespace MythByte.UnitTests.Client;

public class InputServiceTests
{
    private readonly InputService _sut = new(InputMap.Default);

    private static GamePadState Pad(Vector2 leftStick, params Buttons[] buttons) =>
        new(leftStick, Vector2.Zero, 0f, 0f, buttons);

    [Theory]
    [InlineData(Keys.Up, GameAction.Up)]
    [InlineData(Keys.W, GameAction.Up)]
    [InlineData(Keys.Down, GameAction.Down)]
    [InlineData(Keys.S, GameAction.Down)]
    [InlineData(Keys.Left, GameAction.Left)]
    [InlineData(Keys.A, GameAction.Left)]
    [InlineData(Keys.Right, GameAction.Right)]
    [InlineData(Keys.D, GameAction.Right)]
    [InlineData(Keys.Z, GameAction.Confirm)]
    [InlineData(Keys.Enter, GameAction.Confirm)]
    [InlineData(Keys.X, GameAction.Back)]
    [InlineData(Keys.Escape, GameAction.Back)]
    public void IsDown_BoundKey_ReturnsTrue(Keys key, GameAction action)
    {
        _sut.Update(new KeyboardState(key), Pad(Vector2.Zero));

        var result = _sut.IsDown(action);

        Assert.True(result);
    }

    [Theory]
    [InlineData(Buttons.DPadUp, GameAction.Up)]
    [InlineData(Buttons.DPadDown, GameAction.Down)]
    [InlineData(Buttons.DPadLeft, GameAction.Left)]
    [InlineData(Buttons.DPadRight, GameAction.Right)]
    [InlineData(Buttons.A, GameAction.Confirm)]
    [InlineData(Buttons.B, GameAction.Back)]
    [InlineData(Buttons.Start, GameAction.Back)]
    public void IsDown_BoundButton_ReturnsTrue(Buttons button, GameAction action)
    {
        _sut.Update(new KeyboardState(), Pad(Vector2.Zero, button));

        var result = _sut.IsDown(action);

        Assert.True(result);
    }

    [Theory]
    [InlineData(0f, 1f, GameAction.Up)]
    [InlineData(0f, -1f, GameAction.Down)]
    [InlineData(-1f, 0f, GameAction.Left)]
    [InlineData(1f, 0f, GameAction.Right)]
    public void IsDown_LeftStickPushed_ReturnsDirection(float x, float y, GameAction action)
    {
        _sut.Update(new KeyboardState(), Pad(new Vector2(x, y)));

        var result = _sut.IsDown(action);

        Assert.True(result);
    }

    [Fact]
    public void IsDown_LeftStickExactlyAtThreshold_ReturnsFalse()
    {
        _sut.Update(new KeyboardState(), Pad(new Vector2(0f, 0.5f)));

        var result = _sut.IsDown(GameAction.Up);

        Assert.False(result);
    }

    [Fact]
    public void IsDown_NothingPressed_ReturnsFalse()
    {
        _sut.Update(new KeyboardState(), Pad(Vector2.Zero));

        var result = _sut.IsDown(GameAction.Confirm);

        Assert.False(result);
    }

    [Fact]
    public void IsPressed_KeyJustPressed_ReturnsTrue()
    {
        _sut.Update(new KeyboardState(), Pad(Vector2.Zero));
        _sut.Update(new KeyboardState(Keys.Enter), Pad(Vector2.Zero));

        var result = _sut.IsPressed(GameAction.Confirm);

        Assert.True(result);
    }

    [Fact]
    public void IsPressed_KeyHeld_ReturnsFalse()
    {
        _sut.Update(new KeyboardState(Keys.Enter), Pad(Vector2.Zero));
        _sut.Update(new KeyboardState(Keys.Enter), Pad(Vector2.Zero));

        var result = _sut.IsPressed(GameAction.Confirm);

        Assert.False(result);
    }

    [Fact]
    public void IsPressed_KeyHeldThenButtonPressed_ReturnsFalse()
    {
        _sut.Update(new KeyboardState(Keys.Z), Pad(Vector2.Zero));
        _sut.Update(new KeyboardState(Keys.Z), Pad(Vector2.Zero, Buttons.A));

        var result = _sut.IsPressed(GameAction.Confirm);

        Assert.False(result);
    }
}
