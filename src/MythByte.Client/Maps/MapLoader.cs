using MythByte.Shared.Maps;

namespace MythByte.Client.Maps;

public class MapLoader : IMapLoader
{
    public TileMap Load(string relativePath) => TiledMapReader.Read(Path.Combine(AppContext.BaseDirectory, relativePath));
}
