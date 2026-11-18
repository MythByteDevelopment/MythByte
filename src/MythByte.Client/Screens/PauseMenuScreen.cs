using Microsoft.Xna.Framework;
using MythByte.Client.Input;
using MythByte.Client.Rendering;

namespace MythByte.Client.Screens;

public class PauseMenuScreen(IScreenManager screens, IScreenFactory factory) : IScreen
{
    private const int TextScale = 6;
    private static readonly string[] Items = ["Resume", "Title Screen", "Quit"];

    public int SelectedIndex { get; private set; }

    public bool IsOverlay => true;

    public void Update(float elapsedSeconds, IInputService input)
    {
        if (input.IsPressed(GameAction.Back))
        {
            screens.Pop();
            return;
        }

        if (input.IsPressed(GameAction.Up))
            SelectedIndex = (SelectedIndex + Items.Length - 1) % Items.Length;
        else if (input.IsPressed(GameAction.Down))
            SelectedIndex = (SelectedIndex + 1) % Items.Length;
        else if (input.IsPressed(GameAction.Confirm))
            Select();
    }

    public void Draw(ICanvas canvas)
    {
        canvas.Fill(new Rectangle(0, 0, canvas.Width, canvas.Height), Color.Black * 0.5f);

        var panel = new Rectangle(canvas.Width / 2 - 360, canvas.Height / 2 - 220, 720, 440);
        canvas.Fill(panel, Palette.Wood);
        canvas.Fill(new Rectangle(panel.X + 8, panel.Y + 8, panel.Width - 16, panel.Height - 16), Palette.Background);

        for (var index = 0; index < Items.Length; index++)
        {
            var y = panel.Y + 80 + index * 100;
            var color = index == SelectedIndex ? Palette.Parchment : Palette.Stone;
            if (index == SelectedIndex)
                canvas.Text(">", new Vector2(panel.X + 72, y), TextScale, color);
            canvas.Text(Items[index], new Vector2(panel.X + 144, y), TextScale, color);
        }
    }

    private void Select()
    {
        switch (SelectedIndex)
        {
            case 0:
                screens.Pop();
                break;
            case 1:
                screens.Replace(factory.CreateTitle());
                break;
            default:
                screens.RequestExit();
                break;
        }
    }
}
