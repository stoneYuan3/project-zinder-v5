using Godot;

namespace ProjectZinder.Game.UI;

/// <summary>Full-screen title / end card with a single button.</summary>
public partial class SplashScreen : Control
{
    private Label _title = null!, _subtitle = null!;
    private Button _button = null!;
    private TaskCompletionSource? _done;

    public override void _Ready()
    {
        SetAnchorsPreset(LayoutPreset.FullRect);
        MouseFilter = MouseFilterEnum.Stop;
        Visible = false;

        AddChild(new ColorRect { Color = Palette.Paper, AnchorRight = 1, AnchorBottom = 1, MouseFilter = MouseFilterEnum.Ignore });
        var col = new VBoxContainer { Position = new Vector2(190, 210), CustomMinimumSize = new Vector2(900, 0) };
        col.AddThemeConstantOverride("separation", 20);
        AddChild(col);

        _title = UiTheme.MakeLabel("", 64, Palette.Plum, hand: true);
        _subtitle = UiTheme.MakeLabel("", 20, Palette.InkSoft, wrap: true);
        foreach (var l in new[] { _title, _subtitle }) l.HorizontalAlignment = HorizontalAlignment.Center;
        _button = UiTheme.MakeButton("", () => { Visible = false; _done?.TrySetResult(); }, 260);
        _button.SizeFlagsHorizontal = SizeFlags.ShrinkCenter;
        col.AddChild(_title);
        col.AddChild(_subtitle);
        col.AddChild(_button);
    }

    public Task ShowAsync(string title, string subtitle, string buttonText)
    {
        _title.Text = title;
        _subtitle.Text = subtitle;
        _button.Text = buttonText;
        Visible = true;
        _done = new TaskCompletionSource();
        return _done.Task;
    }
}
