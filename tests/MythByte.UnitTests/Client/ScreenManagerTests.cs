using Microsoft.Xna.Framework;
using Moq;
using MythByte.Client.Input;
using MythByte.Client.Rendering;
using MythByte.Client.Screens;

namespace MythByte.UnitTests.Client;

public class ScreenManagerTests
{
    private readonly ScreenManager _sut = new();
    private readonly Mock<IInputService> _input = new();

    private static Mock<IScreen> Screen(bool overlay = false)
    {
        var screen = new Mock<IScreen>();
        screen.Setup(s => s.IsOverlay).Returns(overlay);
        return screen;
    }

    [Fact]
    public void Replace_EmptyStack_ShowsScreenAndFadesInFromBlack()
    {
        var title = Screen();

        _sut.Replace(title.Object);

        Assert.Same(title.Object, Assert.Single(_sut.Screens));
        Assert.Equal(1f, _sut.FadeAlpha);
        Assert.True(_sut.IsTransitioning);
    }

    [Fact]
    public void Update_FadeInFinished_StopsTransition()
    {
        _sut.Replace(Screen().Object);

        _sut.Update(ScreenManager.FadeSeconds, _input.Object);

        Assert.False(_sut.IsTransitioning);
        Assert.Equal(0f, _sut.FadeAlpha);
    }

    [Fact]
    public void Update_DuringTransition_DoesNotUpdateScreens()
    {
        var title = Screen();
        _sut.Replace(title.Object);

        _sut.Update(0.01f, _input.Object);

        title.Verify(s => s.Update(It.IsAny<float>(), It.IsAny<IInputService>()), Times.Never);
    }

    [Fact]
    public void Replace_ExistingScreen_SwapsOnlyAfterFadeOut()
    {
        var title = Screen();
        var world = Screen();
        _sut.Replace(title.Object);
        _sut.Update(ScreenManager.FadeSeconds, _input.Object);

        _sut.Replace(world.Object);
        _sut.Update(ScreenManager.FadeSeconds / 2, _input.Object);
        var halfway = _sut.Screens[0];
        _sut.Update(ScreenManager.FadeSeconds, _input.Object);

        Assert.Same(title.Object, halfway);
        Assert.Same(world.Object, Assert.Single(_sut.Screens));
        Assert.Equal(1f, _sut.FadeAlpha);
    }

    [Fact]
    public void Replace_WithOverlayOnTop_ClearsWholeStack()
    {
        _sut.Replace(Screen().Object);
        _sut.Update(ScreenManager.FadeSeconds, _input.Object);
        _sut.Push(Screen(overlay: true).Object);
        var title = Screen();

        _sut.Replace(title.Object);
        _sut.Update(ScreenManager.FadeSeconds, _input.Object);

        Assert.Same(title.Object, Assert.Single(_sut.Screens));
    }

    [Fact]
    public void Update_NoTransition_UpdatesOnlyTopScreen()
    {
        var world = Screen();
        var menu = Screen(overlay: true);
        _sut.Replace(world.Object);
        _sut.Update(ScreenManager.FadeSeconds, _input.Object);
        _sut.Push(menu.Object);

        _sut.Update(0.016f, _input.Object);

        menu.Verify(s => s.Update(0.016f, _input.Object), Times.Once);
        world.Verify(s => s.Update(It.IsAny<float>(), It.IsAny<IInputService>()), Times.Never);
    }

    [Fact]
    public void Draw_OverlayOnTop_DrawsScreenBelowFirst()
    {
        var order = new List<string>();
        var world = Screen();
        var menu = Screen(overlay: true);
        world.Setup(s => s.Draw(It.IsAny<ICanvas>())).Callback(() => order.Add("world"));
        menu.Setup(s => s.Draw(It.IsAny<ICanvas>())).Callback(() => order.Add("menu"));
        var canvas = new Mock<ICanvas>();
        _sut.Push(world.Object);
        _sut.Push(menu.Object);

        _sut.Draw(canvas.Object);

        Assert.Equal(new[] { "world", "menu" }, order);
    }

    [Fact]
    public void Draw_NonOverlayOnTop_SkipsScreensBelow()
    {
        var title = Screen();
        var world = Screen();
        var canvas = new Mock<ICanvas>();
        _sut.Push(title.Object);
        _sut.Push(world.Object);

        _sut.Draw(canvas.Object);

        title.Verify(s => s.Draw(It.IsAny<ICanvas>()), Times.Never);
        world.Verify(s => s.Draw(canvas.Object), Times.Once);
    }

    [Fact]
    public void Draw_DuringFade_DrawsBlackOverlay()
    {
        var canvas = new Mock<ICanvas>();
        canvas.Setup(c => c.Width).Returns(1920);
        canvas.Setup(c => c.Height).Returns(1080);
        _sut.Replace(Screen().Object);

        _sut.Draw(canvas.Object);

        canvas.Verify(c => c.Fill(new Rectangle(0, 0, 1920, 1080), Color.Black * 1f), Times.Once);
    }

    [Fact]
    public void Pop_EmptyStack_DoesNothing()
    {
        _sut.Pop();

        Assert.Empty(_sut.Screens);
    }

    [Fact]
    public void RequestExit_Subscribed_RaisesExitRequested()
    {
        var raised = 0;
        _sut.ExitRequested += () => raised++;

        _sut.RequestExit();

        Assert.Equal(1, raised);
    }
}
