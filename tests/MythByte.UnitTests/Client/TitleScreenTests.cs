using Moq;
using MythByte.Client.Input;
using MythByte.Client.Screens;

namespace MythByte.UnitTests.Client;

public class TitleScreenTests
{
    private readonly Mock<IScreenManager> _screens = new();
    private readonly Mock<IScreenFactory> _factory = new();
    private readonly Mock<IInputService> _input = new();
    private readonly TitleScreen _sut;

    public TitleScreenTests()
    {
        _sut = new TitleScreen(_screens.Object, _factory.Object);
    }

    [Fact]
    public void Update_Confirm_ReplacesWithWorld()
    {
        var world = new Mock<IScreen>().Object;
        _factory.Setup(f => f.CreateWorld()).Returns(world);
        _input.Setup(i => i.IsPressed(GameAction.Confirm)).Returns(true);

        _sut.Update(0.016f, _input.Object);

        _screens.Verify(s => s.Replace(world), Times.Once);
    }

    [Fact]
    public void Update_Back_RequestsExit()
    {
        _input.Setup(i => i.IsPressed(GameAction.Back)).Returns(true);

        _sut.Update(0.016f, _input.Object);

        _screens.Verify(s => s.RequestExit(), Times.Once);
    }

    [Fact]
    public void Update_NoInput_StaysOnTitle()
    {
        _sut.Update(0.016f, _input.Object);

        _screens.Verify(s => s.Replace(It.IsAny<IScreen>()), Times.Never);
        _screens.Verify(s => s.RequestExit(), Times.Never);
    }
}
