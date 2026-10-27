# Project Zinder — mechanics prototype (v5)

Zombie matchmaking mystery. One complete case ("Burned to Charcoal") built from
`#doc/Gameplay Design Document.md` to test the core loop: observe → question → collect
evidence → cross-examine → identify → stamp → brief the client.

Placeholder art is drawn in code (`ZombiePortrait.cs`) using the palette and chibi
proportions of `#doc/zombie-example.jpg`. There are no image assets yet.

## Run

- Godot **4.7.2 .NET** (`C:\YUAN\TOOLS\Godot\Godot_v4.7.2-stable_mono_win64`): open `project.godot`, then Build and Play.
- Tests: `dotnet test tests/ProjectZinder.Tests`

## Layout

| Path | What |
|---|---|
| `src/Core/` | Pure C# rules, no Godot types: unlocks, evidence, patience, checkpoints, judging, data validation. |
| `src/Game/Main.cs` | Scene controller. Sequences the case flow and calls into Core. |
| `src/Game/UI/` | UI components (dialogue, hotspots, panels, cross-exam, bite, stamp ritual). |
| `data/case01_charcoal.json` | All case content: cast, evidence, hotspots, topics, testimony, contradictions, feedback. |
| `scenes/Main.tscn` | Main scene composition. |
| `tests/` | xUnit tests that run against the real case file, including a golden-path solve. |

To add a case, write a new JSON file and point `Main.CasePath` at it.
`CaseLoader.Validate` reports broken ids or unsolvable rounds when the case loads, and the tests report them too.
The requirement tokens (`ev:`, `seen:`, `solved:`, flags) are documented at the top of `CaseDefinition.cs`.

## Golden path (spoilers)

Sooty's hands (paint) → *(optional)* Sooty cross-exam, statement 4 →
Marrow's apron (badge) → ask Marrow about cameras (footage) →
Patch cross-exam: "I carried the child out" + footage → Patch's bandaged leg (charred tissue) →
Final ID: Patch + Charred Tissue → stamp → briefing.

## In scope vs. deferred

Implemented: commission file (auto-opens once, then on the desk), free candidate switching,
hotspots with hover labels and new/completed states, an evidence close-up with the protagonist's
monologue whenever evidence is found, a desk evidence tray (click any item to inspect it), a "Show interaction points" toggle,
topic bubbles, free replay with no duplicate evidence, evidence inspection, gated cross-examination
(3–5 statements, Previous/Next, Question, Present, Review Testimony Again), confirmation before
judged actions, a "LIE EXPOSED!" moment comparing the statement with the evidence,
full confessions, impostors leaving the agency after being exposed (data-driven via `departs`;
the genuine survivor never leaves), specific vs. generic wrong-answer feedback, hints after 2 mistakes, 3 patience
per phase (persists when you leave), bite failure with checkpoint retry, multiple valid solutions,
final identification (candidate + evidence), drag-or-click denture stamp, client briefing,
and epilogue variations from optional exposures.

Deferred (not needed to test the mechanic): audio, save/load, EN/ZH switching,
settings (mute / reduce shake), accident-reconstruction mini-interactions, and real art.

Export note: add `data/*.json` to the export preset's non-resource include filter.
