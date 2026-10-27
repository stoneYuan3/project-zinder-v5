using Godot;
using ProjectZinder.Core;

namespace ProjectZinder.Game.UI;

/// <summary>
/// Evidence list + detail view. Browse mode is purely informational; select mode returns
/// the chosen evidence id (or null if cancelled). Browsing and cancelling are always free.
/// </summary>
public partial class EvidencePanel : ModalPanel
{
    protected override Vector2 PanelSize => new(860, 520);

    private TaskCompletionSource<string?>? _result;
    private string? _selected;
    private List<EvidenceDefinition> _items = new();
    private bool _selectMode;

    private VBoxContainer _list = null!;
    private Label _icon = null!, _name = null!, _facts = null!, _source = null!;
    private Button _present = null!;
    private Label _prompt = null!;

    public override void _Ready()
    {
        base._Ready();
        _prompt = UiTheme.MakeLabel("", 16, Palette.Blood, wrap: true);
        Content.AddChild(_prompt);

        var row = new HBoxContainer { SizeFlagsVertical = SizeFlags.ExpandFill };
        row.AddThemeConstantOverride("separation", 18);
        Content.AddChild(row);

        var scroll = new ScrollContainer { CustomMinimumSize = new Vector2(300, 330) };
        _list = new VBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };
        scroll.AddChild(_list);
        row.AddChild(scroll);

        var detail = new VBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };
        _icon = UiTheme.MakeLabel("", 56);
        _name = UiTheme.MakeLabel("", 22, Palette.Plum, hand: true);
        _facts = UiTheme.MakeLabel("", 17, wrap: true);
        _facts.CustomMinimumSize = new Vector2(480, 0);
        _source = UiTheme.MakeLabel("", 14, Palette.InkSoft);
        detail.AddChild(_icon);
        detail.AddChild(_name);
        detail.AddChild(_facts);
        detail.AddChild(_source);
        row.AddChild(detail);

        var buttons = new HBoxContainer { Alignment = BoxContainer.AlignmentMode.End };
        buttons.AddThemeConstantOverride("separation", 10);
        _present = UiTheme.MakeButton("Present this evidence", Present, 220);
        buttons.AddChild(_present);
        buttons.AddChild(UiTheme.MakeButton("Close", Cancel, 120));
        Content.AddChild(buttons);
    }

    public void OpenBrowse(IEnumerable<EvidenceDefinition> items, string? focusId = null)
    {
        _ = Open(items, null);
        if (focusId != null) Select(focusId);
    }

    public Task<string?> SelectAsync(IEnumerable<EvidenceDefinition> items, string prompt) => Open(items, prompt);

    private Task<string?> Open(IEnumerable<EvidenceDefinition> items, string? prompt)
    {
        _items = items.ToList();
        _selectMode = prompt != null;
        Title.Text = _selectMode ? "Present Evidence" : $"Evidence ({_items.Count})";
        _prompt.Text = prompt ?? "";
        _prompt.Visible = _selectMode;
        _present.Visible = _selectMode;

        foreach (var child in _list.GetChildren())
        {
            _list.RemoveChild(child); // detach now so indices match the new items
            child.QueueFree();
        }
        foreach (var ev in _items)
        {
            var id = ev.Id;
            var button = UiTheme.MakeButton($"{ev.Icon}  {ev.Name}", () => Select(id));
            button.Alignment = HorizontalAlignment.Left;
            _list.AddChild(button);
        }
        Select(_items.FirstOrDefault()?.Id);

        Visible = true;
        _result?.TrySetResult(null);
        _result = new TaskCompletionSource<string?>();
        return _result.Task;
    }

    private void Select(string? id)
    {
        _selected = id;
        var ev = _items.FirstOrDefault(e => e.Id == id);
        for (var i = 0; i < _items.Count && i < _list.GetChildCount(); i++)
        {
            var button = _list.GetChild<Button>(i);
            if (_items[i].Id == id) button.AddThemeStyleboxOverride("normal", UiTheme.Box(Palette.Bandage, Palette.Plum, 3));
            else button.RemoveThemeStyleboxOverride("normal");
        }
        _icon.Text = ev?.Icon ?? "";
        _name.Text = ev?.Name ?? "No evidence yet";
        _facts.Text = ev?.Facts ?? "";
        _source.Text = ev is null ? "" : "Source: " + ev.Source;
        _present.Disabled = ev is null;
    }

    private void Present()
    {
        Visible = false;
        _result?.TrySetResult(_selected);
    }

    protected override void Cancel()
    {
        Visible = false;
        _result?.TrySetResult(null);
    }
}
