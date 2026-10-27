namespace ProjectZinder.Core;

public enum InteractionStatus { New, Completed }

public enum ChallengeOutcome { Success, Wrong, Bitten }

public sealed record ChallengeResult(
    ChallengeOutcome Outcome,
    string Feedback,
    string? Hint,
    int PatienceLeft,
    IReadOnlyList<EvidenceDefinition> NewEvidence);

/// <summary>
/// Runtime rules for a single case: unlocks, evidence, patience, judging and checkpoints.
/// The UI only reads from and calls into this; it never mutates <see cref="CaseState"/> directly.
/// </summary>
public sealed class CaseSession
{
    public const int StartingPatience = 3;
    public const int MistakesBeforeHint = 2;
    public const string FinalPhaseId = "final";

    private readonly Dictionary<string, EvidenceDefinition> _evidence;
    private readonly Dictionary<string, string> _interactionOwner;
    private readonly Dictionary<string, string> _crossExamOwner;
    private CaseState? _checkpoint;

    public CaseDefinition Definition { get; }
    public CaseState State { get; private set; }

    /// <summary>Raised for each newly collected evidence item (never for duplicates).</summary>
    public event Action<EvidenceDefinition>? EvidenceCollected;
    /// <summary>Raised after any change that could alter what the UI shows.</summary>
    public event Action? StateChanged;

    public CaseSession(CaseDefinition definition)
    {
        Definition = definition;
        _evidence = definition.Evidence.ToDictionary(e => e.Id);
        _interactionOwner = definition.Candidates
            .SelectMany(c => c.Interactions.Select(i => (i.Id, c.Id)))
            .ToDictionary(x => x.Item1, x => x.Item2);
        _crossExamOwner = definition.Candidates.Where(c => c.CrossExam != null)
            .ToDictionary(c => c.CrossExam!.Id, c => c.Id);
        State = new CaseState();
        foreach (var id in definition.StartingEvidence)
            State.Evidence.Add(id);
    }

    // ---- Queries -------------------------------------------------------------------------

    public EvidenceDefinition GetEvidence(string id) => _evidence[id];

    public IEnumerable<EvidenceDefinition> CollectedEvidence => State.Evidence.Select(GetEvidence);

    public CandidateDefinition GetCandidate(string id) => Definition.Candidates.First(c => c.Id == id);

    public bool HasEvidence(string id) => State.Evidence.Contains(id);

    public bool IsSolved(string phaseId) => State.SolvedPhases.Contains(phaseId);

    public bool Meets(IEnumerable<string> requirements) => requirements.All(Meets);

    public bool Meets(string token)
    {
        var sep = token.IndexOf(':');
        if (sep < 0) return State.Flags.Contains(token);
        var value = token[(sep + 1)..];
        return token[..sep] switch
        {
            "ev" => HasEvidence(value),
            "seen" => State.SeenInteractions.Contains(value),
            "solved" => IsSolved(value),
            _ => State.Flags.Contains(token),
        };
    }

    public bool HasDeparted(string candidateId) => State.Flags.Contains(DepartedFlag(candidateId));

    public static string DepartedFlag(string candidateId) => "left:" + candidateId;

    public bool IsAvailable(InteractionDefinition interaction) =>
        !HasDeparted(OwnerOf(interaction)) && Meets(interaction.Requires);

    public InteractionStatus GetStatus(InteractionDefinition interaction) =>
        State.SeenInteractions.Contains(interaction.Id) ? InteractionStatus.Completed : InteractionStatus.New;

    public bool CanBeginCrossExam(CandidateDefinition candidate) =>
        candidate.CrossExam is { } xe && !HasDeparted(candidate.Id) && !IsSolved(xe.Id) && Meets(xe.Requires);

    public bool CanBeginFinal => !IsSolved(FinalPhaseId) && Meets(Definition.Final.Requires);

    public int GetPatience(string phaseId) =>
        State.Patience.TryGetValue(phaseId, out var p) ? p : StartingPatience;

    // ---- Investigation (always free) -----------------------------------------------------

    /// <summary>
    /// Marks an interaction as viewed. Grants its evidence/flags only the first time,
    /// so replaying a conversation never awards duplicates.
    /// </summary>
    public IReadOnlyList<EvidenceDefinition> CompleteInteraction(InteractionDefinition interaction)
    {
        if (!State.SeenInteractions.Add(interaction.Id))
            return Array.Empty<EvidenceDefinition>();

        var gained = Apply(interaction.Grants, interaction.SetsFlags);
        StateChanged?.Invoke();
        return gained;
    }

    // ---- Formal phases (cost patience) ---------------------------------------------------

    /// <summary>
    /// Enters a formal phase and records a checkpoint at its start.
    /// Patience persists if the player leaves and re-enters the same phase.
    /// </summary>
    public void BeginPhase(string phaseId)
    {
        State.Patience.TryAdd(phaseId, StartingPatience);

        _checkpoint = State.Clone();
        _checkpoint.Patience[phaseId] = StartingPatience;
        _checkpoint.Mistakes.Remove(phaseId);
        StateChanged?.Invoke();
    }

    /// <summary>Restores the state captured at the start of the last formal phase.</summary>
    public void RetryFromCheckpoint()
    {
        if (_checkpoint is null) return;
        State = _checkpoint.Clone();
        StateChanged?.Invoke();
    }

    public ChallengeResult Challenge(CrossExamDefinition crossExam, int statementIndex, string evidenceId)
    {
        var statement = crossExam.Statements[statementIndex];
        if (statement.Contradicts.Contains(evidenceId))
        {
            State.SolvedPhases.Add(crossExam.Id);
            if (crossExam.Departs)
                State.Flags.Add(DepartedFlag(_crossExamOwner[crossExam.Id]));
            var gained = Apply(crossExam.Grants, crossExam.SetsFlags);
            StateChanged?.Invoke();
            return new ChallengeResult(ChallengeOutcome.Success, "", null, GetPatience(crossExam.Id), gained);
        }

        var feedback = statement.Feedback.GetValueOrDefault(evidenceId) ?? crossExam.GenericFeedback;
        return Penalise(crossExam.Id, feedback, crossExam.Hint);
    }

    public ChallengeResult Identify(string candidateId, string evidenceId)
    {
        var final = Definition.Final;
        if (candidateId == final.CorrectCandidate && final.AcceptedEvidence.Contains(evidenceId))
        {
            State.SolvedPhases.Add(FinalPhaseId);
            StateChanged?.Invoke();
            return new ChallengeResult(ChallengeOutcome.Success, "", null, GetPatience(FinalPhaseId),
                Array.Empty<EvidenceDefinition>());
        }

        var feedback = candidateId != final.CorrectCandidate
            ? final.CandidateFeedback.GetValueOrDefault(candidateId) ?? final.GenericFeedback
            : final.EvidenceFeedback.GetValueOrDefault(evidenceId) ?? final.GenericFeedback;
        return Penalise(FinalPhaseId, feedback, null);
    }

    /// <summary>Epilogue lines including any variations unlocked by optional discoveries.</summary>
    public IEnumerable<DialogueLine> BuildEpilogue() =>
        Definition.Epilogue.Concat(Definition.EpilogueExtras.Where(x => Meets(x.Requires)).SelectMany(x => x.Lines));

    // ---- Internals -----------------------------------------------------------------------

    private string OwnerOf(InteractionDefinition interaction) =>
        _interactionOwner.GetValueOrDefault(interaction.Id, "");

    private ChallengeResult Penalise(string phaseId, string feedback, string? hint)
    {
        var patience = Math.Max(0, GetPatience(phaseId) - 1);
        State.Patience[phaseId] = patience;
        var mistakes = State.Mistakes.GetValueOrDefault(phaseId) + 1;
        State.Mistakes[phaseId] = mistakes;
        StateChanged?.Invoke();

        var outcome = patience == 0 ? ChallengeOutcome.Bitten : ChallengeOutcome.Wrong;
        var showHint = mistakes >= MistakesBeforeHint && !string.IsNullOrEmpty(hint) ? hint : null;
        return new ChallengeResult(outcome, feedback, showHint, patience, Array.Empty<EvidenceDefinition>());
    }

    private IReadOnlyList<EvidenceDefinition> Apply(IEnumerable<string> grants, IEnumerable<string> flags)
    {
        foreach (var flag in flags)
            State.Flags.Add(flag);

        var gained = new List<EvidenceDefinition>();
        foreach (var id in grants.Where(id => !HasEvidence(id)))
        {
            State.Evidence.Add(id);
            var ev = GetEvidence(id);
            gained.Add(ev);
            EvidenceCollected?.Invoke(ev);
        }
        return gained;
    }
}
