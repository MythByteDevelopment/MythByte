using System.Text.Json;

namespace MythByte.Shared.Maps;

public static class TiledMapReader
{
    private const uint GidMask = 0x0FFFFFFF;
    private const string TileLayerType = "tilelayer";

    public static TileMap Read(string path)
    {
        if (!File.Exists(path))
            throw new FileNotFoundException($"Map file '{path}' does not exist.", path);

        var directory = Path.GetDirectoryName(Path.GetFullPath(path)) ?? string.Empty;
        var map = Deserialize(path, TiledJsonContext.Default.TiledMapJson);

        if (map.Type != "map")
            throw new InvalidDataException($"Map file '{path}' is not a Tiled map.");

        if (map.Orientation != "orthogonal")
            throw new InvalidDataException($"Map file '{path}' must use orthogonal orientation.");

        if (map.Infinite)
            throw new InvalidDataException($"Map file '{path}' must not be infinite.");

        if (map.Width <= 0 || map.Height <= 0 || map.TileWidth <= 0 || map.TileHeight <= 0)
            throw new InvalidDataException($"Map file '{path}' needs a positive size and tile size.");

        var tilesets = (map.Tilesets ?? []).Select(t => ReadTileset(t, directory, path)).ToList();
        var layers = (map.Layers ?? []).Where(l => l.Type == TileLayerType).Select(l => ReadLayer(l, path)).ToList();

        foreach (var layer in layers.Where(l => l.Width != map.Width || l.Height != map.Height))
            throw new InvalidDataException($"Layer '{layer.Name}' in '{path}' must be {map.Width} x {map.Height} tiles.");

        var properties = ReadProperties(map.Properties);
        var remapped = RemapByName(layers, tilesets, properties, path);

        return new TileMap(map.Width, map.Height, map.TileWidth, map.TileHeight, tilesets, remapped, properties);
    }

    private static List<TileLayer> RemapByName(List<TileLayer> layers, List<Tileset> tilesets, Dictionary<string, string> properties, string path)
    {
        var remaps = new Dictionary<Tileset, Dictionary<int, int?>>();
        var storedNames = new Dictionary<Tileset, Dictionary<int, string>>();

        foreach (var tileset in tilesets)
        {
            if (!properties.TryGetValue(TileNames.PropertyName(tileset.Name), out var value))
                continue;

            Dictionary<int, string> stored;
            try
            {
                stored = TileNames.Parse(value);
            }
            catch (FormatException exception)
            {
                throw new InvalidDataException($"Map '{path}' has an invalid '{TileNames.PropertyName(tileset.Name)}' property: {exception.Message}", exception);
            }

            var current = TileNames.ByName(tileset.TileNames);
            remaps[tileset] = stored.ToDictionary(s => s.Key, s => current.TryGetValue(s.Value, out var localId) ? localId : (int?)null);
            storedNames[tileset] = stored;
        }

        if (remaps.Count == 0)
            return layers;

        var missing = new List<string>();
        var result = new List<TileLayer>();

        foreach (var layer in layers)
        {
            var gids = new int[layer.Width * layer.Height];
            for (var y = 0; y < layer.Height; y++)
            {
                for (var x = 0; x < layer.Width; x++)
                {
                    var gid = layer.GidAt(x, y);
                    gids[y * layer.Width + x] = gid;

                    var tileset = OwnerOf(gid, tilesets);
                    if (tileset is null || !remaps.TryGetValue(tileset, out var remap) || !remap.TryGetValue(gid - tileset.FirstGid, out var localId))
                        continue;

                    if (localId is null)
                    {
                        missing.Add($"tileset '{tileset.Name}', tile '{storedNames[tileset][gid - tileset.FirstGid]}', layer '{layer.Name}' at ({x}, {y})");
                        continue;
                    }

                    gids[y * layer.Width + x] = tileset.FirstGid + localId.Value;
                }
            }

            result.Add(new TileLayer(layer.Name, layer.Width, layer.Height, gids));
        }

        if (missing.Count > 0)
            throw new InvalidDataException($"Map '{path}' uses tiles whose name no longer exists in their tileset: {string.Join("; ", missing)}.");

        return result;
    }

    private static Tileset? OwnerOf(int gid, List<Tileset> tilesets) =>
        gid <= 0 ? null : tilesets.Where(t => t.FirstGid <= gid).MaxBy(t => t.FirstGid);

    private static Tileset ReadTileset(TiledTilesetJson reference, string directory, string path)
    {
        var tileset = reference;
        var tilesetDirectory = directory;

        if (reference.Source is not null)
        {
            var tilesetPath = Path.GetFullPath(Path.Combine(directory, reference.Source));
            if (!File.Exists(tilesetPath))
                throw new FileNotFoundException($"Tileset '{reference.Source}' used by '{path}' does not exist.", tilesetPath);

            tileset = Deserialize(tilesetPath, TiledJsonContext.Default.TiledTilesetJson);
            tilesetDirectory = Path.GetDirectoryName(tilesetPath) ?? directory;
        }

        if (string.IsNullOrWhiteSpace(tileset.Image))
            throw new InvalidDataException($"Tileset with first gid {reference.FirstGid} in '{path}' has no image.");

        if (tileset.TileWidth <= 0 || tileset.TileHeight <= 0 || tileset.TileCount <= 0 || tileset.Columns <= 0)
            throw new InvalidDataException($"Tileset with first gid {reference.FirstGid} in '{path}' needs a tile size, tile count and columns.");

        return new Tileset(
            reference.FirstGid,
            tileset.Name ?? string.Empty,
            tileset.TileWidth,
            tileset.TileHeight,
            tileset.TileCount,
            tileset.Columns,
            Path.GetFullPath(Path.Combine(tilesetDirectory, tileset.Image)),
            ReadTileNames(tileset.Tiles));
    }

    private static Dictionary<int, string> ReadTileNames(List<TiledTileJson>? tiles) =>
        (tiles ?? [])
            .Select(t => (t.Id, Name: t.Properties?.FirstOrDefault(p => p.Name == TileNames.TileProperty)))
            .Where(t => t.Name is { Value.ValueKind: JsonValueKind.String })
            .Select(t => (t.Id, Name: t.Name!.Value.GetString() ?? string.Empty))
            .Where(t => t.Name.Length > 0)
            .ToDictionary(t => t.Id, t => t.Name);

    private static TileLayer ReadLayer(TiledLayerJson layer, string path)
    {
        var name = layer.Name ?? string.Empty;

        if (layer.Encoding is not null && layer.Encoding != "csv")
            throw new InvalidDataException($"Layer '{name}' in '{path}' must store its tiles as a plain array (CSV layer format).");

        var data = layer.Data ?? throw new InvalidDataException($"Layer '{name}' in '{path}' has no data.");

        if (data.Count != layer.Width * layer.Height)
            throw new InvalidDataException($"Layer '{name}' in '{path}' has {data.Count} tiles instead of {layer.Width * layer.Height}.");

        return new TileLayer(name, layer.Width, layer.Height, data.Select(g => (int)(g & GidMask)).ToArray());
    }

    private static Dictionary<string, string> ReadProperties(List<TiledPropertyJson>? properties) =>
        (properties ?? [])
            .Where(p => !string.IsNullOrEmpty(p.Name))
            .ToDictionary(p => p.Name!, p => p.Value.ValueKind == JsonValueKind.String ? p.Value.GetString() ?? string.Empty : p.Value.GetRawText());

    private static T Deserialize<T>(string path, System.Text.Json.Serialization.Metadata.JsonTypeInfo<T> typeInfo)
    {
        try
        {
            using var stream = File.OpenRead(path);
            return JsonSerializer.Deserialize(stream, typeInfo) ?? throw new InvalidDataException($"'{path}' is empty.");
        }
        catch (JsonException exception)
        {
            throw new InvalidDataException($"'{path}' is not valid Tiled JSON: {exception.Message}", exception);
        }
    }
}
