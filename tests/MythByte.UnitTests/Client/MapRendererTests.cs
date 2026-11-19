using Microsoft.Xna.Framework;
using Moq;
using MythByte.Client.Rendering;
using MythByte.Client.World;
using MythByte.Shared.Maps;
using MythByte.UnitTests.Shared;

namespace MythByte.UnitTests.Client;

public class MapRendererTests
{
    private readonly Mock<ICanvas> _canvas = new();
    private readonly List<(Rectangle Source, Rectangle Destination)> _sprites = [];

    public MapRendererTests()
    {
        _canvas.SetupGet(c => c.Width).Returns(128);
        _canvas.SetupGet(c => c.Height).Returns(64);
        _canvas.Setup(c => c.Sprite(It.IsAny<string>(), It.IsAny<Rectangle>(), It.IsAny<Rectangle>()))
            .Callback<string, Rectangle, Rectangle>((_, source, destination) => _sprites.Add((source, destination)));
    }

    [Fact]
    public void Draw_CameraAtOrigin_DrawsOnlyVisibleTiles()
    {
        var map = TestMaps.Create(4, 4);

        MapRenderer.Draw(_canvas.Object, map, Point.Zero);

        Assert.Equal(2, _sprites.Count);
    }

    [Fact]
    public void Draw_CameraBetweenTiles_DrawsPartlyVisibleTilesWithOffset()
    {
        var map = TestMaps.Create(4, 4);

        MapRenderer.Draw(_canvas.Object, map, new Point(32, 0));

        Assert.Equal(3, _sprites.Count);
        Assert.Equal(new Rectangle(-32, 0, 64, 64), _sprites[0].Destination);
    }

    [Fact]
    public void Draw_EmptyGid_SkipsTile()
    {
        var map = TestMaps.Create(2, 1, ground: [0, 9]);

        MapRenderer.Draw(_canvas.Object, map, Point.Zero);

        var sprite = Assert.Single(_sprites);
        Assert.Equal(new Rectangle(0, 64, 64, 64), sprite.Source);
        Assert.Equal(new Rectangle(64, 0, 64, 64), sprite.Destination);
    }

    [Fact]
    public void Draw_CollisionLayer_IsNotDrawn()
    {
        var map = TestMaps.Create(1, 1, collision: [15], ground: [0]);

        MapRenderer.Draw(_canvas.Object, map, Point.Zero);

        Assert.Empty(_sprites);
    }

    [Fact]
    public void Draw_DecorationLayer_IsDrawnAfterGround()
    {
        var layers = new List<TileLayer>
        {
            new(MapLayers.Decoration, 1, 1, [9]),
            new(MapLayers.Ground, 1, 1, [1])
        };
        var map = new TileMap(1, 1, 64, 64, [new Tileset(1, "test", 64, 64, 16, 8, "tiles.png")], layers, new Dictionary<string, string>());

        MapRenderer.Draw(_canvas.Object, map, Point.Zero);

        Assert.Equal(new Rectangle(0, 0, 64, 64), _sprites[0].Source);
        Assert.Equal(new Rectangle(0, 64, 64, 64), _sprites[1].Source);
    }
}
