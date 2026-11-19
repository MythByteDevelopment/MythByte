using Microsoft.Xna.Framework;
using Moq;
using MythByte.Client.Input;
using MythByte.Client.Maps;
using MythByte.Client.Options;
using MythByte.Client.Screens;
using MythByte.UnitTests.Shared;

namespace MythByte.UnitTests.Client;

public class WorldScreenTests
{
    private readonly Mock<IScreenManager> _screens = new();
    private readonly Mock<IScreenFactory> _factory = new();
    private readonly Mock<IMapLoader> _maps = new();
    private readonly Mock<IInputService> _input = new();
    private readonly WorldScreen _sut;

    public WorldScreenTests()
    {
        var collision = new int[5 * 5];
        collision[1 * 5 + 2] = 15;
        _maps.Setup(m => m.Load("Content/Maps/test.tmj")).Returns(TestMaps.Create(5, 5, collision: collision, spawnX: 2, spawnY: 2));
        var options = Microsoft.Extensions.Options.Options.Create(new ClientOptions
        {
            WindowWidth = 1280,
            WindowHeight = 720,
            Fullscreen = false,
            TargetFramesPerSecond = 60,
            StartMap = "Content/Maps/test.tmj"
        });
        _sut = new WorldScreen(_screens.Object, _factory.Object, _maps.Object, options);
    }

    private void Press(GameAction action)
    {
        _input.Reset();
        _input.Setup(i => i.IsPressed(action)).Returns(true);
        _sut.Update(0.016f, _input.Object);
    }

    [Fact]
    public void Constructor_StartMap_PlacesPlayerOnSpawn()
    {
        Assert.Equal((2, 2), (_sut.Player.X, _sut.Player.Y));
    }

    [Fact]
    public void Update_RightOnFreeTile_MovesPlayer()
    {
        Press(GameAction.Right);

        Assert.Equal((3, 2), (_sut.Player.X, _sut.Player.Y));
    }

    [Fact]
    public void Update_UpIntoCollision_KeepsPlayerInPlace()
    {
        Press(GameAction.Up);

        Assert.Equal((2, 2), (_sut.Player.X, _sut.Player.Y));
    }

    [Fact]
    public void Update_Back_OpensPauseMenu()
    {
        var menu = new Mock<IScreen>().Object;
        _factory.Setup(f => f.CreatePauseMenu()).Returns(menu);

        Press(GameAction.Back);

        _screens.Verify(s => s.Push(menu), Times.Once);
    }

    [Fact]
    public void CameraFor_MapSmallerThanView_CentersMap()
    {
        var camera = _sut.CameraFor(1920, 1080);

        Assert.Equal(new Point(-800, -380), camera);
    }
}
