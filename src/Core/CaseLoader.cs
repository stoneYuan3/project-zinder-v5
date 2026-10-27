using System.Text.Json;
using System.Text.Json.Serialization;

namespace ProjectZinder.Core;

public static class CaseLoader
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
    };

    public static CaseDefinition Parse(string json) =>
        JsonSerializer.Deserialize<CaseDefinition>(json, Options)
        ?? throw new InvalidDataException("Case file is empty.");

    /// <summary>
    /// Catches authoring mistakes (dangling ids, unsolvable rounds) at load time
    /// instead of mid-playtest. Returns an empty list when the case is consistent.
    /// </summary>
    public static List<string> Validate(CaseDefinition def)
    {
        var errors = new List<string>();
        var evidence = def.Evidence.Select(e => e.Id).ToHashSet();
        var candidates = def.Candidates.Select(c => c.Id).ToHashSet();
        var interactions = def.Candidates.SelectMany(c => c.Interactions).Select(i => i.Id).ToList();
        var phases = def.Candidates.Where(c => c.CrossExam != null).Select(c => c.CrossExam!.Id)
            .Append(CaseSession.FinalPhaseId).ToHashSet();

        void CheckEvidence(IEnumerable<string> ids, string where)
        {
            foreach (var id in ids.Where(id => !evidence.Contains(id)))
                errors.Add($"{where}: unknown evidence '{id}'.");
        }

        void CheckRequires(IEnumerable<string> tokens, string where)
        {
            foreach (var t in tokens)
            {
                if (t.StartsWith("ev:") && !evidence.Contains(t[3..])) errors.Add($"{where}: unknown evidence in '{t}'.");
                if (t.StartsWith("solved:") && !phases.Contains(t[7..])) errors.Add($"{where}: unknown phase in '{t}'.");
                if (t.StartsWith("seen:") && !interactions.Contains(t[5..])) errors.Add($"{where}: unknown interaction in '{t}'.");
            }
        }

        foreach (var dup in interactions.GroupBy(i => i).Where(g => g.Count() > 1))
            errors.Add($"Duplicate interaction id '{dup.Key}'.");

        CheckEvidence(def.StartingEvidence, "startingEvidence");

        foreach (var c in def.Candidates)
        {
            foreach (var i in c.Interactions)
            {
                CheckEvidence(i.Grants, $"{c.Id}/{i.Id}");
                CheckRequires(i.Requires, $"{c.Id}/{i.Id}");
                if (i.Kind == InteractionKind.Hotspot && i.Region is not { Length: 4 })
                    errors.Add($"{c.Id}/{i.Id}: hotspot needs a 4-number region.");
            }

            if (c.CrossExam is not { } xe) continue;
            CheckEvidence(xe.Grants, xe.Id);
            CheckRequires(xe.Requires, xe.Id);
            if (xe.Statements.Count is < 3 or > 5)
                errors.Add($"{xe.Id}: rounds should have 3-5 statements (has {xe.Statements.Count}).");
            if (!xe.Statements.Any(s => s.Contradicts.Count > 0))
                errors.Add($"{xe.Id}: no statement can be contradicted; round is unsolvable.");
            foreach (var s in xe.Statements)
            {
                CheckEvidence(s.Contradicts, xe.Id);
                CheckEvidence(s.Feedback.Keys, xe.Id);
            }
        }

        var final = def.Final;
        if (!candidates.Contains(final.CorrectCandidate))
            errors.Add($"final: unknown correct candidate '{final.CorrectCandidate}'.");
        if (final.AcceptedEvidence.Count == 0)
            errors.Add("final: no accepted evidence.");
        CheckEvidence(final.AcceptedEvidence, "final");
        CheckEvidence(final.EvidenceFeedback.Keys, "final");
        CheckRequires(final.Requires, "final");

        foreach (var extra in def.EpilogueExtras)
            CheckRequires(extra.Requires, "epilogueExtras");

        return errors;
    }
}
