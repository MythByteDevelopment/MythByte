using MythByte.Shared.Maps;

namespace MythByte.UnitTests.Client;

public class StartMapTests
{
    private static readonly string StartMapPath = Path.Combine(AppContext.BaseDirectory, "Content", "Maps", "start.tmj");

    [Fact]
    public void StartMap_InClientContent_HasAllLayers()
    {
        var map = TiledMapReader.Read(StartMapPath);

        Assert.NotNull(map.Layer(MapLayers.Ground));
        Assert.NotNull(map.Layer(MapLayers.Decoration));
        Assert.NotNull(map.Layer(MapLayers.Collision));
        Assert.NotNull(map.Layer(MapLayers.Encounter));
        Assert.Equal(64, map.TileWidth);
    }

    [Fact]
    public void StartMap_InClientContent_SpawnIsWalkable()
    {
        var map = TiledMapReader.Read(StartMapPath);
        var (x, y) = map.Spawn;

        Assert.False(map.IsBlocked(x, y));
    }

    [Fact]
    public void StartMap_InClientContent_TilesetImageExists()
    {
        var map = TiledMapReader.Read(StartMapPath);

        Assert.All(map.Tilesets, t => Assert.True(File.Exists(t.ImagePath)));
    }
}
