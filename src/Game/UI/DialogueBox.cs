using Godot;
using ProjectZinder.Core;

namespace ProjectZinder.Game.UI;

/// <summary>
/// Single-line dialogue with a nameplate. Advanced manually (click / Enter / Space);
/// clicking while text is revealing completes the line instead. Blocks other input while open.
/// </summary>
public partial class DialogueBox : Control
{
    private const float CharsPerSecond = 60f;

    /// <summary>Raised when a line starts, so the scene can swap the centred portrait.</summary>
    public event Action<DialogueLine>? LineStarted;

    private Label _name = null!;
    private Label _text = null!;
    private Label _arrow = null!;
    private readonly Queue<DialogueLine> _queue = new();
    private TaskCompletionSource? _done;
    private Tween? _reveal;

    public bool IsOpen => Visible;

    public override void _Ready()
    {
        SetAnchorsPreset(LayoutPreset.FullRect);
        MouseFilter = MouseFilterEnum.Stop;
        Visible = false;

        var panel = new PanelContainer { Position = new Vector2(150, 560), Size = new Vector2(980, 140) };
        AddChild(panel);
        var box = new VBoxContainer();
        box.AddThemeConstantOverride("separation", 4);
        panel.AddChild(box);

        _name = UiTheme.MakeLabel("", 20, Palette.Plum, hand: true);
        _text = UiTheme.MakeLabel("", 20, wrap: true);
        _text.CustomMinimumSize = new Vector2(940, 60);
        _arrow = UiTheme.MakeLabel("▼ click", 13, Palette.InkSoft);
        _arrow.HorizontalAlignment = HorizontalAlignment.Right;
        box.AddChild(_name);
        box.AddChild(_text);
        box.AddChild(_arrow);
    }

    public Task PlayAsync(IEnumerable<DialogueLine> lines)
    {
        foreach (var line in lines) _queue.Enqueue(line);
        if (_queue.Count == 0) return Task.CompletedTask;

        _done ??= new TaskCompletionSource();
        if (!Visible)
        {
            Visible = true;
            ShowNext();
        }
        return _done.Task;
    }

    public Task SayAsync(string speaker, string text) =>
        PlayAsync(new[] { new DialogueLine { Speaker = speaker, Text = text } });

    public override void _GuiInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton { Pressed: true, ButtonIndex: MouseButton.Left })
        {
            AcceptEvent();
            Advance();
        }
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (Visible && @event.IsActionPressed("ui_accept"))
        {
            GetViewport().SetInputAsHandled();
            Advance();
        }
    }

    private void Advance()
    {
        if (_reveal is { } t && t.IsRunning())
        {
            t.Kill();
            _text.VisibleRatio = 1f;
            _arrow.Visible = true;
            return;
        }
        ShowNext();
    }

    private void ShowNext()
    {
        if (_queue.Count == 0)
        {
            Visible = false;
            var done = _done;
            _done = null;
            done?.SetResult();
            return;
        }

        var line = _queue.Dequeue();
        _name.Text = line.Speaker;
        _name.Visible = !string.IsNullOrEmpty(line.Speaker);
        _text.Text = line.Text;
        _text.VisibleRatio = 0f;
        _arrow.Visible = false;
        LineStarted?.Invoke(line);

        _reveal = CreateTween();
        _reveal.TweenProperty(_text, "visible_ratio", 1f, Mathf.Max(0.1f, line.Text.Length / CharsPerSecond));
        _reveal.TweenCallback(Callable.From(() => _arrow.Visible = true));
    }
}
