namespace ProjectZinder.Core;

// Data contracts for a case file (data/*.json). Content only — no behaviour.
//
// Requirement tokens (used by every `Requires` list; all must be met):
//   "ev:<evidenceId>"     evidence has been collected
//   "seen:<interactionId>" an interaction has been viewed at least once
//   "solved:<phaseId>"    a cross-examination (or "final") has been won
//   "<flag>"              any custom flag set via `SetsFlags`

public sealed class DialogueLine
{
    public string Speaker { get; set; } = "";
    public string Text { get; set; } = "";
}

public sealed class CaseDefinition
{
    public string Id { get; set; } = "";
    public string Title { get; set; } = "";
    public string ClientName { get; set; } = "";
    /// <summary>Non-candidate speakers shown in the portrait slot, keyed by speaker name.</summary>
    public Dictionary<string, Appearance> Cast { get; set; } = new();
    public List<DialogueLine> Intro { get; set; } = new();
    public CommissionFile Commission { get; set; } = new();
    public List<string> StartingEvidence { get; set; } = new();
    public List<EvidenceDefinition> Evidence { get; set; } = new();
    public List<CandidateDefinition> Candidates { get; set; } = new();
    public FinalIdentificationDefinition Final { get; set; } = new();
    public List<DialogueLine> Briefing { get; set; } = new();
    public List<DialogueLine> Epilogue { get; set; } = new();
    public List<ConditionalLines> EpilogueExtras { get; set; } = new();
}

public sealed class CommissionFile
{
    public string Request { get; set; } = "";
    public List<string> Appointment { get; set; } = new();
    public string PublicAnnouncement { get; set; } = "";
}

public sealed class EvidenceDefinition
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    /// <summary>Short glyph used as a placeholder image.</summary>
    public string Icon { get; set; } = "?";
    /// <summary>Confirmed facts only. Never states which testimony to challenge.</summary>
    public string Facts { get; set; } = "";
    public string Source { get; set; } = "";
}

/// <summary>Look parameters for the procedural placeholder portrait.</summary>
public sealed class Appearance
{
    public string Skin { get; set; } = "#b89aa6";
    public string Spots { get; set; } = "#7b5f80";
    public string Hair { get; set; } = "#d6d6d6";
    public bool Bandages { get; set; }
    public bool Stitches { get; set; }
    public bool Apron { get; set; }
    public bool Bag { get; set; }
}

public sealed class CandidateDefinition
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string Tagline { get; set; } = "";
    public Appearance Look { get; set; } = new();
    public List<DialogueLine> Greeting { get; set; } = new();
    public List<InteractionDefinition> Interactions { get; set; } = new();
    public CrossExamDefinition? CrossExam { get; set; }
}

public enum InteractionKind { Hotspot, Topic }

public sealed class InteractionDefinition
{
    public string Id { get; set; } = "";
    public InteractionKind Kind { get; set; }
    public string Label { get; set; } = "";
    /// <summary>Hotspot rect in portrait space (400x480): x, y, w, h.</summary>
    public float[]? Region { get; set; }
    public List<string> Requires { get; set; } = new();
    public List<DialogueLine> Lines { get; set; } = new();
    public List<string> Grants { get; set; } = new();
    public List<string> SetsFlags { get; set; } = new();
}

public sealed class CrossExamDefinition
{
    public string Id { get; set; } = "";
    public List<string> Requires { get; set; } = new();
    public List<DialogueLine> Intro { get; set; } = new();
    public List<StatementDefinition> Statements { get; set; } = new();
    public List<DialogueLine> SuccessLines { get; set; } = new();
    public List<string> Grants { get; set; } = new();
    public List<string> SetsFlags { get; set; } = new();
    public string GenericFeedback { get; set; } = "That evidence doesn't contradict this statement.";
    /// <summary>Shown after repeated mistakes. Points the way without naming the answer.</summary>
    public string Hint { get; set; } = "";
}

public sealed class StatementDefinition
{
    public string Text { get; set; } = "";
    /// <summary>Free follow-up when the player presses ("Question") this statement.</summary>
    public List<DialogueLine> Press { get; set; } = new();
    /// <summary>Evidence that logically contradicts this statement. Empty = statement holds.</summary>
    public List<string> Contradicts { get; set; } = new();
    /// <summary>Specific feedback for relevant-but-insufficient evidence, keyed by evidence id.</summary>
    public Dictionary<string, string> Feedback { get; set; } = new();
}

public sealed class FinalIdentificationDefinition
{
    public List<string> Requires { get; set; } = new();
    public string CorrectCandidate { get; set; } = "";
    public List<string> AcceptedEvidence { get; set; } = new();
    public Dictionary<string, string> CandidateFeedback { get; set; } = new();
    public Dictionary<string, string> EvidenceFeedback { get; set; } = new();
    public string GenericFeedback { get; set; } = "That doesn't prove the injury.";
    public List<DialogueLine> SuccessLines { get; set; } = new();
}

public sealed class ConditionalLines
{
    public List<string> Requires { get; set; } = new();
    public List<DialogueLine> Lines { get; set; } = new();
}
