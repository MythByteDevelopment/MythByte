using Microsoft.Xna.Framework;
using MythByte.Shared.Maps;

namespace MythByte.Client.World;

public class LocalPlayer(int x, int y)
{
    public int X { get; private set; } = x;
    public int Y { get; private set; } = y;

    public Point Tile => new(X, Y);

    public bool TryStep(int dx, int dy, TileMap map)
    {
        if (map.IsBlocked(X + dx, Y + dy))
            return false;

        X += dx;
        Y += dy;
        return true;
    }
}
