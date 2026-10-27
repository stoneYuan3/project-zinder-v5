using Godot;
using ProjectZinder.Core;

namespace ProjectZinder.Game.UI;

/// <summary>
/// The "gotcha" moment of a successful challenge: red flash, a slammed-in stamp, and the
/// struck-through statement set against the evidence that breaks it. Click to continue.
/// </summary>
public partial class ExposeOverlay : Control
{
    private ColorRect _flash = null!;
    private Label _stamp = null!, _evidence = null!, _facts = null!, _continue = null!;
    private RichTextLabel _statement = null!;
    private Control _compare = null!;
    private TaskCompletionSource? _done;
    private bool _ready;

    public override void _Ready()
    {
        SetAnchorsPreset(LayoutPreset.FullRect);
        MouseFilter = MouseFilterEnum.Stop;
        Visible = false;

        AddChild(new ColorRect { Color = Palette.Ink with { A = 0.78f }, AnchorRight = 1, AnchorBottom = 1, MouseFilter = MouseFilterEnum.Ignore });
        _flash = new ColorRect { Color = Palette.Blood, AnchorRight = 1, AnchorBottom = 1, MouseFilter = MouseFilterEnum.Ignore };
        AddChild(_flash);

        _stamp = UiTheme.MakeLabel("LIE EXPOSED!", 92, Palette.Blood, hand: true);
        _stamp.HorizontalAlignment = HorizontalAlignment.Center;
        _stamp.Position = new Vector2(0, 70);
        _stamp.Size = new Vector2(1280, 140);
        _stamp.PivotOffset = new Vector2(640, 70);
        _stamp.AddThemeColorOverride("font_outline_color", Palette.Paper);
        _stamp.AddThemeConstantOverride("outline_size", 10);
        AddChild(_stamp);

        var row = new HBoxContainer { Position = new Vector2(110, 260), CustomMinimumSize = new Vector2(1060, 260) };
        row.AddThemeConstantOverride("separation", 30);
        AddChild(row);
        _compare = row;

        var said = Card(row, "THEY SAID");
        _statement = new RichTextLabel { BbcodeEnabled = true, FitContent = true, CustomMinimumSize = new Vector2(450, 0), ScrollActive = false };
        _statement.AddThemeFontOverride("normal_font", UiTheme.HandFont);
        _statement.AddThemeFontSizeOverride("normal_font_size", 24);
        _statement.AddThemeColorOverride("default_color", Palette.Blood);
        said.AddChild(_statement);

        var but = Card(row, "BUT THE EVIDENCE SHOWS");
        _evidence = UiTheme.MakeLabel("", 24, Palette.Plum, hand: true);
        _facts = UiTheme.MakeLabel("", 16, wrap: true);
        _facts.CustomMinimumSize = new Vector2(450, 0);
        but.AddChild(_evidence);
        but.AddChild(_facts);

        _continue = UiTheme.MakeLabel("▼ click to continue", 15, Palette.Paper);
        _continue.HorizontalAlignment = HorizontalAlignment.Center;
        _continue.Position = new Vector2(0, 560);
        _continue.Size = new Vector2(1280, 30);
        AddChild(_continue);
    }

    public Task ShowAsync(string statement, EvidenceDefinition evidence)
    {
        _statement.Text = $"[s]“{statement}”[/s]";
        _evidence.Text = $"{evidence.Icon}  {evidence.Name}";
        _facts.Text = evidence.Facts;
        _ready = false;
        _continue.Visible = false;
        Visible = true;

        _flash.Color = Palette.Blood with { A = 0.9f };
        _stamp.Scale = Vector2.One * 3f;
        _stamp.Rotation = Mathf.DegToRad(-14);
        _stamp.Modulate = Colors.Transparent;
        _compare.Modulate = Colors.Transparent;

        var t = CreateTween();
        t.TweenProperty(_flash, "color:a", 0f, 0.35);
        t.Parallel().TweenProperty(_stamp, "modulate:a", 1f, 0.1);
        t.Parallel().TweenProperty(_stamp, "scale", Vector2.One, 0.22).SetTrans(Tween.TransitionType.Back).SetEase(Tween.EaseType.Out);
        t.Parallel().TweenProperty(_stamp, "rotation", Mathf.DegToRad(-6), 0.22);
        t.TweenProperty(_compare, "modulate:a", 1f, 0.3);
        t.TweenCallback(Callable.From(() => { _ready = true; _continue.Visible = true; }));

        _done = new TaskCompletionSource();
        return _done.Task;
    }

    public override void _GuiInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton { Pressed: true, ButtonIndex: MouseButton.Left })
        {
            AcceptEvent();
            Finish();
        }
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (Visible && @event.IsActionPressed("ui_accept"))
        {
            GetViewport().SetInputAsHandled();
            Finish();
        }
    }

    private void Finish()
    {
        if (!_ready) return;
        Visible = false;
        _done?.TrySetResult();
    }

    private static VBoxContainer Card(Container parent, string header)
    {
        var panel = new PanelContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };
        parent.AddChild(panel);
        var col = new VBoxContainer();
        col.AddThemeConstantOverride("separation", 10);
        panel.AddChild(col);
        col.AddChild(UiTheme.MakeLabel(header, 14, Palette.InkSoft));
        return col;
    }
}
