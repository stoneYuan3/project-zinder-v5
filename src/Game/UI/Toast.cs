using Godot;

namespace ProjectZinder.Game.UI;

/// <summary>Brief, fading announcements: objectives, unlocks, phase changes. Queued so none are lost.</summary>
public partial class Toast : PanelContainer
{
    private Label _label = null!;
    private Tween? _tween;
    private readonly Queue<(string Text, Color Accent, float Hold)> _queue = new();

    public override void _Ready()
    {
        MouseFilter = MouseFilterEnum.Ignore;
        Modulate = Colors.Transparent;
        AddThemeStyleboxOverride("panel", UiTheme.Box(Palette.Ink with { A = 0.9f }, Palette.Plum, 2, 6, 10));
        _label = UiTheme.MakeLabel("", 20, Palette.Paper, hand: true);
        _label.HorizontalAlignment = HorizontalAlignment.Center;
        AddChild(_label);
    }

    public void Announce(string text, Color? accent = null, float hold = 2.2f)
    {
        _queue.Enqueue((text, accent ?? Palette.Paper, hold));
        if (_tween is not { } t || !t.IsRunning()) PlayNext();
    }

    private void PlayNext()
    {
        if (!_queue.TryDequeue(out var next)) return;
        _label.Text = next.Text;
        _label.AddThemeColorOverride("font_color", next.Accent);
        ResetSize();
        Position = new Vector2((1280 - Size.X) / 2, 488); // between portrait and dialogue box

        _tween = CreateTween();
        _tween.TweenProperty(this, "modulate:a", 1f, 0.2);
        _tween.TweenInterval(next.Hold);
        _tween.TweenProperty(this, "modulate:a", 0f, 0.4);
        _tween.TweenCallback(Callable.From(PlayNext));
    }
}
