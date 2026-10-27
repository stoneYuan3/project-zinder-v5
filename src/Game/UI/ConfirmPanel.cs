using Godot;

namespace ProjectZinder.Game.UI;

/// <summary>Final confirmation before any judged action. Only "confirm" can cost patience.</summary>
public partial class ConfirmPanel : ModalPanel
{
    protected override Vector2 PanelSize => new(640, 300);

    private TaskCompletionSource<bool>? _result;
    private Label _body = null!;
    private Button _confirm = null!;

    public override void _Ready()
    {
        base._Ready();
        _body = UiTheme.MakeLabel("", 18, wrap: true);
        _body.SizeFlagsVertical = SizeFlags.ExpandFill;
        Content.AddChild(_body);

        var buttons = new HBoxContainer { Alignment = BoxContainer.AlignmentMode.End };
        buttons.AddThemeConstantOverride("separation", 10);
        buttons.AddChild(UiTheme.MakeButton("Cancel", Cancel, 120));
        _confirm = UiTheme.MakeButton("", () => Resolve(true), 180);
        buttons.AddChild(_confirm);
        Content.AddChild(buttons);
    }

    public Task<bool> AskAsync(string title, string body, string confirmText)
    {
        Title.Text = title;
        _body.Text = body;
        _confirm.Text = confirmText;
        Visible = true;
        _result = new TaskCompletionSource<bool>();
        return _result.Task;
    }

    protected override void Cancel() => Resolve(false);

    private void Resolve(bool value)
    {
        Visible = false;
        _result?.TrySetResult(value);
    }
}
