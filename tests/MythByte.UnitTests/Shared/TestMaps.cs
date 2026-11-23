using MythByte.Shared.Maps;

namespace MythByte.UnitTests.Shared;

public static class TestMaps
{
    public static TileMap Create(int width, int height, int[]? collision = null, int[]? ground = null, int spawnX = 0, int spawnY = 0)
    {
        var size = width * height;
        var layers = new List<TileLayer>
        {
            new(MapLayers.Ground, width, height, ground ?? Enumerable.Repeat(1, size).ToArray()),
            new(MapLayers.Collision, width, height, collision ?? new int[size])
        };
        var tilesets = new[] { new Tileset(1, "test", 64, 64, 16, 8, "tiles.png") };
        var properties = new Dictionary<string, string>
        {
            [TileMap.SpawnXProperty] = spawnX.ToString(),
            [TileMap.SpawnYProperty] = spawnY.ToString()
        };

        return new TileMap(width, height, 64, 64, tilesets, layers, properties);
    }
}
