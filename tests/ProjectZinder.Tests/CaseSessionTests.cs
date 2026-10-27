using ProjectZinder.Core;
using Xunit;

namespace ProjectZinder.Tests;

public class CaseSessionTests
{
    private static CaseDefinition LoadCase() =>
        CaseLoader.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "data", "case01_charcoal.json")));

    private static CaseSession NewSession() => new(LoadCase());

    private static InteractionDefinition Interaction(CaseSession s, string id) =>
        s.Definition.Candidates.SelectMany(c => c.Interactions).First(i => i.Id == id);

    private static CrossExamDefinition CrossExam(CaseSession s, string candidate) => s.GetCandidate(candidate).CrossExam!;

    private static int StatementIndex(CrossExamDefinition xe, string evidence) =>
        xe.Statements.FindIndex(st => st.Contradicts.Contains(evidence));

    [Fact]
    public void ShippedCase_PassesValidation()
    {
        Assert.Empty(CaseLoader.Validate(LoadCase()));
    }

    [Fact]
    public void GoldenPath_IsSolvable()
    {
        var s = NewSession();
        s.CompleteInteraction(Interaction(s, "marrow_apron"));
        Assert.True(s.IsAvailable(Interaction(s, "marrow_cameras")));
        s.CompleteInteraction(Interaction(s, "marrow_cameras"));

        var patch = s.GetCandidate("patch");
        Assert.True(s.CanBeginCrossExam(patch));
        s.BeginPhase(patch.CrossExam!.Id);
        var result = s.Challenge(patch.CrossExam, StatementIndex(patch.CrossExam, "evac_footage"), "evac_footage");
        Assert.Equal(ChallengeOutcome.Success, result.Outcome);
        Assert.Contains(result.NewEvidence, e => e.Id == "meal_ticket");

        var leg = Interaction(s, "patch_leg");
        Assert.True(s.IsAvailable(leg));
        s.CompleteInteraction(leg);

        Assert.True(s.CanBeginFinal);
        s.BeginPhase(CaseSession.FinalPhaseId);
        Assert.Equal(ChallengeOutcome.Success, s.Identify("patch", "charred_tissue").Outcome);
        Assert.True(s.IsSolved(CaseSession.FinalPhaseId));
    }

    [Fact]
    public void LockedContent_StaysLockedUntilRequirementsMet()
    {
        var s = NewSession();
        Assert.False(s.IsAvailable(Interaction(s, "marrow_cameras")));
        Assert.False(s.IsAvailable(Interaction(s, "patch_leg")));
        Assert.False(s.CanBeginCrossExam(s.GetCandidate("sooty")));
        Assert.False(s.CanBeginFinal);
    }

    [Fact]
    public void Replay_DoesNotDuplicateEvidence()
    {
        var s = NewSession();
        var hands = Interaction(s, "sooty_hands");
        Assert.Single(s.CompleteInteraction(hands));
        Assert.Empty(s.CompleteInteraction(hands));
        Assert.Single(s.State.Evidence, "black_paint");
        Assert.Equal(InteractionStatus.Completed, s.GetStatus(hands));
    }

    [Fact]
    public void WrongChallenge_CostsPatience_AndGivesSpecificFeedback()
    {
        var s = NewSession();
        s.CompleteInteraction(Interaction(s, "sooty_hands"));
        var xe = CrossExam(s, "sooty");
        s.BeginPhase(xe.Id);

        // Paint vs. "every inch cooked black": relevant but insufficient.
        var r = s.Challenge(xe, 2, "black_paint");
        Assert.Equal(ChallengeOutcome.Wrong, r.Outcome);
        Assert.Equal(xe.Statements[2].Feedback["black_paint"], r.Feedback);
        Assert.Equal(2, r.PatienceLeft);

        // Unrelated evidence: generic feedback.
        r = s.Challenge(xe, 0, "black_paint");
        Assert.Equal(xe.GenericFeedback, r.Feedback);
        Assert.NotNull(r.Hint); // second mistake unlocks hint
    }

    [Fact]
    public void PatienceZero_Bites_AndRetryRestoresPhaseButKeepsInvestigation()
    {
        var s = NewSession();
        s.CompleteInteraction(Interaction(s, "sooty_hands"));
        var xe = CrossExam(s, "sooty");
        s.BeginPhase(xe.Id);

        s.Challenge(xe, 0, "announcement");
        s.Challenge(xe, 0, "announcement");
        Assert.Equal(ChallengeOutcome.Bitten, s.Challenge(xe, 0, "announcement").Outcome);

        s.RetryFromCheckpoint();
        Assert.Equal(CaseSession.StartingPatience, s.GetPatience(xe.Id));
        Assert.True(s.HasEvidence("black_paint"));
        Assert.Contains("sooty_hands", s.State.SeenInteractions);
    }

    [Fact]
    public void LeavingAndReturning_DoesNotRestorePatience()
    {
        var s = NewSession();
        s.CompleteInteraction(Interaction(s, "sooty_hands"));
        var xe = CrossExam(s, "sooty");
        s.BeginPhase(xe.Id);
        s.Challenge(xe, 0, "announcement");

        s.BeginPhase(xe.Id); // player left, then re-entered
        Assert.Equal(2, s.GetPatience(xe.Id));
    }

    [Fact]
    public void MultipleValidSolutions_AreAccepted()
    {
        var s = NewSession();
        s.CompleteInteraction(Interaction(s, "marrow_apron"));
        s.CompleteInteraction(Interaction(s, "marrow_cameras"));
        var xe = CrossExam(s, "marrow");
        var solutions = xe.Statements.Count(st => st.Contradicts.Contains("evac_footage"));
        Assert.True(solutions >= 2);

        s.BeginPhase(xe.Id);
        Assert.Equal(ChallengeOutcome.Success, s.Challenge(xe, 3, "evac_footage").Outcome);
    }

    [Fact]
    public void FinalIdentification_WrongCandidate_UsesCandidateFeedback()
    {
        var s = NewSession();
        s.BeginPhase(CaseSession.FinalPhaseId);
        var r = s.Identify("sooty", "charred_tissue");
        Assert.Equal(ChallengeOutcome.Wrong, r.Outcome);
        Assert.Equal(s.Definition.Final.CandidateFeedback["sooty"], r.Feedback);
    }

    [Fact]
    public void Epilogue_IncludesOptionalExposures()
    {
        var s = NewSession();
        var baseCount = s.BuildEpilogue().Count();
        s.CompleteInteraction(Interaction(s, "sooty_hands"));
        var xe = CrossExam(s, "sooty");
        s.BeginPhase(xe.Id);
        s.Challenge(xe, StatementIndex(xe, "black_paint"), "black_paint");
        Assert.Equal(baseCount + 1, s.BuildEpilogue().Count());
    }
}
