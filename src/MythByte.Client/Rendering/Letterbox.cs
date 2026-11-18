using Microsoft.Xna.Framework;

namespace MythByte.Client.Rendering;

public static class Letterbox
{
    public static Rectangle Fit(int windowWidth, int windowHeight, int virtualWidth, int virtualHeight)
    {
        if (windowWidth <= 0 || windowHeight <= 0 || virtualWidth <= 0 || virtualHeight <= 0)
            return Rectangle.Empty;

        var scale = Math.Min((float)windowWidth / virtualWidth, (float)windowHeight / virtualHeight);
        if (scale >= 1f)
            scale = MathF.Floor(scale);

        var width = (int)MathF.Round(virtualWidth * scale);
        var height = (int)MathF.Round(virtualHeight * scale);

        return new Rectangle((windowWidth - width) / 2, (windowHeight - height) / 2, width, height);
    }
}
