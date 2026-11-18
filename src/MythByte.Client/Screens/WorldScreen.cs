using Microsoft.Xna.Framework;
using MythByte.Client.Input;
using MythByte.Client.Rendering;

namespace MythByte.Client.Screens;

public class WorldScreen(IScreenManager screens, IScreenFactory factory) : IScreen
{
    private const int TileSize = 64;

    public bool IsOverlay => false;

    public void Update(float elapsedSeconds, IInputService input)
    {
        if (input.IsPressed(GameAction.Back))
            screens.Push(factory.CreatePauseMenu());
    }

    public void Draw(ICanvas canvas)
    {
        for (var y = 0; y < canvas.Height; y += TileSize)
        {
            for (var x = 0; x < canvas.Width; x += TileSize)
            {
                var color = (x / TileSize + y / TileSize) % 2 == 0 ? Palette.Moss : Palette.MossDark;
                canvas.Fill(new Rectangle(x, y, TileSize, TileSize), color);
            }
        }

        canvas.Text("Esc - Menu", new Vector2(32, canvas.Height - PixelFont.MeasureHeight(4) - 32), 4, Palette.Parchment);
    }
}
