using Microsoft.Xna.Framework;

namespace MythByte.Client.World;

public static class Camera
{
    public static Point TopLeft(Point focus, Point mapSize, Point viewSize) =>
        new(Axis(focus.X, mapSize.X, viewSize.X), Axis(focus.Y, mapSize.Y, viewSize.Y));

    private static int Axis(int focus, int mapSize, int viewSize)
    {
        if (mapSize <= viewSize)
            return -(viewSize - mapSize) / 2;

        return Math.Clamp(focus - viewSize / 2, 0, mapSize - viewSize);
    }
}
