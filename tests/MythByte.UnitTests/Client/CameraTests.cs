using Microsoft.Xna.Framework;
using MythByte.Client.World;

namespace MythByte.UnitTests.Client;

public class CameraTests
{
    private static readonly Point View = new(1920, 1080);
    private static readonly Point Map = new(3072, 2048);

    [Fact]
    public void TopLeft_FocusInMiddle_CentersOnFocus()
    {
        var topLeft = Camera.TopLeft(new Point(1500, 1000), Map, View);

        Assert.Equal(new Point(540, 460), topLeft);
    }

    [Fact]
    public void TopLeft_FocusNearTopLeft_ClampsToZero()
    {
        var topLeft = Camera.TopLeft(new Point(32, 32), Map, View);

        Assert.Equal(Point.Zero, topLeft);
    }

    [Fact]
    public void TopLeft_FocusNearBottomRight_ClampsToMapEdge()
    {
        var topLeft = Camera.TopLeft(new Point(3040, 2016), Map, View);

        Assert.Equal(new Point(1152, 968), topLeft);
    }

    [Fact]
    public void TopLeft_MapSmallerThanView_CentersMap()
    {
        var topLeft = Camera.TopLeft(new Point(100, 100), new Point(640, 480), View);

        Assert.Equal(new Point(-640, -300), topLeft);
    }

    [Fact]
    public void TopLeft_MapExactlyViewSize_ReturnsZero()
    {
        var topLeft = Camera.TopLeft(new Point(1900, 1000), View, View);

        Assert.Equal(Point.Zero, topLeft);
    }
}
