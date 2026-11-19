using MythByte.Shared.Maps;

namespace MythByte.UnitTests.Shared;

public class TileMapTests
{
    [Fact]
    public void IsBlocked_CollisionTile_ReturnsTrue()
    {
        var map = TestMaps.Create(2, 2, collision: [0, 15, 0, 0]);

        var blocked = map.IsBlocked(1, 0);

        Assert.True(blocked);
    }

    [Fact]
    public void IsBlocked_EmptyCollisionTile_ReturnsFalse()
    {
        var map = TestMaps.Create(2, 2, collision: [0, 15, 0, 0]);

        var blocked = map.IsBlocked(0, 1);

        Assert.False(blocked);
    }

    [Theory]
    [InlineData(-1, 0)]
    [InlineData(0, -1)]
    [InlineData(2, 0)]
    [InlineData(0, 2)]
    public void IsBlocked_OutsideMap_ReturnsTrue(int x, int y)
    {
        var map = TestMaps.Create(2, 2);

        var blocked = map.IsBlocked(x, y);

        Assert.True(blocked);
    }

    [Fact]
    public void IsBlocked_NoCollisionLayer_ReturnsFalseInsideMap()
    {
        var map = new TileMap(2, 2, 64, 64, [], [], new Dictionary<string, string>());

        var blocked = map.IsBlocked(1, 1);

        Assert.False(blocked);
    }

    [Fact]
    public void TilesetFor_SecondTileset_PicksHighestFirstGidBelowGid()
    {
        var first = new Tileset(1, "a", 64, 64, 10, 5, "a.png");
        var second = new Tileset(11, "b", 64, 64, 10, 5, "b.png");
        var map = new TileMap(1, 1, 64, 64, [first, second], [], new Dictionary<string, string>());

        var tileset = map.TilesetFor(12);

        Assert.Same(second, tileset);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(17)]
    public void TilesetFor_EmptyOrUnknownGid_ReturnsNull(int gid)
    {
        var map = TestMaps.Create(1, 1);

        var tileset = map.TilesetFor(gid);

        Assert.Null(tileset);
    }

    [Theory]
    [InlineData(1, 0, 0)]
    [InlineData(8, 448, 0)]
    [InlineData(9, 0, 64)]
    [InlineData(16, 448, 64)]
    public void SourceOf_Gid_ReturnsPixelPositionInImage(int gid, int expectedX, int expectedY)
    {
        var tileset = new Tileset(1, "test", 64, 64, 16, 8, "tiles.png");

        var source = tileset.SourceOf(gid);

        Assert.Equal((expectedX, expectedY), source);
    }

    [Fact]
    public void Spawn_NoProperties_ReturnsOrigin()
    {
        var map = new TileMap(1, 1, 64, 64, [], [], new Dictionary<string, string>());

        var spawn = map.Spawn;

        Assert.Equal((0, 0), spawn);
    }
}
