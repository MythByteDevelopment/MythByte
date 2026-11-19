namespace MythByte.Shared.Maps;

public sealed class TileMap(
    int width,
    int height,
    int tileWidth,
    int tileHeight,
    IReadOnlyList<Tileset> tilesets,
    IReadOnlyList<TileLayer> layers,
    IReadOnlyDictionary<string, string> properties)
{
    public const string SpawnXProperty = "spawnX";
    public const string SpawnYProperty = "spawnY";

    public int Width { get; } = width;
    public int Height { get; } = height;
    public int TileWidth { get; } = tileWidth;
    public int TileHeight { get; } = tileHeight;
    public int PixelWidth => Width * TileWidth;
    public int PixelHeight => Height * TileHeight;
    public IReadOnlyList<Tileset> Tilesets { get; } = tilesets;
    public IReadOnlyList<TileLayer> Layers { get; } = layers;
    public IReadOnlyDictionary<string, string> Properties { get; } = properties;

    public (int X, int Y) Spawn => (IntProperty(SpawnXProperty), IntProperty(SpawnYProperty));

    public TileLayer? Layer(string name) => Layers.FirstOrDefault(l => l.Name == name);

    public bool IsInside(int x, int y) => x >= 0 && y >= 0 && x < Width && y < Height;

    public bool IsBlocked(int x, int y) => !IsInside(x, y) || (Layer(MapLayers.Collision)?.GidAt(x, y) ?? 0) != 0;

    public bool IsEncounterTile(int x, int y) => IsInside(x, y) && (Layer(MapLayers.Encounter)?.GidAt(x, y) ?? 0) != 0;

    public Tileset? TilesetFor(int gid)
    {
        if (gid <= 0)
            return null;

        return Tilesets.Where(t => t.FirstGid <= gid).MaxBy(t => t.FirstGid) is { } tileset && tileset.Contains(gid) ? tileset : null;
    }

    private int IntProperty(string name) => Properties.TryGetValue(name, out var value) && int.TryParse(value, out var number) ? number : 0;
}
