namespace MythByte.Shared.Maps;

public sealed record Tileset(int FirstGid, string Name, int TileWidth, int TileHeight, int TileCount, int Columns, string ImagePath, IReadOnlyDictionary<int, string>? TileNames = null)
{
    public bool Contains(int gid) => gid >= FirstGid && gid < FirstGid + TileCount;

    public (int X, int Y) SourceOf(int gid)
    {
        var localId = gid - FirstGid;
        return (localId % Columns * TileWidth, localId / Columns * TileHeight);
    }
}
