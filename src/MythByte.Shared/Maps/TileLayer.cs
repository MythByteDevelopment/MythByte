namespace MythByte.Shared.Maps;

public sealed class TileLayer(string name, int width, int height, int[] gids)
{
    public string Name { get; } = name;
    public int Width { get; } = width;
    public int Height { get; } = height;

    public int GidAt(int x, int y)
    {
        if (x < 0 || y < 0 || x >= Width || y >= Height)
            return 0;

        return gids[y * Width + x];
    }
}
