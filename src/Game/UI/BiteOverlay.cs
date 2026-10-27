using Godot;

namespace ProjectZinder.Game.UI;

/// <summary>Patience ran out: the candidate bites the protagonist. Offers checkpoint retry or main menu.</summary>
public partial class BiteOverlay : Control
{
    private ColorRect _flash = null!;
    private Label _chomp = null!, _reason = null!;
    private Control _buttons = null!;
    private TaskCompletionSource<bool>? _result;

    public override void _Ready()
    {
        SetAnchorsPreset(LayoutPreset.FullRect);
        MouseFilter = MouseFilterEnum.Stop;
        Visible = false;

        _flash = new ColorRect { Color = Palette.Blood, AnchorRight = 1, AnchorBottom = 1, MouseFilter = MouseFilterEnum.Ignore };
        AddChild(_flash);

        var col = new VBoxContainer { Position = new Vector2(240, 190), CustomMinimumSize = new Vector2(800, 0) };
        col.AddThemeConstantOverride("separation", 18);
        AddChild(col);

        _chomp = UiTheme.MakeLabel("CHOMP!", 96, Palette.Paper, hand: true);
        _chomp.HorizontalAlignment = HorizontalAlignment.Center;
        _chomp.PivotOffset = new Vector2(400, 60);
        _reason = UiTheme.MakeLabel("", 20, Palette.Paper, wrap: true);
        _reason.HorizontalAlignment = HorizontalAlignment.Center;
        col.AddChild(_chomp);
        col.AddChild(_reason);

        var buttons = new HBoxContainer { Alignment = BoxContainer.AlignmentMode.Center };
        buttons.AddThemeConstantOverride("separation", 16);
        buttons.AddChild(UiTheme.MakeButton("Retry from checkpoint", () => Resolve(true), 240));
        buttons.AddChild(UiTheme.MakeButton("Main menu", () => Resolve(false), 160));
        col.AddChild(buttons);
        _buttons = buttons;
    }

    /// <returns>true = retry from checkpoint, false = main menu.</returns>
    public Task<bool> PlayAsync(string biterName)
    {
        _reason.Text = $"{biterName} ran out of patience after too many wrong accusations and bit you.\n" +
                       "Your evidence and investigation progress are safe. The phase restarts at full patience.";
        Visible = true;
        _buttons.Visible = false;
        _flash.Color = Palette.Blood with { A = 1f };
        _chomp.Scale = Vector2.One * 2.5f;

        var t = CreateTween();
        t.TweenProperty(_chomp, "scale", Vector2.One, 0.25).SetTrans(Tween.TransitionType.Back).SetEase(Tween.EaseType.Out);
        t.TweenProperty(_flash, "color:a", 0.88f, 0.4);
        t.TweenCallback(Callable.From(() => _buttons.Visible = true));

        _result = new TaskCompletionSource<bool>();
        return _result.Task;
    }

    private void Resolve(bool retry)
    {
        Visible = false;
        _result?.TrySetResult(retry);
    }
}
