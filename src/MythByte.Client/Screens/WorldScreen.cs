using Microsoft.Extensions.Options;
using Microsoft.Xna.Framework;
using MythByte.Client.Input;
using MythByte.Client.Maps;
using MythByte.Client.Options;
using MythByte.Client.Rendering;
using MythByte.Client.World;
using MythByte.Shared.Maps;

namespace MythByte.Client.Screens;

public class WorldScreen : IScreen
{
    private const int MarkerInset = 12;

    private static readonly (GameAction Action, int Dx, int Dy)[] Steps =
    [
        (GameAction.Up, 0, -1),
        (GameAction.Down, 0, 1),
        (GameAction.Left, -1, 0),
        (GameAction.Right, 1, 0)
    ];

    private readonly IScreenManager _screens;
    private readonly IScreenFactory _factory;

    public WorldScreen(IScreenManager screens, IScreenFactory factory, IMapLoader maps, IOptions<ClientOptions> options)
    {
        _screens = screens;
        _factory = factory;
        Map = maps.Load(options.Value.StartMap);
        var (spawnX, spawnY) = Map.Spawn;
        Player = new LocalPlayer(spawnX, spawnY);
    }

    public bool IsOverlay => false;
    public TileMap Map { get; }
    public LocalPlayer Player { get; }

    public void Update(float elapsedSeconds, IInputService input)
    {
        if (input.IsPressed(GameAction.Back))
        {
            _screens.Push(_factory.CreatePauseMenu());
            return;
        }

        foreach (var (action, dx, dy) in Steps)
        {
            if (!input.IsPressed(action))
                continue;

            Player.TryStep(dx, dy, Map);
            return;
        }
    }

    public Point CameraFor(int viewWidth, int viewHeight)
    {
        var focus = new Point(Player.X * Map.TileWidth + Map.TileWidth / 2, Player.Y * Map.TileHeight + Map.TileHeight / 2);
        return Camera.TopLeft(focus, new Point(Map.PixelWidth, Map.PixelHeight), new Point(viewWidth, viewHeight));
    }

    public void Draw(ICanvas canvas)
    {
        var camera = CameraFor(canvas.Width, canvas.Height);
        MapRenderer.Draw(canvas, Map, camera);

        var x = Player.X * Map.TileWidth - camera.X;
        var y = Player.Y * Map.TileHeight - camera.Y;
        canvas.Fill(new Rectangle(x + MarkerInset, y + MarkerInset, Map.TileWidth - MarkerInset * 2, Map.TileHeight - MarkerInset * 2), Palette.Iron);
        canvas.Fill(new Rectangle(x + MarkerInset + 4, y + MarkerInset + 4, Map.TileWidth - MarkerInset * 2 - 8, Map.TileHeight - MarkerInset * 2 - 8), Palette.FadedRed);

        canvas.Text("Esc - Menu", new Vector2(32, canvas.Height - PixelFont.MeasureHeight(4) - 32), 4, Palette.Parchment);
    }
}
