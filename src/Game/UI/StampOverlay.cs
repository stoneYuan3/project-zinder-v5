using Godot;

namespace ProjectZinder.Game.UI;

/// <summary>
/// Final approval ritual: place the candidate's file, then bite it with the agency's denture stamp.
/// Both drag and click work; misplacing never costs anything.
/// </summary>
public partial class StampOverlay : Control
{
    private static readonly Rect2 SlotRect = new(470, 170, 340, 420);
    private static readonly Rect2 FieldRect = new(560, 470, 160, 90); // verification field once the file is placed

    private Draggable _file = null!, _stamp = null!;
    private Label _instruction = null!;
    private Button _continue = null!;
    private bool _filed, _stamped;
    private string _candidate = "";
    private TaskCompletionSource? _done;

    public override void _Ready()
    {
        SetAnchorsPreset(LayoutPreset.FullRect);
        MouseFilter = MouseFilterEnum.Stop;
        Visible = false;

        _instruction = UiTheme.MakeLabel("", 22, Palette.Paper, hand: true);
        _instruction.Position = new Vector2(0, 30);
        _instruction.Size = new Vector2(1280, 40);
        _instruction.HorizontalAlignment = HorizontalAlignment.Center;
        AddChild(_instruction);

        _file = new Draggable(new Vector2(340, 420), DrawFile) { Position = new Vector2(70, 170) };
        _file.Released += OnFileReleased;
        AddChild(_file);

        _stamp = new Draggable(new Vector2(150, 130), DrawStamp) { Position = new Vector2(1040, 300) };
        _stamp.Released += OnStampReleased;
        AddChild(_stamp);

        _continue = UiTheme.MakeButton("Brief the client ▶", () => { Visible = false; _done?.TrySetResult(); }, 220);
        _continue.Position = new Vector2(1010, 630);
        AddChild(_continue);
    }

    public Task PlayAsync(string candidateName)
    {
        _candidate = candidateName;
        _filed = _stamped = false;
        _file.Position = new Vector2(70, 170);
        _file.Home = _file.Position;
        _stamp.Home = _stamp.Position = new Vector2(1040, 300);
        _stamp.Enabled = false;
        _file.Enabled = true;
        _continue.Visible = false;
        _instruction.Text = "Place the candidate's file in the approval tray (drag or click).";
        _file.QueueRedraw();
        QueueRedraw();
        Visible = true;
        _done = new TaskCompletionSource();
        return _done.Task;
    }

    public override void _Draw()
    {
        DrawRect(new Rect2(Vector2.Zero, new Vector2(1280, 720)), Palette.Desk);
        DrawRect(new Rect2(0, 640, 1280, 80), Palette.DeskEdge);
        if (!_filed)
        {
            DrawRect(SlotRect, Palette.Paper with { A = 0.15f });
            DrawRect(SlotRect, Palette.Paper with { A = 0.7f }, false, 3);
            DrawString(UiTheme.HandFont, SlotRect.Position + new Vector2(70, 215), "APPROVAL TRAY", HorizontalAlignment.Left, -1, 24, Palette.Paper);
        }
    }

    private void OnFileReleased(bool wasClick)
    {
        if (_filed) return;
        if (wasClick || SlotRect.HasPoint(_file.Position + _file.Size / 2))
        {
            _filed = true;
            _file.Enabled = false;
            _file.Home = _file.Position = SlotRect.Position;
            _stamp.Enabled = true;
            _instruction.Text = "Bite the verification field with the denture stamp.";
            QueueRedraw();
        }
        else _file.ReturnHome();
    }

    private void OnStampReleased(bool wasClick)
    {
        if (!_filed || _stamped) return;
        if (wasClick || FieldRect.HasPoint(_stamp.Position + _stamp.Size / 2))
        {
            _stamped = true;
            _stamp.Enabled = false;
            _stamp.ReturnHome();
            _file.QueueRedraw();
            _instruction.Text = "Injury experience VERIFIED.";
            _continue.Visible = true;
        }
        else _stamp.ReturnHome(); // misplaced: free to try again
    }

    private void DrawFile(Control c)
    {
        var r = new Rect2(Vector2.Zero, c.Size);
        c.DrawRect(r, Palette.Paper);
        c.DrawRect(r, Palette.Ink, false, 3);
        c.DrawString(UiTheme.HandFont, new Vector2(24, 50), "CANDIDATE FILE", HorizontalAlignment.Left, -1, 24, Palette.Plum);
        c.DrawString(UiTheme.BodyFont, new Vector2(24, 100), _candidate, HorizontalAlignment.Left, -1, 22, Palette.Ink);
        c.DrawString(UiTheme.BodyFont, new Vector2(24, 140), "Injury: burned to charcoal", HorizontalAlignment.Left, -1, 17, Palette.Ink);
        c.DrawString(UiTheme.BodyFont, new Vector2(24, 168), "Event: Gnawing Spoon fire", HorizontalAlignment.Left, -1, 17, Palette.Ink);
        var field = new Rect2(FieldRect.Position - SlotRect.Position, FieldRect.Size);
        c.DrawRect(field, Palette.InkSoft, false, 2);
        c.DrawString(UiTheme.BodyFont, field.Position + new Vector2(0, -8), "VERIFICATION", HorizontalAlignment.Left, -1, 13, Palette.InkSoft);
        if (_stamped) DrawBiteMark(c, field.GetCenter());
    }

    private static void DrawBiteMark(Control c, Vector2 center)
    {
        for (var i = 0; i < 7; i++)
        {
            var x = (i - 3) * 18f;
            c.DrawCircle(center + new Vector2(x, -22 + Mathf.Abs(x) * 0.25f), 6, Palette.Blood);
            c.DrawCircle(center + new Vector2(x, 22 - Mathf.Abs(x) * 0.25f), 6, Palette.Blood);
        }
    }

    private static void DrawStamp(Control c)
    {
        var w = c.Size.X;
        c.DrawRect(new Rect2(w / 2 - 14, 0, 28, 60), new Color("#7a5b3c"));       // handle
        c.DrawRect(new Rect2(10, 60, w - 20, 34), Palette.Blood with { V = 0.6f }); // gums
        for (var i = 0; i < 6; i++)
            c.DrawRect(new Rect2(16 + i * 20, 92, 16, 22), Palette.Paper);         // teeth
        c.DrawRect(new Rect2(10, 60, w - 20, 54), Palette.Ink, false, 2);
    }

    /// <summary>Minimal drag-or-click item. A release with little movement counts as a click.</summary>
    private partial class Draggable : Control
    {
        public event Action<bool>? Released;
        public Vector2 Home { get; set; }
        public bool Enabled { get; set; } = true;

        private readonly Action<Control> _draw;
        private bool _dragging;
        private Vector2 _grabOffset, _pressPos;

        public Draggable() : this(Vector2.Zero, _ => { }) { }

        public Draggable(Vector2 size, Action<Control> draw)
        {
            _draw = draw;
            Size = size;
            MouseDefaultCursorShape = CursorShape.PointingHand;
        }

        public void ReturnHome() => CreateTween().TweenProperty(this, "position", Home, 0.15);

        public override void _Draw() => _draw(this);

        public override void _GuiInput(InputEvent @event)
        {
            if (!Enabled) return;
            if (@event is InputEventMouseButton { ButtonIndex: MouseButton.Left } mb)
            {
                AcceptEvent();
                if (mb.Pressed)
                {
                    _dragging = true;
                    _grabOffset = mb.Position;
                    _pressPos = mb.GlobalPosition;
                }
                else if (_dragging)
                {
                    _dragging = false;
                    Released?.Invoke(mb.GlobalPosition.DistanceTo(_pressPos) < 6);
                }
            }
            else if (@event is InputEventMouseMotion mm && _dragging)
            {
                Position = GetParent<Control>().GetLocalMousePosition() - _grabOffset;
            }
        }
    }
}
