using System.Text.Json.Nodes;

namespace MythByte.Shared.Maps;

public static class TileNameSync
{
    private const uint GidMask = 0x0FFFFFFF;

    public static TileNameSyncResult Sync(string path, bool rewriteIds)
    {
        var map = TiledMapReader.Read(path);
        var original = File.ReadAllText(path);
        var root = JsonNode.Parse(original) as JsonObject ?? throw new InvalidDataException($"Map file '{path}' is not a Tiled map.");

        var idsOutdated = UpdateIds(root, map, rewriteIds);
        if (idsOutdated && !rewriteIds)
            return new TileNameSyncResult(path, IdsOutdated: true, Changed: false);

        UpdateNameProperties(root, map);

        var updated = TiledJsonWriter.Write(root);
        if (original.Contains("\r\n"))
            updated = updated.Replace("\n", "\r\n");

        var changed = updated != original;
        if (changed)
            File.WriteAllText(path, updated);

        return new TileNameSyncResult(path, idsOutdated, changed);
    }

    public static Dictionary<string, string> NameProperties(TileMap map)
    {
        var properties = new Dictionary<string, string>();

        foreach (var tileset in map.Tilesets.Where(t => t.TileNames is { Count: > 0 }))
        {
            var used = map.Layers
                .SelectMany(l => Enumerable.Range(0, l.Width * l.Height).Select(i => l.GidAt(i % l.Width, i / l.Width)))
                .Where(gid => map.TilesetFor(gid) == tileset)
                .Select(gid => gid - tileset.FirstGid)
                .Distinct()
                .Where(id => tileset.TileNames!.ContainsKey(id))
                .ToDictionary(id => id, id => tileset.TileNames![id]);

            if (used.Count > 0)
                properties[TileNames.PropertyName(tileset.Name)] = TileNames.Format(used);
        }

        return properties;
    }

    private static bool UpdateIds(JsonObject root, TileMap map, bool rewriteIds)
    {
        var rawLayers = (root["layers"] as JsonArray ?? new JsonArray())
            .OfType<JsonObject>()
            .Where(l => l["type"]?.GetValue<string>() == "tilelayer")
            .ToList();

        var outdated = false;
        for (var layerIndex = 0; layerIndex < rawLayers.Count && layerIndex < map.Layers.Count; layerIndex++)
        {
            if (rawLayers[layerIndex]["data"] is not JsonArray data)
                continue;

            var layer = map.Layers[layerIndex];
            for (var i = 0; i < data.Count; i++)
            {
                var raw = data[i]!.GetValue<uint>();
                var gid = layer.GidAt(i % layer.Width, i / layer.Width);
                if ((int)(raw & GidMask) == gid)
                    continue;

                outdated = true;
                if (rewriteIds)
                    data[i] = (raw & ~GidMask) | (uint)gid;
            }
        }

        return outdated;
    }

    private static void UpdateNameProperties(JsonObject root, TileMap map)
    {
        var existing = root["properties"] as JsonArray ?? new JsonArray();
        var kept = existing
            .OfType<JsonObject>()
            .Where(p => p["name"]?.GetValue<string>() is not { } name || !name.StartsWith(TileNames.PropertyPrefix, StringComparison.Ordinal))
            .Select(p => p.DeepClone());

        var added = NameProperties(map).Select(p => (JsonNode)new JsonObject
        {
            ["name"] = p.Key,
            ["type"] = "string",
            ["value"] = p.Value
        });

        var properties = kept
            .Concat(added)
            .OrderBy(p => p["name"]?.GetValue<string>(), StringComparer.Ordinal)
            .ToArray();

        if (properties.Length == 0 && root["properties"] is null)
            return;

        root["properties"] = new JsonArray(properties);
    }
}

public sealed record TileNameSyncResult(string Path, bool IdsOutdated, bool Changed);
