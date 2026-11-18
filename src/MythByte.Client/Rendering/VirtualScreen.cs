using Microsoft.Xna.Framework.Graphics;

namespace MythByte.Client.Rendering;

public sealed class VirtualScreen(GraphicsDevice graphicsDevice) : IDisposable
{
    public const int Width = 1920;
    public const int Height = 1080;

    public RenderTarget2D Target { get; } = new(graphicsDevice, Width, Height);

    public void Begin() => graphicsDevice.SetRenderTarget(Target);

    public void End() => graphicsDevice.SetRenderTarget(null);

    public void Dispose() => Target.Dispose();
}
