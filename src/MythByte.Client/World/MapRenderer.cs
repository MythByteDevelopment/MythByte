using Microsoft.Xna.Framework;
using MythByte.Client.Rendering;
using MythByte.Shared.Maps;

namespace MythByte.Client.World;

public static class MapRenderer
{
    public static void Draw(ICanvas canvas, TileMap map, Point camera)
    {
        var extraColumns = ExtraCells(map.Tilesets.Select(t => t.TileWidth), map.TileWidth);
        var extraRows = ExtraCells(map.Tilesets.Select(t => t.TileHeight), map.TileHeight);

        var firstX = Math.Max(0, FloorDiv(camera.X, map.TileWidth) - extraColumns);
        var firstY = Math.Max(0, FloorDiv(camera.Y, map.TileHeight));
        var lastX = Math.Min(map.Width - 1, FloorDiv(camera.X + canvas.Width - 1, map.TileWidth));
        var lastY = Math.Min(map.Height - 1, FloorDiv(camera.Y + canvas.Height - 1, map.TileHeight) + extraRows);

        foreach (var name in MapLayers.Drawn)
        {
            var layer = map.Layer(name);
            if (layer is null)
                continue;

            for (var y = firstY; y <= lastY; y++)
            {
                for (var x = firstX; x <= lastX; x++)
                {
                    var gid = layer.GidAt(x, y);
                    var tileset = map.TilesetFor(gid);
                    if (tileset is null)
                        continue;

                    var (sourceX, sourceY) = tileset.SourceOf(gid);
                    var source = new Rectangle(sourceX, sourceY, tileset.TileWidth, tileset.TileHeight);
                    var destination = new Rectangle(
                        x * map.TileWidth - camera.X,
                        (y + 1) * map.TileHeight - tileset.TileHeight - camera.Y,
                        tileset.TileWidth,
                        tileset.TileHeight);
                    canvas.Sprite(tileset.ImagePath, source, destination);
                }
            }
        }
    }

    private static int ExtraCells(IEnumerable<int> tileSizes, int cellSize)
    {
        var largest = tileSizes.DefaultIfEmpty(cellSize).Max();
        return Math.Max(0, (largest - cellSize + cellSize - 1) / cellSize);
    }

    private static int FloorDiv(int value, int divisor) => (int)Math.Floor((double)value / divisor);
}
