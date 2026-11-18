using Microsoft.Extensions.Options;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MythByte.Client.Input;
using MythByte.Client.Options;
using MythByte.Client.Rendering;
using MythByte.Client.Screens;
using MythByte.Shared.Versioning;

namespace MythByte.Client;

public class MythByteGame : Game
{
    private readonly GraphicsDeviceManager _graphics;
    private readonly IInputService _input;
    private readonly IScreenManager _screens;
    private readonly IScreenFactory _factory;
    private SpriteBatch? _spriteBatch;
    private VirtualScreen? _virtualScreen;
    private Canvas? _canvas;
    private bool _resizing;

    public MythByteGame(IOptions<ClientOptions> options, IInputService input, IScreenManager screens, IScreenFactory factory)
    {
        _input = input;
        _screens = screens;
        _factory = factory;

        var settings = options.Value;
        _graphics = new GraphicsDeviceManager(this)
        {
            PreferredBackBufferWidth = settings.WindowWidth,
            PreferredBackBufferHeight = settings.WindowHeight,
            IsFullScreen = settings.Fullscreen,
            SynchronizeWithVerticalRetrace = true
        };

        IsFixedTimeStep = true;
        TargetElapsedTime = TimeSpan.FromSeconds(1d / settings.TargetFramesPerSecond);
        IsMouseVisible = true;
        Window.AllowUserResizing = true;
        Window.Title = "MythByte v" + AppVersion.Of(typeof(MythByteGame).Assembly);
        Window.ClientSizeChanged += OnClientSizeChanged;
        _screens.ExitRequested += Exit;
    }

    protected override void Initialize()
    {
        base.Initialize();
        _screens.Replace(_factory.CreateTitle());
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        _virtualScreen = new VirtualScreen(GraphicsDevice);
        _canvas = new Canvas(_spriteBatch, GraphicsDevice);
    }

    protected override void Update(GameTime gameTime)
    {
        _input.Update(Keyboard.GetState(), GamePad.GetState(PlayerIndex.One));
        _screens.Update((float)gameTime.ElapsedGameTime.TotalSeconds, _input);

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        if (_spriteBatch is null || _virtualScreen is null || _canvas is null)
            return;

        _virtualScreen.Begin();
        GraphicsDevice.Clear(Palette.Background);
        _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
        _screens.Draw(_canvas);
        _spriteBatch.End();
        _virtualScreen.End();

        var parameters = GraphicsDevice.PresentationParameters;
        var destination = Letterbox.Fit(parameters.BackBufferWidth, parameters.BackBufferHeight, VirtualScreen.Width, VirtualScreen.Height);

        GraphicsDevice.Clear(Color.Black);
        _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
        _spriteBatch.Draw(_virtualScreen.Target, destination, Color.White);
        _spriteBatch.End();

        base.Draw(gameTime);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _screens.ExitRequested -= Exit;
            _canvas?.Dispose();
            _virtualScreen?.Dispose();
            _spriteBatch?.Dispose();
        }

        base.Dispose(disposing);
    }

    private void OnClientSizeChanged(object? sender, EventArgs e)
    {
        if (_resizing || _graphics.IsFullScreen)
            return;

        var bounds = Window.ClientBounds;
        if (bounds.Width <= 0 || bounds.Height <= 0)
            return;

        _resizing = true;
        _graphics.PreferredBackBufferWidth = bounds.Width;
        _graphics.PreferredBackBufferHeight = bounds.Height;
        _graphics.ApplyChanges();
        _resizing = false;
    }
}
