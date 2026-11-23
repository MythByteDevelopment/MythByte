using Microsoft.Xna.Framework.Graphics;

namespace MythByte.Client.Rendering;

public sealed class TextureCache : ITextureCache, IDisposable
{
    private readonly Dictionary<string, Texture2D> _textures = new(StringComparer.Ordinal);
    private GraphicsDevice? _graphicsDevice;

    public void Initialize(GraphicsDevice graphicsDevice) => _graphicsDevice = graphicsDevice;

    public Texture2D Get(string path)
    {
        if (_textures.TryGetValue(path, out var texture))
            return texture;

        if (_graphicsDevice is null)
            throw new InvalidOperationException("The texture cache is used before the graphics device exists.");

        texture = Texture2D.FromFile(_graphicsDevice, path);
        _textures[path] = texture;
        return texture;
    }

    public void Dispose()
    {
        foreach (var texture in _textures.Values)
            texture.Dispose();

        _textures.Clear();
    }
}
