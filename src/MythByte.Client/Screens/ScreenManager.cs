using Microsoft.Xna.Framework;
using MythByte.Client.Input;
using MythByte.Client.Rendering;

namespace MythByte.Client.Screens;

public class ScreenManager : IScreenManager
{
    public const float FadeSeconds = 0.25f;

    private readonly List<IScreen> _screens = [];
    private IScreen? _pendingScreen;
    private TransitionPhase _phase = TransitionPhase.None;

    public event Action? ExitRequested;

    public IReadOnlyList<IScreen> Screens => _screens;
    public bool IsTransitioning => _phase != TransitionPhase.None;
    public float FadeAlpha { get; private set; }

    public void Push(IScreen screen) => _screens.Add(screen);

    public void Pop()
    {
        if (_screens.Count > 0)
            _screens.RemoveAt(_screens.Count - 1);
    }

    public void Replace(IScreen screen)
    {
        _pendingScreen = screen;

        if (_screens.Count == 0)
        {
            SwapToPending();
            FadeAlpha = 1f;
            _phase = TransitionPhase.FadingIn;
            return;
        }

        _phase = TransitionPhase.FadingOut;
    }

    public void RequestExit() => ExitRequested?.Invoke();

    public void Update(float elapsedSeconds, IInputService input)
    {
        if (_phase == TransitionPhase.FadingOut)
        {
            FadeAlpha = Math.Min(1f, FadeAlpha + elapsedSeconds / FadeSeconds);
            if (FadeAlpha >= 1f)
            {
                SwapToPending();
                _phase = TransitionPhase.FadingIn;
            }
            return;
        }

        if (_phase == TransitionPhase.FadingIn)
        {
            FadeAlpha = Math.Max(0f, FadeAlpha - elapsedSeconds / FadeSeconds);
            if (FadeAlpha <= 0f)
                _phase = TransitionPhase.None;
            return;
        }

        if (_screens.Count > 0)
            _screens[^1].Update(elapsedSeconds, input);
    }

    public void Draw(ICanvas canvas)
    {
        var first = _screens.Count - 1;
        while (first > 0 && _screens[first].IsOverlay)
            first--;

        for (var index = Math.Max(first, 0); index < _screens.Count; index++)
            _screens[index].Draw(canvas);

        if (FadeAlpha > 0f)
            canvas.Fill(new Rectangle(0, 0, canvas.Width, canvas.Height), Color.Black * FadeAlpha);
    }

    private void SwapToPending()
    {
        _screens.Clear();
        if (_pendingScreen is not null)
            _screens.Add(_pendingScreen);
        _pendingScreen = null;
    }

    private enum TransitionPhase
    {
        None,
        FadingOut,
        FadingIn
    }
}
