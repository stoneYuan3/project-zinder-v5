using Godot;
using ProjectZinder.Core;

namespace ProjectZinder.Game.UI;

/// <summary>Final identification step 1: choose the candidate whose injury is genuine.</summary>
public partial class CandidatePickPanel : ModalPanel
{
    protected override Vector2 PanelSize => new(620, 360);

    private TaskCompletionSource<string?>? _result;

    public Task<string?> PickAsync(IEnumerable<CandidateDefinition> candidates, int patience, Func<CandidateDefinition, bool> hasLeft)
    {
        ClearContent();
        Title.Text = "Final Identification";
        Content.AddChild(UiTheme.MakeLabel(
            $"Who genuinely survived the injury? You'll then choose the evidence that proves it.\nPatience: {PatienceText(patience)}",
            17, wrap: true));

        foreach (var c in candidates)
        {
            var id = c.Id;
            Content.AddChild(UiTheme.MakeButton($"{c.Name}  —  {(hasLeft(c) ? "(left the agency)" : c.Tagline)}", () => Resolve(id)));
        }
        var cancel = UiTheme.MakeButton("Not yet", Cancel, 140);
        cancel.SizeFlagsHorizontal = SizeFlags.ShrinkEnd;
        Content.AddChild(cancel);

        Visible = true;
        _result = new TaskCompletionSource<string?>();
        return _result.Task;
    }

    public static string PatienceText(int patience) =>
        new string('●', patience) + new string('○', CaseSession.StartingPatience - patience);

    protected override void Cancel() => Resolve(null);

    private void Resolve(string? id)
    {
        Visible = false;
        _result?.TrySetResult(id);
    }
}
