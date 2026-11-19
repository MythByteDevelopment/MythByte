using Microsoft.Xna.Framework;

namespace MythByte.Client.Rendering;

public interface ICanvas
{
    int Width { get; }
    int Height { get; }
    void Fill(Rectangle area, Color color);
    void Sprite(string texturePath, Rectangle source, Rectangle destination);
    void Text(string text, Vector2 position, int scale, Color color);
    void TextCentered(string text, int y, int scale, Color color);
}
