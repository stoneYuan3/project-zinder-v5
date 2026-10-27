using Godot;
using ProjectZinder.Core;

namespace ProjectZinder.Game.UI;

/// <summary>The desk's commission file: request, unverified appointment info, public announcement.</summary>
public partial class CommissionPanel : ModalPanel
{
    private TaskCompletionSource? _closed;

    public Task OpenAsync(CaseDefinition def)
    {
        ClearContent();
        Title.Text = $"Commission File — {def.ClientName}";

        Content.AddChild(Section("Request"));
        Content.AddChild(UiTheme.MakeLabel(def.Commission.Request, 18, wrap: true));
        Content.AddChild(Section("Appointment information (unverified)"));
        foreach (var line in def.Commission.Appointment)
            Content.AddChild(UiTheme.MakeLabel("• " + line, 16, wrap: true));
        Content.AddChild(Section("Public announcement"));
        var ad = UiTheme.MakeLabel("“" + def.Commission.PublicAnnouncement + "”", 16, Palette.InkSoft, wrap: true);
        Content.AddChild(ad);

        var close = UiTheme.MakeButton("Close file", Cancel, 160);
        close.SizeFlagsHorizontal = SizeFlags.ShrinkEnd;
        Content.AddChild(close);

        Visible = true;
        _closed = new TaskCompletionSource();
        return _closed.Task;
    }

    protected override void Cancel()
    {
        Visible = false;
        _closed?.TrySetResult();
    }

    private static Label Section(string text) => UiTheme.MakeLabel(text.ToUpperInvariant(), 14, Palette.Plum);
}
