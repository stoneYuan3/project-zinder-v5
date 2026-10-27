using Godot;
using ProjectZinder.Core;

namespace ProjectZinder.Game.UI;

/// <summary>
/// Close-up shown when evidence is first obtained: the evidence card fills the stage while the
/// protagonist explains it in the dialogue box, then the card flies into the evidence tray.
/// The big icon stands in for a future close-up illustration.
/// </summary>
public partial class EvidenceReveal : Control
{
    private static readonly Vector2 CardSize = new(440, 400);
    private static readonly Vector2 CardPosition = new((1280 - 440) / 2f, 110);

    private PanelContainer _card = null!;
    private Label _icon = null!, _name = null!, _source = null!;

    public override void _Ready()
    {
        SetAnchorsPreset(LayoutPreset.FullRect);
        MouseFilter = MouseFilterEnum.Stop;
        Visible = false;

        AddChild(new ColorRect { Color = Palette.Ink with { A = 0.6f }, AnchorRight = 1, AnchorBottom = 1, MouseFilter = MouseFilterEnum.Ignore });

        _card = new PanelContainer { CustomMinimumSize = CardSize, Size = CardSize, PivotOffset = CardSize / 2 };
        _card.AddThemeStyleboxOverride("panel", UiTheme.Box(Palette.Paper, Palette.Ink, 4, 6, 20));
        AddChild(_card);

        var col = new VBoxContainer { Alignment = BoxContainer.AlignmentMode.Center };
        col.AddThemeConstantOverride("separation", 6);
        _card.AddChild(col);

        var banner = UiTheme.MakeLabel("NEW EVIDENCE", 16, Palette.Blood);
        _icon = UiTheme.MakeLabel("", 130);
        _name = UiTheme.MakeLabel("", 30, Palette.Plum, hand: true);
        _source = UiTheme.MakeLabel("", 15, Palette.InkSoft);
        foreach (var l in new[] { banner, _icon, _name, _source })
        {
            l.HorizontalAlignment = HorizontalAlignment.Center;
            col.AddChild(l);
        }
    }

    /// <param name="trayTarget">Global position the card shrinks into when the reveal ends.</param>
    public async Task ShowAsync(EvidenceDefinition ev, DialogueBox dialogue, Vector2 trayTarget)
    {
        _icon.Text = ev.Icon;
        _name.Text = ev.Name;
        _source.Text = "Source: " + ev.Source;
        _card.Position = CardPosition;
        _card.Scale = Vector2.One * 0.3f;
        _card.Modulate = Colors.White;
        _card.Rotation = 0;
        Visible = true;

        var intro = CreateTween();
        intro.TweenProperty(_card, "scale", Vector2.One, 0.3).SetTrans(Tween.TransitionType.Back).SetEase(Tween.EaseType.Out);
        intro.Parallel().TweenProperty(_card, "rotation", Mathf.DegToRad(-2f), 0.3);
        await ToSignal(intro, Tween.SignalName.Finished);

        var lines = ev.Reveal.Count > 0
            ? ev.Reveal
            : new List<DialogueLine> { new() { Speaker = "You", Text = $"({ev.Name}. That goes in the evidence tray.)" } };
        await dialogue.PlayAsync(lines);

        var outro = CreateTween().SetParallel();
        outro.TweenProperty(_card, "global_position", trayTarget - CardSize * 0.06f, 0.35).SetTrans(Tween.TransitionType.Cubic).SetEase(Tween.EaseType.In);
        outro.TweenProperty(_card, "scale", Vector2.One * 0.12f, 0.35);
        outro.TweenProperty(_card, "modulate:a", 0.2f, 0.35);
        await ToSignal(outro, Tween.SignalName.Finished);
        Visible = false;
    }
}
