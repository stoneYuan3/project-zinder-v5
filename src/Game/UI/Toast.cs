using Godot;

namespace ProjectZinder.Game.UI;

/// <summary>Brief, fading announcements: objectives, new evidence, phase changes.</summary>
public partial class Toast : PanelContainer
{
    private Label _label = null!;
    private Tween? _tween;

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
        _label.Text = text;
        _label.AddThemeColorOverride("font_color", accent ?? Palette.Paper);
        ResetSize();
        Position = new Vector2((1280 - Size.X) / 2, 488); // between portrait and dialogue box

        _tween?.Kill();
        _tween = CreateTween();
        _tween.TweenProperty(this, "modulate:a", 1f, 0.2);
        _tween.TweenInterval(hold);
        _tween.TweenProperty(this, "modulate:a", 0f, 0.6);
    }
}
