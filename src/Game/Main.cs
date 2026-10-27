using Godot;
using ProjectZinder.Core;
using ProjectZinder.Game.UI;

namespace ProjectZinder.Game;

/// <summary>
/// Scene controller: wires UI components to the <see cref="CaseSession"/> rules and scripts
/// the case flow (intro → investigation ⇄ cross-examinations → final ID → stamp → briefing → epilogue).
/// All rule decisions live in Core; this class only sequences presentation.
/// </summary>
public partial class Main : Control
{
    [Export(PropertyHint.File, "*.json")]
    public string CasePath { get; set; } = "res://data/case01_charcoal.json";

    private CaseSession _session = null!;
    private CandidateDefinition _current = null!;
    private readonly HashSet<string> _greeted = new();
    /// <summary>Candidates whose exit animation has played; their portrait stays hidden from then on.</summary>
    private readonly HashSet<string> _offStage = new();
    /// <summary>Evidence mid-reveal: kept out of the tray until its card lands there.</summary>
    private readonly HashSet<string> _revealing = new();
    private HashSet<string> _unlocked = new();
    private bool _busy;
    private bool _showPoints;
    private bool _caseStarted;
    /// <summary>Formal phase in progress (cross-exam id or "final"), null during free investigation.</summary>
    private string? _phaseId;

    private Label _phase = null!, _patience = null!, _nameplate = null!;
    private CheckButton _showPointsToggle = null!;
    private VBoxContainer _candidateList = null!, _topics = null!;
    private ZombiePortrait _portrait = null!;
    private Button _fileButton = null!, _crossButton = null!, _finalButton = null!;
    private HBoxContainer _evidenceTray = null!;
    private EvidenceReveal _evidenceReveal = null!;
    private ExposeOverlay _expose = null!;
    private CrossExamPanel _cross = null!;
    private DialogueBox _dialogue = null!;
    private EvidencePanel _evidencePanel = null!;
    private CommissionPanel _commission = null!;
    private CandidatePickPanel _pick = null!;
    private ConfirmPanel _confirm = null!;
    private StampOverlay _stamp = null!;
    private BiteOverlay _bite = null!;
    private Toast _toast = null!;
    private SplashScreen _splash = null!;

    public override void _Ready()
    {
        Theme = UiTheme.Build();
        BindNodes();
        _session = new CaseSession(LoadCase(CasePath));
        _current = _session.Definition.Candidates[0];
        WireEvents();
        Refresh();
        Run(ShowTitleAsync);
    }

    // ---- Setup ----------------------------------------------------------------------------

    private void BindNodes()
    {
        _phase = GetNode<Label>("%PhaseLabel");
        _patience = GetNode<Label>("%PatienceLabel");
        _nameplate = GetNode<Label>("%Nameplate");
        _showPointsToggle = GetNode<CheckButton>("%ShowPointsToggle");
        _candidateList = GetNode<VBoxContainer>("%CandidateList");
        _topics = GetNode<VBoxContainer>("%Topics");
        _portrait = GetNode<ZombiePortrait>("%Portrait");
        _fileButton = GetNode<Button>("%FileButton");
        _evidenceTray = GetNode<HBoxContainer>("%EvidenceTray");
        _evidenceReveal = GetNode<EvidenceReveal>("%EvidenceReveal");
        _expose = GetNode<ExposeOverlay>("%Expose");
        _crossButton = GetNode<Button>("%CrossExamButton");
        _finalButton = GetNode<Button>("%FinalButton");
        _cross = GetNode<CrossExamPanel>("%CrossExam");
        _dialogue = GetNode<DialogueBox>("%Dialogue");
        _evidencePanel = GetNode<EvidencePanel>("%EvidencePanel");
        _commission = GetNode<CommissionPanel>("%CommissionPanel");
        _pick = GetNode<CandidatePickPanel>("%CandidatePick");
        _confirm = GetNode<ConfirmPanel>("%Confirm");
        _stamp = GetNode<StampOverlay>("%Stamp");
        _bite = GetNode<BiteOverlay>("%Bite");
        _toast = GetNode<Toast>("%Toast");
        _splash = GetNode<SplashScreen>("%Splash");
    }

    private static CaseDefinition LoadCase(string path)
    {
        var def = CaseLoader.Parse(Godot.FileAccess.GetFileAsString(path));
        foreach (var error in CaseLoader.Validate(def))
            GD.PushError($"[{path}] {error}");
        return def;
    }

    private void WireEvents()
    {
        _showPointsToggle.Toggled += on => { _showPoints = on; Refresh(); };
        _fileButton.Pressed += () => Run(() => _commission.OpenAsync(_session.Definition));
        _crossButton.Pressed += () => Run(BeginCrossExamAsync);
        _finalButton.Pressed += () => Run(FinalIdentificationAsync);

        _cross.QuestionRequested += i => Run(() => PressStatementAsync(i));
        _cross.PresentRequested += i => Run(() => PresentEvidenceAsync(i));
        _cross.EvidenceRequested += () => _evidencePanel.OpenBrowse(_session.CollectedEvidence);
        _cross.FileRequested += () => _ = _commission.OpenAsync(_session.Definition);
        _cross.LeaveRequested += LeaveCrossExam;

        _dialogue.LineStarted += line => ShowSpeaker(line.Speaker);
        _session.StateChanged += Refresh;
    }

    /// <summary>Runs one player-initiated flow at a time; logs instead of swallowing exceptions.</summary>
    private async void Run(Func<Task> flow)
    {
        if (_busy) return;
        _busy = true;
        try { await flow(); }
        catch (Exception e) { GD.PushError(e.ToString()); }
        finally
        {
            _busy = false;
            if (IsInstanceValid(this) && IsInsideTree()) Refresh();
        }
    }

    // ---- Case flow ------------------------------------------------------------------------

    private async Task ShowTitleAsync()
    {
        await _splash.ShowAsync("Zinder", "Zombie Matchmaking Agency — mechanics prototype\n" + _session.Definition.Title, "Open the agency");
        await _dialogue.PlayAsync(_session.Definition.Intro);
        await _commission.OpenAsync(_session.Definition); // opens once automatically, then lives on the desk
        _caseStarted = true;
        _unlocked = Unlocks();
        _toast.Announce("Objective: verify who truly survived being burned to charcoal.");
        Refresh();
        await GreetIfNeededAsync();
    }

    private async Task GreetIfNeededAsync()
    {
        if (_greeted.Add(_current.Id))
            await _dialogue.PlayAsync(_current.Greeting);
    }

    private async Task SelectCandidateAsync(CandidateDefinition candidate)
    {
        _current = candidate;
        Refresh();
        if (IsOffStage(candidate))
            await _dialogue.PlayAsync(candidate.DepartedLines); // every visit, not just the first
        else
            await GreetIfNeededAsync();
    }

    private bool IsOffStage(CandidateDefinition candidate) =>
        _session.HasDeparted(candidate.Id) && _offStage.Contains(candidate.Id);

    private async Task InteractAsync(InteractionDefinition interaction)
    {
        await _dialogue.PlayAsync(interaction.Lines);
        await AnnounceEvidenceAsync(_session.CompleteInteraction(interaction));
        AnnounceUnlocks();
    }

    private async Task AnnounceEvidenceAsync(IEnumerable<EvidenceDefinition> gained)
    {
        var items = gained.ToList();
        foreach (var ev in items) _revealing.Add(ev.Id);
        Refresh();
        foreach (var ev in items)
        {
            await _evidenceReveal.ShowAsync(ev, _dialogue, TrayDropPoint());
            _revealing.Remove(ev.Id);
            Refresh();
        }
    }

    // ---- Cross-examination ----------------------------------------------------------------

    private async Task BeginCrossExamAsync()
    {
        var xe = _current.CrossExam!;
        _session.BeginPhase(xe.Id); // checkpoint
        _phaseId = xe.Id;
        _toast.Announce("CROSS-EXAMINATION", Palette.Blood, 1.4f);
        await _dialogue.PlayAsync(xe.Intro);
        _cross.Open(_current, _session.GetPatience(xe.Id));
        Refresh();
    }

    private async Task PressStatementAsync(int index)
    {
        var statement = _current.CrossExam!.Statements[index];
        _cross.Visible = false;
        await _dialogue.PlayAsync(statement.Press.Count > 0
            ? statement.Press
            : new() { new DialogueLine { Speaker = _current.Name, Text = "I already told you everything." } });
        _cross.Visible = true;
    }

    private async Task PresentEvidenceAsync(int index)
    {
        var xe = _current.CrossExam!;
        var statement = xe.Statements[index];
        var evidenceId = await _evidencePanel.SelectAsync(_session.CollectedEvidence, $"Challenge: “{statement.Text}”");
        if (evidenceId is null) return;
        var ev = _session.GetEvidence(evidenceId);

        var confirmed = await _confirm.AskAsync("Confirm challenge",
            $"Statement:  “{statement.Text}”\nEvidence:  {ev.Icon} {ev.Name}\n\nA wrong challenge costs 1 patience.", "Challenge!");
        if (!confirmed) return;

        var result = _session.Challenge(xe, index, evidenceId);
        _cross.Visible = false;
        switch (result.Outcome)
        {
            case ChallengeOutcome.Success:
                _portrait.Shake();
                ScreenShake();
                await _expose.ShowAsync(statement.Text, ev);
                await _dialogue.PlayAsync(xe.SuccessLines); // full confession
                await AnnounceEvidenceAsync(result.NewEvidence);
                _phaseId = null;
                if (_session.HasDeparted(_current.Id))
                {
                    await _portrait.ExitAsync();
                    _offStage.Add(_current.Id);
                    _toast.Announce($"{_current.Name} has left the agency.");
                }
                _toast.Announce("Cross-Examination Complete", Palette.Good);
                AnnounceUnlocks();
                break;

            case ChallengeOutcome.Wrong:
                _portrait.Shake();
                await PlayFeedbackAsync(_current.Name, result);
                _cross.SetPatience(result.PatienceLeft);
                _cross.Visible = true;
                break;

            case ChallengeOutcome.Bitten:
                if (await BiteAsync(_current.Name))
                    _cross.Open(_current, _session.GetPatience(xe.Id));
                break;
        }
    }

    private void LeaveCrossExam()
    {
        _cross.Visible = false;
        _phaseId = null;
        _toast.Announce("Left cross-examination. Patience is kept until you return.");
        Refresh();
    }

    private async Task PlayFeedbackAsync(string speaker, ChallengeResult result, bool speakerPresent = true)
    {
        var lines = new List<DialogueLine>();
        if (speakerPresent)
            lines.Add(new() { Speaker = speaker, Text = result.PatienceLeft == 1
                ? "One more accusation like that and I'm taking a bite."
                : "That proves nothing! Watch your accusations." });
        lines.Add(new() { Speaker = "You (thinking)", Text = result.Feedback });
        if (result.Hint is { } hint)
            lines.Add(new DialogueLine { Speaker = "Hint", Text = hint });
        await _dialogue.PlayAsync(lines);
    }

    /// <returns>true if the player retried (state restored), false if they went to the main menu.</returns>
    private async Task<bool> BiteAsync(string biterName)
    {
        _portrait.Visible = true; // a departed candidate storms back in for this
        await _portrait.LungeAsync();
        ScreenShake();
        if (!await _bite.PlayAsync(biterName))
        {
            GetTree().ReloadCurrentScene();
            return false;
        }
        _session.RetryFromCheckpoint();
        return true;
    }

    // ---- Final identification -------------------------------------------------------------

    private async Task FinalIdentificationAsync()
    {
        _session.BeginPhase(CaseSession.FinalPhaseId); // checkpoint
        _phaseId = CaseSession.FinalPhaseId;
        CandidateDefinition chosen;
        while (true)
        {
            Refresh();
            var candidateId = await _pick.PickAsync(_session.Definition.Candidates, _session.GetPatience(CaseSession.FinalPhaseId), IsOffStage);
            if (candidateId is null) { _phaseId = null; return; }
            chosen = _session.GetCandidate(candidateId);

            var evidenceId = await _evidencePanel.SelectAsync(_session.CollectedEvidence,
                $"Which evidence proves {chosen.Name}'s injury is genuine?");
            if (evidenceId is null) continue;
            var ev = _session.GetEvidence(evidenceId);

            var confirmed = await _confirm.AskAsync("Confirm identification",
                $"Identify {chosen.Name} as the genuine survivor,\nsupported by {ev.Icon} {ev.Name}?\n\nA wrong identification costs 1 patience.", "Submit");
            if (!confirmed) continue;

            _current = chosen;
            Refresh();
            var result = _session.Identify(chosen.Id, evidenceId);
            if (result.Outcome == ChallengeOutcome.Success) break;

            _portrait.Shake();
            if (result.Outcome == ChallengeOutcome.Wrong)
                await PlayFeedbackAsync(chosen.Name, result, !IsOffStage(chosen));
            else if (!await BiteAsync(chosen.Name))
                return;
        }
        _phaseId = null;
        Refresh();

        await _dialogue.PlayAsync(_session.Definition.Final.SuccessLines);
        await _stamp.PlayAsync(chosen.Name);
        await _dialogue.PlayAsync(_session.Definition.Briefing);
        await _dialogue.PlayAsync(_session.BuildEpilogue());
        await _splash.ShowAsync("Case Closed", "Thanks for playtesting! Optional: replay and expose every impostor for extra epilogue lines.", "Play again");
        GetTree().ReloadCurrentScene();
    }

    // ---- Presentation ---------------------------------------------------------------------

    private HashSet<string> Unlocks()
    {
        var set = _session.Definition.Candidates.Where(_session.CanBeginCrossExam).Select(c => c.Id).ToHashSet();
        if (_session.CanBeginFinal) set.Add(CaseSession.FinalPhaseId);
        return set;
    }

    private void AnnounceUnlocks()
    {
        var now = Unlocks();
        foreach (var id in now.Except(_unlocked))
            _toast.Announce(id == CaseSession.FinalPhaseId
                ? "Final Identification unlocked"
                : $"Cross-examination available: {_session.GetCandidate(id).Name}", Palette.EyeYellow);
        _unlocked = now;
    }

    private void ShowSpeaker(string speaker)
    {
        var candidate = _session.Definition.Candidates.FirstOrDefault(c => c.Name == speaker);
        if (candidate != null && IsOffStage(candidate)) return;
        if (candidate != null) SetPortrait(candidate.Name, candidate.Look, candidate.Id, candidate == _current && _caseStarted);
        else if (_session.Definition.Cast.TryGetValue(speaker, out var look)) SetPortrait(speaker, look, speaker, false);
        // "You", "Narrator", "Evidence", ...: keep whoever is on screen.
    }

    private void SetPortrait(string name, Appearance look, string seed, bool showHotspots)
    {
        _nameplate.Text = name;
        _portrait.Visible = true;
        _portrait.SetLook(look, seed);
        foreach (var hotspot in _portrait.GetChildren().OfType<Hotspot>())
            hotspot.Visible = showHotspots;
    }

    private void Refresh()
    {
        if (!_caseStarted) return;
        // Status area: patience is only shown during formal phases.
        _phase.Text = _phaseId switch
        {
            null => "Investigation — " + _session.Definition.Title,
            CaseSession.FinalPhaseId => "Final Identification",
            _ => "Cross-Examination",
        };
        _patience.Text = _phaseId is null ? "" : "Patience " + CandidatePickPanel.PatienceText(_session.GetPatience(_phaseId));

        // Candidate switcher
        foreach (var child in _candidateList.GetChildren()) child.QueueFree();
        foreach (var c in _session.Definition.Candidates)
        {
            var candidate = c;
            var hasNew = c.Interactions.Any(i => _session.IsAvailable(i) && _session.GetStatus(i) == InteractionStatus.New);
            var gone = IsOffStage(c);
            var mark = c == _current ? "▶ " : "   ";
            var button = UiTheme.MakeButton($"{mark}{c.Name}{(hasNew ? "  •" : "")}\n     {(gone ? "(left the agency)" : c.Tagline)}",
                () => Run(() => SelectCandidateAsync(candidate)));
            button.Alignment = HorizontalAlignment.Left;
            button.CustomMinimumSize = new Vector2(230, 64);
            if (gone) button.Modulate = Colors.White with { A = 0.5f };
            if (c == _current) button.AddThemeStyleboxOverride("normal", UiTheme.Box(Palette.Bandage, Palette.Plum, 3));
            _candidateList.AddChild(button);
        }

        // Portrait + hotspots
        foreach (var old in _portrait.GetChildren().OfType<Hotspot>()) old.QueueFree();
        foreach (var i in _current.Interactions.Where(i => i.Kind == InteractionKind.Hotspot && _session.IsAvailable(i)))
        {
            var hotspot = Hotspot.Create(i, _session.GetStatus(i), _showPoints);
            hotspot.Clicked += x => Run(() => InteractAsync(x));
            _portrait.AddChild(hotspot);
        }
        SetPortrait(_current.Name, _current.Look, _current.Id, true);
        var currentGone = IsOffStage(_current);
        if (currentGone)
        {
            _portrait.Visible = false;
            _nameplate.Text = $"{_current.Name} (left)";
        }

        // Topic bubbles
        foreach (var child in _topics.GetChildren()) child.QueueFree();
        if (!currentGone)
            _topics.AddChild(UiTheme.MakeLabel("Ask about…", 16, Palette.InkSoft, hand: true));
        foreach (var i in _current.Interactions.Where(i => i.Kind == InteractionKind.Topic && _session.IsAvailable(i)))
        {
            var interaction = i;
            var isNew = _session.GetStatus(i) == InteractionStatus.New;
            var bubble = UiTheme.MakeButton((isNew ? "NEW  " : "↺  ") + i.Label, () => Run(() => InteractAsync(interaction)));
            bubble.Alignment = HorizontalAlignment.Left;
            bubble.AddThemeStyleboxOverride("normal", isNew
                ? UiTheme.Box(Palette.Paper, Palette.Blood, 3, 16)
                : UiTheme.Box(Palette.PaperDark, Palette.InkSoft, 2, 16));
            if (!isNew) bubble.AddThemeColorOverride("font_color", Palette.InkSoft);
            _topics.AddChild(bubble);
        }

        // Desk tools
        RefreshEvidenceTray();
        var xe = _current.CrossExam;
        _crossButton.Visible = xe != null && !currentGone;
        if (xe != null)
        {
            var solved = _session.IsSolved(xe.Id);
            _crossButton.Disabled = !_session.CanBeginCrossExam(_current);
            _crossButton.Text = solved ? "Cross-Examination ✓" : "Begin Cross-Examination";
            _crossButton.TooltipText = solved ? "Already resolved." : _crossButton.Disabled ? "Collect the necessary evidence first." : "";
        }
        _finalButton.Disabled = !_session.CanBeginFinal;
        _finalButton.TooltipText = _finalButton.Disabled ? "Find decisive proof of a genuine injury first." : "";
    }

    /// <summary>Evidence tray on the desk: one slot per item, click to inspect it at any time.</summary>
    private void RefreshEvidenceTray()
    {
        foreach (var child in _evidenceTray.GetChildren()) child.QueueFree();
        var collected = _session.CollectedEvidence.ToList();
        foreach (var ev in collected.Where(e => !_revealing.Contains(e.Id)))
        {
            var id = ev.Id;
            var slot = UiTheme.MakeButton(ev.Icon, () => _evidencePanel.OpenBrowse(collected, id));
            slot.CustomMinimumSize = new Vector2(48, 44);
            slot.TooltipText = ev.Name;
            slot.AddThemeFontSizeOverride("font_size", 22);
            _evidenceTray.AddChild(slot);
        }
    }

    private Vector2 TrayDropPoint() =>
        _evidenceTray.GlobalPosition + new Vector2(_evidenceTray.Size.X + 24, 22);

    private void ScreenShake()
    {
        var t = CreateTween();
        for (var i = 0; i < 8; i++)
            t.TweenProperty(this, "position", new Vector2(GD.RandRange(-12, 12), GD.RandRange(-12, 12)), 0.03);
        t.TweenProperty(this, "position", Vector2.Zero, 0.05);
    }

    public override void _Draw()
    {
        // Agency office: paper wall, faint wainscot, desk edge in the foreground.
        DrawRect(new Rect2(0, 0, 1280, 720), Palette.Paper);
        DrawLine(new Vector2(0, 470), new Vector2(1280, 470), Palette.PaperDark, 3);
        DrawRect(new Rect2(0, 600, 1280, 120), Palette.Desk);
        DrawRect(new Rect2(0, 600, 1280, 8), Palette.DeskEdge);
    }
}
