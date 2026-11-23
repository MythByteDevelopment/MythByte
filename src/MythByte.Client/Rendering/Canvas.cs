using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace MythByte.Client.Rendering;

public sealed class Canvas : ICanvas, IDisposable
{
    private readonly SpriteBatch _spriteBatch;
    private readonly Texture2D _pixel;
    private readonly ITextureCache _textures;

    public Canvas(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, ITextureCache textures)
    {
        _spriteBatch = spriteBatch;
        _textures = textures;
        _pixel = new Texture2D(graphicsDevice, 1, 1);
        _pixel.SetData([Color.White]);
    }

    public int Width => VirtualScreen.Width;
    public int Height => VirtualScreen.Height;

    public void Fill(Rectangle area, Color color) => _spriteBatch.Draw(_pixel, area, color);

    public void Sprite(string texturePath, Rectangle source, Rectangle destination) =>
        _spriteBatch.Draw(_textures.Get(texturePath), destination, source, Color.White);

    public void Text(string text, Vector2 position, int scale, Color color)
    {
        var x = (int)position.X;
        var y = (int)position.Y;

        foreach (var character in text)
        {
            var glyph = PixelFont.GetGlyph(character);
            for (var row = 0; row < PixelFont.GlyphHeight; row++)
            {
                for (var column = 0; column < PixelFont.GlyphWidth; column++)
                {
                    if (glyph[row][column] == '#')
                        Fill(new Rectangle(x + column * scale, y + row * scale, scale, scale), color);
                }
            }

            x += (PixelFont.GlyphWidth + PixelFont.Spacing) * scale;
        }
    }

    public void TextCentered(string text, int y, int scale, Color color)
    {
        var x = (Width - PixelFont.MeasureWidth(text, scale)) / 2;
        Text(text, new Vector2(x, y), scale, color);
    }

    public void Dispose() => _pixel.Dispose();
}
