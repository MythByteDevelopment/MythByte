using Microsoft.Xna.Framework;
using MythByte.Client.Rendering;

namespace MythByte.UnitTests.Client;

public class LetterboxTests
{
    [Fact]
    public void Fit_WindowSameAsVirtual_FillsWindow()
    {
        var result = Letterbox.Fit(1920, 1080, 1920, 1080);

        Assert.Equal(new Rectangle(0, 0, 1920, 1080), result);
    }

    [Fact]
    public void Fit_SmallerSixteenByNine_ScalesDownToFill()
    {
        var result = Letterbox.Fit(1280, 720, 1920, 1080);

        Assert.Equal(new Rectangle(0, 0, 1280, 720), result);
    }

    [Fact]
    public void Fit_TwiceTheSize_UsesScaleTwo()
    {
        var result = Letterbox.Fit(3840, 2160, 1920, 1080);

        Assert.Equal(new Rectangle(0, 0, 3840, 2160), result);
    }

    [Fact]
    public void Fit_SlightlyLargerWindow_KeepsWholeNumberScaleAndCenters()
    {
        var result = Letterbox.Fit(2000, 1200, 1920, 1080);

        Assert.Equal(new Rectangle(40, 60, 1920, 1080), result);
    }

    [Fact]
    public void Fit_WiderWindow_AddsBarsLeftAndRight()
    {
        var result = Letterbox.Fit(1600, 600, 1920, 1080);

        Assert.Equal(600, result.Height);
        Assert.Equal((1600 - result.Width) / 2, result.X);
        Assert.Equal(0, result.Y);
    }

    [Fact]
    public void Fit_TallerWindow_AddsBarsTopAndBottom()
    {
        var result = Letterbox.Fit(960, 1000, 1920, 1080);

        Assert.Equal(new Rectangle(0, 230, 960, 540), result);
    }

    [Theory]
    [InlineData(0, 720)]
    [InlineData(1280, 0)]
    [InlineData(-1, -1)]
    public void Fit_EmptyWindow_ReturnsEmpty(int width, int height)
    {
        var result = Letterbox.Fit(width, height, 1920, 1080);

        Assert.Equal(Rectangle.Empty, result);
    }
}
