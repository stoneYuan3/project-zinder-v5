using Godot;
using ProjectZinder.Core;

namespace ProjectZinder.Game.UI;

/// <summary>
/// Clickable region on a character. Hover shows a label; the marker shows reading progress
/// (new vs. completed) — never whether the character is lying.
/// </summary>
public partial class Hotspot : Control
{
    public event Action<InteractionDefinition>? Clicked;

    private InteractionDefinition _interaction = null!;
    private InteractionStatus _status;
    private bool _hovered;
    private bool _showMarker;

    public static Hotspot Create(InteractionDefinition interaction, InteractionStatus status, bool showMarker)
    {
        var r = interaction.Region!;
        var hotspot = new Hotspot
        {
            _interaction = interaction,
            _status = status,
            _showMarker = showMarker,
            Position = new Vector2(r[0], r[1]),
            Size = new Vector2(r[2], r[3]),
            MouseFilter = MouseFilterEnum.Stop,
            MouseDefaultCursorShape = CursorShape.PointingHand,
        };
        hotspot.MouseEntered += () => { hotspot._hovered = true; hotspot.QueueRedraw(); };
        hotspot.MouseExited += () => { hotspot._hovered = false; hotspot.QueueRedraw(); };
        return hotspot;
    }

    public override void _Process(double delta)
    {
        if (_showMarker && _status == InteractionStatus.New) QueueRedraw(); // pulse
    }

    public override void _GuiInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton { Pressed: true, ButtonIndex: MouseButton.Left })
        {
            AcceptEvent();
            Clicked?.Invoke(_interaction);
        }
    }

    public override void _Draw()
    {
        var rect = new Rect2(Vector2.Zero, Size);
        if (_hovered || _showMarker)
        {
            var color = _status == InteractionStatus.New ? Palette.Blood : Palette.InkSoft;
            color.A = _hovered ? 0.9f : 0.45f;
            DrawDashedRect(rect, color);
        }

        if (_showMarker)
        {
            var c = Size / 2;
            if (_status == InteractionStatus.New)
            {
                var pulse = 9 + Mathf.Sin(Time.GetTicksMsec() / 180f) * 2;
                DrawCircle(c, pulse, Palette.Blood);
                DrawString(UiTheme.BodyFont, c + new Vector2(-3, 6), "!", HorizontalAlignment.Left, -1, 16, Colors.White);
            }
            else
            {
                DrawCircle(c, 9, Palette.Paper);
                DrawArc(c, 9, 0, Mathf.Tau, 16, Palette.InkSoft, 2, true);
                DrawString(UiTheme.BodyFont, c + new Vector2(-6, 6), "✓", HorizontalAlignment.Left, -1, 15, Palette.InkSoft);
            }
        }

        if (_hovered)
        {
            var text = _interaction.Label;
            var width = UiTheme.BodyFont.GetStringSize(text, HorizontalAlignment.Left, -1, 16).X + 16;
            var box = new Rect2(new Vector2((Size.X - width) / 2, -30), new Vector2(width, 26));
            DrawRect(box, Palette.Ink);
            DrawString(UiTheme.BodyFont, box.Position + new Vector2(8, 19), text, HorizontalAlignment.Left, -1, 16, Palette.Paper);
        }
    }

    private void DrawDashedRect(Rect2 r, Color c)
    {
        var tl = r.Position; var br = r.End;
        var tr = new Vector2(br.X, tl.Y); var bl = new Vector2(tl.X, br.Y);
        DrawDashedLine(tl, tr, c, 2, 6); DrawDashedLine(tr, br, c, 2, 6);
        DrawDashedLine(br, bl, c, 2, 6); DrawDashedLine(bl, tl, c, 2, 6);
    }
}
