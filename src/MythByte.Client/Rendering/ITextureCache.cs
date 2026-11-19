using Microsoft.Xna.Framework.Graphics;

namespace MythByte.Client.Rendering;

public interface ITextureCache
{
    void Initialize(GraphicsDevice graphicsDevice);
    Texture2D Get(string path);
}
