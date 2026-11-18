using Moq;
using MythByte.Client.Input;
using MythByte.Client.Screens;

namespace MythByte.UnitTests.Client;

public class PauseMenuScreenTests
{
    private readonly Mock<IScreenManager> _screens = new();
    private readonly Mock<IScreenFactory> _factory = new();
    private readonly Mock<IInputService> _input = new();
    private readonly PauseMenuScreen _sut;

    public PauseMenuScreenTests()
    {
        _sut = new PauseMenuScreen(_screens.Object, _factory.Object);
    }

    private void Press(GameAction action)
    {
        _input.Reset();
        _input.Setup(i => i.IsPressed(action)).Returns(true);
        _sut.Update(0.016f, _input.Object);
    }

    [Fact]
    public void Update_UpOnFirstItem_WrapsToLastItem()
    {
        Press(GameAction.Up);

        Assert.Equal(2, _sut.SelectedIndex);
    }

    [Fact]
    public void Update_DownOnLastItem_WrapsToFirstItem()
    {
        Press(GameAction.Down);
        Press(GameAction.Down);

        Press(GameAction.Down);

        Assert.Equal(0, _sut.SelectedIndex);
    }

    [Fact]
    public void Update_Back_PopsMenu()
    {
        Press(GameAction.Back);

        _screens.Verify(s => s.Pop(), Times.Once);
    }

    [Fact]
    public void Update_ConfirmOnResume_PopsMenu()
    {
        Press(GameAction.Confirm);

        _screens.Verify(s => s.Pop(), Times.Once);
    }

    [Fact]
    public void Update_ConfirmOnTitleScreen_ReplacesWithTitle()
    {
        var title = new Mock<IScreen>().Object;
        _factory.Setup(f => f.CreateTitle()).Returns(title);
        Press(GameAction.Down);

        Press(GameAction.Confirm);

        _screens.Verify(s => s.Replace(title), Times.Once);
    }

    [Fact]
    public void Update_ConfirmOnQuit_RequestsExit()
    {
        Press(GameAction.Up);

        Press(GameAction.Confirm);

        _screens.Verify(s => s.RequestExit(), Times.Once);
    }
}
