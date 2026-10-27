using Godot;

namespace ProjectZinder.Game.UI;

/// <summary>
/// Palette sampled from #doc/zombie-example.jpg: sketchy ink on off-white paper,
/// mauve skin with plum spots, grey hair, cream bandages, red eyes.
/// </summary>
public static class Palette
{
    public static readonly Color Paper = new("#f4f1ea");
    public static readonly Color PaperDark = new("#e6e0d3");
    public static readonly Color Ink = new("#262326");
    public static readonly Color InkSoft = new("#6d6670");
    public static readonly Color Mauve = new("#b89aa6");
    public static readonly Color Plum = new("#7b5f80");
    public static readonly Color Bandage = new("#efe9d6");
    public static readonly Color Blood = new("#c0283a");
    public static readonly Color EyeYellow = new("#f0d36a");
    public static readonly Color Desk = new("#5a4650");
    public static readonly Color DeskEdge = new("#3e2f37");
    public static readonly Color Good = new("#4d7a5a");
}

public static class UiTheme
{
    public static Font BodyFont { get; } = new SystemFont { FontNames = new[] { "Segoe UI", "Arial" } };
    public static Font HandFont { get; } = new SystemFont { FontNames = new[] { "Segoe Print", "Comic Sans MS", "Segoe UI" } };

    public static Theme Build()
    {
        var theme = new Theme { DefaultFont = BodyFont, DefaultFontSize = 18 };

        theme.SetStylebox("normal", "Button", Box(Palette.Paper, Palette.Ink, 2, 8));
        theme.SetStylebox("hover", "Button", Box(Palette.Bandage, Palette.Plum, 3, 8));
        theme.SetStylebox("pressed", "Button", Box(Palette.PaperDark, Palette.Ink, 3, 8));
        theme.SetStylebox("disabled", "Button", Box(Palette.PaperDark with { A = 0.6f }, Palette.InkSoft with { A = 0.5f }, 2, 8));
        theme.SetStylebox("focus", "Button", new StyleBoxEmpty());
        foreach (var state in new[] { "font_color", "font_hover_color", "font_pressed_color", "font_focus_color" })
            theme.SetColor(state, "Button", Palette.Ink);
        theme.SetColor("font_disabled_color", "Button", Palette.InkSoft with { A = 0.6f });

        theme.SetColor("font_color", "Label", Palette.Ink);

        // CheckButton inherits Button styles; it sits on the dark top bar, so strip the paper box.
        foreach (var state in new[] { "normal", "hover", "pressed", "hover_pressed", "focus" })
            theme.SetStylebox(state, "CheckButton", new StyleBoxEmpty());
        foreach (var state in new[] { "font_color", "font_hover_color", "font_pressed_color", "font_hover_pressed_color", "font_focus_color" })
            theme.SetColor(state, "CheckButton", Palette.Paper);

        theme.SetStylebox("panel", "PanelContainer", Box(Palette.Paper, Palette.Ink, 3, 10, 18));
        return theme;
    }

    public static StyleBoxFlat Box(Color bg, Color border, int borderWidth = 2, int radius = 8, float margin = 10)
    {
        var sb = new StyleBoxFlat { BgColor = bg, BorderColor = border };
        sb.SetBorderWidthAll(borderWidth);
        sb.SetCornerRadiusAll(radius);
        sb.SetContentMarginAll(margin);
        return sb;
    }

    public static Label MakeLabel(string text, int size = 18, Color? color = null, bool hand = false, bool wrap = false)
    {
        var label = new Label { Text = text };
        label.AddThemeFontSizeOverride("font_size", size);
        if (color is { } c) label.AddThemeColorOverride("font_color", c);
        if (hand) label.AddThemeFontOverride("font", HandFont);
        if (wrap) label.AutowrapMode = TextServer.AutowrapMode.WordSmart;
        return label;
    }

    public static Button MakeButton(string text, Action onPressed, int minWidth = 0)
    {
        var button = new Button { Text = text, CustomMinimumSize = new Vector2(minWidth, 40) };
        button.Pressed += onPressed;
        return button;
    }
}
