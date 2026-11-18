using Microsoft.Xna.Framework;
using MythByte.Client.Input;
using MythByte.Client.Rendering;
using MythByte.Shared.Versioning;

namespace MythByte.Client.Screens;

public class TitleScreen(IScreenManager screens, IScreenFactory factory) : IScreen
{
    private readonly string _version = "v" + AppVersion.Of(typeof(TitleScreen).Assembly);
    private float _time;

    public bool IsOverlay => false;

    public void Update(float elapsedSeconds, IInputService input)
    {
        _time += elapsedSeconds;

        if (input.IsPressed(GameAction.Confirm))
            screens.Replace(factory.CreateWorld());
        else if (input.IsPressed(GameAction.Back))
            screens.RequestExit();
    }

    public void Draw(ICanvas canvas)
    {
        canvas.Fill(new Rectangle(0, 0, canvas.Width, canvas.Height), Palette.Background);
        canvas.Fill(new Rectangle(0, 300, canvas.Width, 8), Palette.Wood);
        canvas.Fill(new Rectangle(0, 560, canvas.Width, 8), Palette.Wood);
        canvas.TextCentered("MythByte", 340, 24, Palette.Parchment);

        if ((int)(_time * 2) % 2 == 0)
            canvas.TextCentered("Press Enter", 720, 6, Palette.Stone);

        var versionX = canvas.Width - PixelFont.MeasureWidth(_version, 4) - 32;
        canvas.Text(_version, new Vector2(versionX, canvas.Height - PixelFont.MeasureHeight(4) - 32), 4, Palette.Stone);
    }
}
