using Godot;
using ProjectZinder.Core;

namespace ProjectZinder.Game.UI;

/// <summary>
/// Formal testimony loop: one statement at a time, manual Previous/Next, no timers,
/// no highlighting of the contradictory statement. Covers the investigation UI so the
/// formal phase is clearly separated from free investigation.
/// </summary>
public partial class CrossExamPanel : Control
{
    public event Action<int>? QuestionRequested;
    public event Action<int>? PresentRequested;
    public event Action? EvidenceRequested;
    public event Action? FileRequested;
    public event Action? LeaveRequested;

    private CrossExamDefinition _xe = null!;
    private int _index;

    private Label _header = null!, _counter = null!, _patience = null!, _statement = null!;
    private Button _prev = null!, _next = null!;

    public int Index => _index;

    public override void _Ready()
    {
        SetAnchorsPreset(LayoutPreset.FullRect);
        MouseFilter = MouseFilterEnum.Stop;
        Visible = false;

        var banner = new PanelContainer { Position = new Vector2(390, 12), CustomMinimumSize = new Vector2(500, 0) };
        banner.AddThemeStyleboxOverride("panel", UiTheme.Box(Palette.Blood, Palette.Ink, 3, 6, 8));
        _header = UiTheme.MakeLabel("", 22, Palette.Paper, hand: true);
        _header.HorizontalAlignment = HorizontalAlignment.Center;
        banner.AddChild(_header);
        AddChild(banner);

        var panel = new PanelContainer { Position = new Vector2(150, 540), CustomMinimumSize = new Vector2(980, 165) };
        AddChild(panel);
        var col = new VBoxContainer();
        col.AddThemeConstantOverride("separation", 8);
        panel.AddChild(col);

        var top = new HBoxContainer();
        _counter = UiTheme.MakeLabel("", 16, Palette.Plum);
        _patience = UiTheme.MakeLabel("", 18, Palette.Blood);
        _patience.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        _patience.HorizontalAlignment = HorizontalAlignment.Right;
        top.AddChild(_counter);
        top.AddChild(_patience);
        col.AddChild(top);

        _statement = UiTheme.MakeLabel("", 22, wrap: true, hand: true);
        _statement.CustomMinimumSize = new Vector2(940, 56);
        col.AddChild(_statement);

        var buttons = new HBoxContainer();
        buttons.AddThemeConstantOverride("separation", 8);
        _prev = UiTheme.MakeButton("◀ Previous", () => ShowStatement(_index - 1), 130);
        _next = UiTheme.MakeButton("", NextOrRestart, 210);
        buttons.AddChild(_prev);
        buttons.AddChild(UiTheme.MakeButton("Question", () => QuestionRequested?.Invoke(_index), 120));
        var present = UiTheme.MakeButton("Present Evidence", () => PresentRequested?.Invoke(_index), 180);
        present.AddThemeColorOverride("font_color", Palette.Blood);
        buttons.AddChild(present);
        buttons.AddChild(_next);
        buttons.AddChild(new Control { SizeFlagsHorizontal = SizeFlags.ExpandFill });
        buttons.AddChild(UiTheme.MakeButton("Evidence", () => EvidenceRequested?.Invoke(), 100));
        buttons.AddChild(UiTheme.MakeButton("File", () => FileRequested?.Invoke(), 70));
        buttons.AddChild(UiTheme.MakeButton("Leave", () => LeaveRequested?.Invoke(), 80));
        col.AddChild(buttons);
    }

    public void Open(CandidateDefinition candidate, int patience, int startIndex = 0)
    {
        _xe = candidate.CrossExam!;
        _header.Text = $"CROSS-EXAMINATION — {candidate.Name}";
        SetPatience(patience);
        Visible = true;
        ShowStatement(startIndex);
    }

    public void SetPatience(int patience) => _patience.Text = "Patience " + CandidatePickPanel.PatienceText(patience);

    private void NextOrRestart() => ShowStatement(_index + 1 >= _xe.Statements.Count ? 0 : _index + 1);

    private void ShowStatement(int index)
    {
        _index = Mathf.Clamp(index, 0, _xe.Statements.Count - 1);
        _counter.Text = $"Statement {_index + 1} / {_xe.Statements.Count}";
        _statement.Text = "“" + _xe.Statements[_index].Text + "”";
        _prev.Disabled = _index == 0;
        _next.Text = _index == _xe.Statements.Count - 1 ? "↺ Review Testimony Again" : "Next ▶";
    }
}
