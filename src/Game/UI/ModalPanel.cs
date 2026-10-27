using Godot;

namespace ProjectZinder.Game.UI;

/// <summary>
/// Base for secondary menus: dims the scene, blocks input behind it, closes with Esc.
/// Opening/closing never touches game state, so players always return to where they were.
/// </summary>
public abstract partial class ModalPanel : Control
{
    protected VBoxContainer Content { get; private set; } = null!;
    protected Label Title { get; private set; } = null!;
    protected virtual Vector2 PanelSize => new(760, 520);

    public override void _Ready()
    {
        SetAnchorsPreset(LayoutPreset.FullRect);
        MouseFilter = MouseFilterEnum.Stop;
        Visible = false;

        AddChild(new ColorRect { Color = Palette.Ink with { A = 0.55f }, AnchorRight = 1, AnchorBottom = 1, MouseFilter = MouseFilterEnum.Ignore });

        var panel = new PanelContainer
        {
            Position = (new Vector2(1280, 720) - PanelSize) / 2,
            CustomMinimumSize = PanelSize,
        };
        AddChild(panel);

        var root = new VBoxContainer();
        root.AddThemeConstantOverride("separation", 12);
        panel.AddChild(root);

        Title = UiTheme.MakeLabel("", 26, Palette.Plum, hand: true);
        root.AddChild(Title);
        Content = new VBoxContainer { SizeFlagsVertical = SizeFlags.ExpandFill };
        Content.AddThemeConstantOverride("separation", 10);
        root.AddChild(Content);
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (Visible && @event.IsActionPressed("ui_cancel"))
        {
            GetViewport().SetInputAsHandled();
            Cancel();
        }
    }

    protected void ClearContent()
    {
        foreach (var child in Content.GetChildren()) child.QueueFree();
    }

    /// <summary>Called on Esc. Subclasses resolve their pending task as cancelled.</summary>
    protected abstract void Cancel();
}
