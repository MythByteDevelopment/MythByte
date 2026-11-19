using MythByte.Shared.Maps;

namespace MythByte.UnitTests.Shared;

public class TiledMapReaderTests : IDisposable
{
    private const string ExternalTileset = """{ "firstgid": 1, "source": "../Tilesets/tiles.tsj" }""";
    private const string Ground = """{ "type": "tilelayer", "name": "Ground", "width": 3, "height": 2, "data": [1, 2, 3, 4, 5, 6] }""";

    private readonly string _directory = Path.Combine(Path.GetTempPath(), "mythbyte-maps-" + Guid.NewGuid());

    public TiledMapReaderTests()
    {
        Directory.CreateDirectory(Path.Combine(_directory, "Maps"));
        Directory.CreateDirectory(Path.Combine(_directory, "Tilesets"));
        File.WriteAllText(Path.Combine(_directory, "Tilesets", "tiles.tsj"),
            """{ "type": "tileset", "name": "tiles", "tilewidth": 64, "tileheight": 64, "tilecount": 16, "columns": 8, "image": "tiles.png" }""");
    }

    public void Dispose() => Directory.Delete(_directory, true);

    private string WriteMap(string layers, string tileset = ExternalTileset, string orientation = "orthogonal", string infinite = "false")
    {
        var path = Path.Combine(_directory, "Maps", "map.tmj");
        File.WriteAllText(path,
            $$"""
            {
              "type": "map",
              "orientation": "{{orientation}}",
              "infinite": {{infinite}},
              "width": 3,
              "height": 2,
              "tilewidth": 64,
              "tileheight": 64,
              "properties": [
                { "name": "spawnX", "type": "int", "value": 2 },
                { "name": "spawnY", "type": "int", "value": 1 },
                { "name": "title", "type": "string", "value": "Test" }
              ],
              "tilesets": [{{tileset}}],
              "layers": [{{layers}}]
            }
            """);
        return path;
    }

    private static string Layer(string name, string data, string? encoding = null) =>
        $$"""{ "type": "tilelayer", "name": "{{name}}", "width": 3, "height": 2{{(encoding is null ? "" : $", \"encoding\": \"{encoding}\"")}}, "data": {{data}} }""";

    [Fact]
    public void Read_ValidMap_ReadsSizeLayersAndSpawn()
    {
        var path = WriteMap(Ground + "," + Layer("Collision", "[0, 0, 15, 0, 0, 0]"));

        var map = TiledMapReader.Read(path);

        Assert.Equal(3, map.Width);
        Assert.Equal(2, map.Height);
        Assert.Equal(64, map.TileWidth);
        Assert.Equal((2, 1), map.Spawn);
        Assert.Equal("Test", map.Properties["title"]);
        Assert.Equal(6, map.Layer(MapLayers.Ground)!.GidAt(2, 1));
        Assert.True(map.IsBlocked(2, 0));
    }

    [Fact]
    public void Read_ExternalTileset_ResolvesImageNextToTileset()
    {
        var path = WriteMap(Ground);

        var map = TiledMapReader.Read(path);

        var tileset = Assert.Single(map.Tilesets);
        Assert.Equal(1, tileset.FirstGid);
        Assert.Equal(8, tileset.Columns);
        Assert.Equal(Path.GetFullPath(Path.Combine(_directory, "Tilesets", "tiles.png")), tileset.ImagePath);
    }

    [Fact]
    public void Read_EmbeddedTileset_ResolvesImageNextToMap()
    {
        var path = WriteMap(Ground, """{ "firstgid": 1, "name": "inline", "tilewidth": 64, "tileheight": 64, "tilecount": 4, "columns": 2, "image": "inline.png" }""");

        var map = TiledMapReader.Read(path);

        var tileset = Assert.Single(map.Tilesets);
        Assert.Equal("inline", tileset.Name);
        Assert.Equal(Path.GetFullPath(Path.Combine(_directory, "Maps", "inline.png")), tileset.ImagePath);
    }

    [Fact]
    public void Read_FlippedTile_RemovesFlipFlags()
    {
        var path = WriteMap(Layer("Ground", $"[{2147483648u | 3u}, 1, 1, 1, 1, 1]"));

        var map = TiledMapReader.Read(path);

        Assert.Equal(3, map.Layer(MapLayers.Ground)!.GidAt(0, 0));
    }

    [Fact]
    public void Read_ObjectLayer_IsSkipped()
    {
        var path = WriteMap(Ground + """, { "type": "objectgroup", "name": "Objects", "objects": [] }""");

        var map = TiledMapReader.Read(path);

        Assert.Single(map.Layers);
    }

    [Fact]
    public void Read_Base64Layer_ThrowsInvalidDataException()
    {
        var path = WriteMap(Layer("Ground", "\"AAAA\"", "base64"));

        Assert.Throws<InvalidDataException>(() => TiledMapReader.Read(path));
    }

    [Fact]
    public void Read_WrongTileCount_ThrowsInvalidDataException()
    {
        var path = WriteMap(Layer("Ground", "[1, 1, 1]"));

        Assert.Throws<InvalidDataException>(() => TiledMapReader.Read(path));
    }

    [Fact]
    public void Read_InfiniteMap_ThrowsInvalidDataException()
    {
        var path = WriteMap(Ground, infinite: "true");

        Assert.Throws<InvalidDataException>(() => TiledMapReader.Read(path));
    }

    [Fact]
    public void Read_IsometricMap_ThrowsInvalidDataException()
    {
        var path = WriteMap(Ground, orientation: "isometric");

        Assert.Throws<InvalidDataException>(() => TiledMapReader.Read(path));
    }

    [Fact]
    public void Read_InvalidJson_ThrowsInvalidDataException()
    {
        var path = Path.Combine(_directory, "Maps", "broken.tmj");
        File.WriteAllText(path, "{ \"type\": \"map\", ");

        Assert.Throws<InvalidDataException>(() => TiledMapReader.Read(path));
    }

    [Fact]
    public void Read_MissingFile_ThrowsFileNotFoundException()
    {
        Assert.Throws<FileNotFoundException>(() => TiledMapReader.Read(Path.Combine(_directory, "missing.tmj")));
    }

    [Fact]
    public void Read_MissingTileset_ThrowsFileNotFoundException()
    {
        File.Delete(Path.Combine(_directory, "Tilesets", "tiles.tsj"));
        var path = WriteMap(Ground);

        Assert.Throws<FileNotFoundException>(() => TiledMapReader.Read(path));
    }
}
