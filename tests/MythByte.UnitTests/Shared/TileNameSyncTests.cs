using MythByte.Shared.Maps;

namespace MythByte.UnitTests.Shared;

public class TileNameSyncTests : IDisposable
{
    private readonly string _directory = Path.Combine(Path.GetTempPath(), "mythbyte-sync-" + Guid.NewGuid());

    public TileNameSyncTests()
    {
        Directory.CreateDirectory(Path.Combine(_directory, "Maps"));
        Directory.CreateDirectory(Path.Combine(_directory, "Tilesets"));
    }

    public void Dispose() => Directory.Delete(_directory, true);

    private void WriteTileset(params string[] names)
    {
        var tiles = string.Join(", ", names.Select((name, id) => $$"""{ "id": {{id}}, "properties": [{ "name": "name", "type": "string", "value": "{{name}}" }] }"""));
        File.WriteAllText(Path.Combine(_directory, "Tilesets", "tiles.tsj"),
            $$"""{ "type": "tileset", "name": "tiles", "tilewidth": 64, "tileheight": 64, "tilecount": 16, "columns": 8, "image": "tiles.png", "tiles": [{{tiles}}] }""");
    }

    private string WriteMap(string data, string tileNames = "")
    {
        var property = tileNames.Length == 0 ? "" : $$""", { "name": "tileNames:tiles", "type": "string", "value": "{{tileNames}}" }""";
        var path = Path.Combine(_directory, "Maps", "map.tmj");
        File.WriteAllText(path,
            $$"""
            {
             "type": "map",
             "orientation": "orthogonal",
             "infinite": false,
             "width": 3,
             "height": 2,
             "tilewidth": 64,
             "tileheight": 64,
             "properties": [{ "name": "spawnX", "type": "int", "value": 2 }{{property}}],
             "tilesets": [{ "firstgid": 1, "source": "../Tilesets/tiles.tsj" }],
             "layers": [{ "type": "tilelayer", "name": "Ground", "width": 3, "height": 2, "data": {{data}} }]
            }
            """);
        return path;
    }

    [Fact]
    public void Sync_MapWithoutTileNames_WritesNamesOfUsedTiles()
    {
        WriteTileset("grass", "path", "water");
        var path = WriteMap("[1, 1, 2, 2, 1, 1]");

        var result = TileNameSync.Sync(path, rewriteIds: false);

        Assert.True(result.Changed);
        Assert.False(result.IdsOutdated);
        Assert.Equal("0=grass;1=path", TiledMapReader.Read(path).Properties["tileNames:tiles"]);
    }

    [Fact]
    public void Sync_UpToDateMap_DoesNotChangeFile()
    {
        WriteTileset("grass", "path");
        var path = WriteMap("[1, 1, 2, 2, 1, 1]");
        TileNameSync.Sync(path, rewriteIds: false);
        var before = File.ReadAllText(path);

        var result = TileNameSync.Sync(path, rewriteIds: false);

        Assert.False(result.Changed);
        Assert.Equal(before, File.ReadAllText(path));
    }

    [Fact]
    public void Sync_ReorderedTilesetWithoutRewrite_ReportsOutdatedIdsAndKeepsFile()
    {
        WriteTileset("path", "grass");
        var path = WriteMap("[1, 1, 2, 2, 1, 1]", "0=grass;1=path");
        var before = File.ReadAllText(path);

        var result = TileNameSync.Sync(path, rewriteIds: false);

        Assert.True(result.IdsOutdated);
        Assert.False(result.Changed);
        Assert.Equal(before, File.ReadAllText(path));
    }

    [Fact]
    public void Sync_ReorderedTilesetWithRewrite_WritesNewIdsAndNames()
    {
        WriteTileset("path", "grass");
        var path = WriteMap("[1, 1, 2, 2, 1, 1]", "0=grass;1=path");

        var result = TileNameSync.Sync(path, rewriteIds: true);

        Assert.True(result.Changed);
        var map = TiledMapReader.Read(path);
        Assert.Equal("0=path;1=grass", map.Properties["tileNames:tiles"]);
        Assert.Equal(2, map.Layer(MapLayers.Ground)!.GidAt(0, 0));
        Assert.Equal(1, map.Layer(MapLayers.Ground)!.GidAt(2, 0));
    }

    [Fact]
    public void Sync_ReorderedTilesetWithRewrite_KeepsFlipFlags()
    {
        WriteTileset("path", "grass");
        var path = WriteMap($"[{2147483648u | 1u}, 1, 2, 2, 1, 1]", "0=grass;1=path");

        TileNameSync.Sync(path, rewriteIds: true);

        Assert.Contains((2147483648u | 2u).ToString(), File.ReadAllText(path));
    }

    [Fact]
    public void Sync_DeletedTile_ThrowsInvalidDataException()
    {
        WriteTileset("path");
        var path = WriteMap("[1, 1, 2, 2, 1, 1]", "0=path;1=well");

        Assert.Throws<InvalidDataException>(() => TileNameSync.Sync(path, rewriteIds: true));
    }
}
