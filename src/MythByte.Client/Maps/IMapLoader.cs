using MythByte.Shared.Maps;

namespace MythByte.Client.Maps;

public interface IMapLoader
{
    TileMap Load(string relativePath);
}
