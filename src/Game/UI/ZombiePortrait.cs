using Godot;
using ProjectZinder.Core;

namespace ProjectZinder.Game.UI;

/// <summary>
/// Placeholder chibi zombie drawn procedurally in the reference sketch style
/// (wobbly double ink lines, flat mauve fills, plum spots). Portrait space is 400x480;
/// hotspot regions in the case data use the same coordinates.
/// Replace with real sprites later — only <see cref="SetLook"/> needs to change.
/// </summary>
public partial class ZombiePortrait : Control
{
    public static readonly Vector2 PortraitSize = new(400, 480);

    private Appearance _look = new();
    private Color _skin, _spots, _hair;
    private float _seed;
    private Vector2 _basePosition;
    private Tween? _reaction;

    public override void _Ready()
    {
        CustomMinimumSize = PortraitSize;
        Size = PortraitSize;
        PivotOffset = new Vector2(PortraitSize.X / 2, PortraitSize.Y);
        _basePosition = Position;
        MouseFilter = MouseFilterEnum.Ignore;
    }

    public void SetLook(Appearance look, string seedKey)
    {
        _look = look;
        _skin = new Color(look.Skin);
        _spots = new Color(look.Spots);
        _hair = new Color(look.Hair);
        _seed = Math.Abs(seedKey.GetHashCode() % 1000) * 0.37f;
        QueueRedraw();
    }

    public override void _Process(double delta)
    {
        // Gentle idle sway so the scene doesn't feel frozen.
        if (_reaction is { } t && t.IsRunning()) return;
        var time = Time.GetTicksMsec() / 1000f;
        Position = _basePosition + new Vector2(0, Mathf.Sin(time * 1.6f + _seed) * 3f);
    }

    public void Shake()
    {
        _reaction?.Kill();
        _reaction = CreateTween();
        for (var i = 0; i < 6; i++)
            _reaction.TweenProperty(this, "position:x", _basePosition.X + (i % 2 == 0 ? 10 : -10), 0.04);
        _reaction.TweenProperty(this, "position:x", _basePosition.X, 0.04);
    }

    public async Task LungeAsync()
    {
        _reaction?.Kill();
        _reaction = CreateTween();
        _reaction.TweenProperty(this, "scale", new Vector2(1.6f, 1.6f), 0.18).SetTrans(Tween.TransitionType.Back);
        _reaction.TweenInterval(0.35);
        _reaction.TweenProperty(this, "scale", Vector2.One, 0.25);
        await ToSignal(_reaction, Tween.SignalName.Finished);
    }

    /// <summary>Candidate walks off stage; the portrait ends hidden and reset for reuse.</summary>
    public async Task ExitAsync()
    {
        _reaction?.Kill();
        _reaction = CreateTween();
        _reaction.TweenProperty(this, "position:x", _basePosition.X + 520, 0.6).SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.In);
        _reaction.Parallel().TweenProperty(this, "modulate:a", 0f, 0.6);
        await ToSignal(_reaction, Tween.SignalName.Finished);
        Visible = false;
        Position = _basePosition;
        Modulate = Colors.White;
    }

    // ---- Drawing --------------------------------------------------------------------------

    public override void _Draw()
    {
        // Back hair
        Shape(Ellipse(new(200, 150), new(124, 108)), _hair);
        Shape(new Vector2[] { new(86, 140), new(76, 230), new(92, 305), new(132, 292), new(132, 200) }, _hair);
        Shape(new Vector2[] { new(314, 140), new(324, 230), new(308, 305), new(268, 292), new(268, 200) }, _hair);

        // Legs + boots
        Shape(new Vector2[] { new(166, 362), new(196, 362), new(193, 446), new(168, 446) }, _skin);
        Shape(new Vector2[] { new(205, 362), new(235, 362), new(238, 446), new(212, 446) }, _skin);
        Shape(Ellipse(new(178, 452), new(28, 14)), Palette.Paper);
        Shape(Ellipse(new(227, 452), new(28, 14)), Palette.Paper);

        // Torso (white top, like the reference)
        Shape(new Vector2[] { new(156, 256), new(244, 256), new(252, 372), new(148, 372) }, Palette.Paper);
        Shape(new Vector2[] { new(186, 244), new(214, 244), new(212, 262), new(188, 262) }, _skin);

        // Arms + hands
        Shape(new Vector2[] { new(152, 262), new(168, 276), new(126, 352), new(106, 342) }, _skin);
        Shape(new Vector2[] { new(248, 262), new(232, 276), new(274, 352), new(294, 342) }, _skin);
        Shape(Ellipse(new(115, 356), new(19, 17)), _skin);
        Shape(Ellipse(new(285, 356), new(19, 17)), _skin);

        // Head + ears
        Shape(new Vector2[] { new(108, 150), new(60, 112), new(112, 186) }, _skin);
        Shape(new Vector2[] { new(292, 150), new(340, 112), new(288, 186) }, _skin);
        Shape(Ellipse(new(200, 162), new(100, 92)), _skin);

        // Decay spots
        foreach (var (c, r) in new (Vector2, Vector2)[]
                 { (new(150, 196), new(15, 10)), (new(246, 132), new(12, 9)), (new(124, 318), new(9, 7)),
                   (new(276, 298), new(10, 8)), (new(182, 402), new(8, 6)), (new(226, 214), new(7, 5)) })
            DrawColoredPolygon(Ellipse(c, r, 14), _spots);

        DrawEye(new Vector2(165, 170));
        DrawEye(new Vector2(235, 170));

        // Mouth with fang
        Sketch(new Vector2[] { new(184, 216), new(200, 220), new(216, 214) }, false);
        Shape(new Vector2[] { new(204, 218), new(209, 230), new(213, 216) }, Colors.White);

        // Bangs
        Shape(new Vector2[] { new(98, 168), new(104, 96), new(150, 62), new(200, 54), new(254, 62), new(298, 100),
                              new(302, 168), new(282, 122), new(252, 106), new(226, 132), new(200, 102),
                              new(172, 128), new(142, 106), new(116, 132) }, _hair);

        if (_look.Apron) DrawApron();
        if (_look.Bag) DrawBag();
        if (_look.Bandages) DrawBandages();
        if (_look.Stitches) DrawStitches();
    }

    private void DrawEye(Vector2 c)
    {
        Shape(Ellipse(c, new(19, 21)), Palette.EyeYellow);
        DrawColoredPolygon(Ellipse(c + new Vector2(2, 2), new(12, 15), 16), Palette.Blood);
        DrawColoredPolygon(new[] { c + new Vector2(2, -6), c + new Vector2(6, 2), c + new Vector2(2, 9), c + new Vector2(-2, 2) }, Palette.Ink);
        DrawArc(c + new Vector2(0, 8), 20, 0.4f, 2.7f, 12, _spots, 3f, true); // tired under-eye
    }

    private void DrawApron()
    {
        Shape(new Vector2[] { new(162, 286), new(238, 286), new(246, 372), new(154, 372) }, new Color("#d9d3c1"));
        DrawColoredPolygon(Ellipse(new(184, 340), new(10, 7), 12), new Color("#9c8a6a"));
        DrawColoredPolygon(Ellipse(new(222, 356), new(7, 5), 12), new Color("#9c8a6a"));
        Shape(new Vector2[] { new(196, 296), new(226, 296), new(226, 312), new(196, 312) }, new Color("#e8c766")); // badge
    }

    private void DrawBag()
    {
        Sketch(new Vector2[] { new(160, 262), new(310, 352) }, false, 4f);
        Shape(new Vector2[] { new(294, 346), new(360, 350), new(356, 410), new(298, 406) }, new Color("#5c4a5e"));
    }

    private void DrawBandages()
    {
        // Forehead band and wrap over the right eye
        Bandaged(new Vector2[] { new(106, 108), new(292, 96), new(298, 134), new(104, 152) });
        Bandaged(new Vector2[] { new(212, 122), new(270, 110), new(272, 204), new(216, 206) });
        // Sling on the left arm
        Bandaged(new Vector2[] { new(110, 300), new(200, 292), new(206, 330), new(112, 344) });
        // Right leg wrap
        Bandaged(new Vector2[] { new(204, 372), new(238, 372), new(240, 448), new(210, 448) });
        // Coat pocket
        Shape(new Vector2[] { new(164, 318), new(202, 318), new(202, 350), new(164, 350) }, Palette.PaperDark);
    }

    private void DrawStitches()
    {
        var center = new Vector2(250, 280);
        for (var a = -1.2f; a <= 1.9f; a += 0.28f)
        {
            var p = center + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * 26;
            var n = new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * 6;
            DrawLine(p - n, p + n, Palette.Ink, 2f, true);
        }
        DrawArc(center, 26, -1.2f, 1.9f, 20, Palette.Ink, 1.5f, true);
    }

    private void Bandaged(Vector2[] quad)
    {
        Shape(quad, Palette.Bandage);
        // Wrapping strokes across the quad
        for (var t = 0.2f; t < 1f; t += 0.2f)
        {
            var a = quad[0].Lerp(quad[3], t);
            var b = quad[1].Lerp(quad[2], Mathf.Min(1f, t + 0.12f));
            DrawLine(a, b, Palette.InkSoft, 1.5f, true);
        }
    }

    /// <summary>Filled shape with a sketchy double outline.</summary>
    private void Shape(Vector2[] points, Color fill)
    {
        DrawColoredPolygon(points, fill);
        Sketch(points, true);
    }

    private void Sketch(Vector2[] points, bool closed, float width = 2.2f)
    {
        for (var pass = 0; pass < 2; pass++)
        {
            var count = points.Length + (closed ? 1 : 0);
            var line = new Vector2[count];
            for (var i = 0; i < count; i++)
            {
                var p = points[i % points.Length];
                var j = i * 12.9898f + pass * 78.233f + _seed;
                line[i] = p + new Vector2(Mathf.Sin(j) * 1.6f, Mathf.Cos(j * 1.3f) * 1.6f);
            }
            DrawPolyline(line, pass == 0 ? Palette.Ink : Palette.Ink with { A = 0.45f }, pass == 0 ? width : width * 0.6f, true);
        }
    }

    private static Vector2[] Ellipse(Vector2 c, Vector2 r, int segments = 32)
    {
        var pts = new Vector2[segments];
        for (var i = 0; i < segments; i++)
        {
            var a = Mathf.Tau * i / segments;
            pts[i] = c + new Vector2(Mathf.Cos(a) * r.X, Mathf.Sin(a) * r.Y);
        }
        return pts;
    }
}
