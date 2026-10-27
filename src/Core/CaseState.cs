namespace ProjectZinder.Core;

/// <summary>
/// Mutable progress for one case. Kept as plain data so checkpoints are a simple deep copy.
/// </summary>
public sealed class CaseState
{
    public List<string> Evidence { get; private set; } = new();
    public HashSet<string> Flags { get; private set; } = new();
    public HashSet<string> SeenInteractions { get; private set; } = new();
    public HashSet<string> SolvedPhases { get; private set; } = new();
    public Dictionary<string, int> Patience { get; private set; } = new();
    public Dictionary<string, int> Mistakes { get; private set; } = new();

    public CaseState Clone() => new()
    {
        Evidence = new List<string>(Evidence),
        Flags = new HashSet<string>(Flags),
        SeenInteractions = new HashSet<string>(SeenInteractions),
        SolvedPhases = new HashSet<string>(SolvedPhases),
        Patience = new Dictionary<string, int>(Patience),
        Mistakes = new Dictionary<string, int>(Mistakes),
    };
}
