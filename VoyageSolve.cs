using System;
using System.Collections.Generic;
using System.Linq;
using DeepwaterEngagementSuite.VoyagePlannerData;

namespace DeepwaterEngagementSuite;

public enum VoyageSolveFallbackStage
{
    None,
    FastAfterSlow,
    ReservationsRelaxed,
    StrategyLocksRelaxed,
}

public sealed class VoyageSolve
{
    private VoyagePlanner _slowPlanner;

    public VoyageScorer Scorer { get; private set; }
    public VoyagePlacementRules.Result Placement { get; private set; }
    public VoyagePuzzle Puzzle { get; private set; }
    public VoyageFocusAnalysis FocusAnalysis { get; private set; } = VoyageFocusAnalysis.Disabled;
    public VoyageSolveFallbackStage FallbackStage { get; private set; }
    public bool WasSlowTimedOut { get; private set; }
    public bool RecoveredWithFastSolver { get; private set; }
    public bool CurrentAttemptUsesFastSolver { get; private set; }
    public int InputPieceCount { get; private set; }
    public int InitialReservedPieceCount { get; private set; }
    public int SolverPieceCount => Placement?.Pieces?.Count ?? 0;
    public int ReservedPieceCount => Math.Max(0, InputPieceCount - SolverPieceCount);
    public int LockedPlacementCount => Placement?.Locks?.Count ?? 0;

    public void Cancel() => _slowPlanner?.Cancel();

    public IEnumerable<VoyageSolutionResult> Run(
        List<MapPiece> pieces,
        IReadOnlyList<BorderEffect>[,] tileBorders,
        bool useFastSolver,
        VoyagePlannerSettings settings = null,
        VoyageStrategyOptions strategyOptions = null,
        VoyageLayoutPreference layoutPreference = VoyageLayoutPreference.SnakeOrCompact,
        double layoutPreferenceStrength = 0,
        VoyageLayoutFamilies allowedLayoutFamilies = VoyageLayoutFamilies.All,
        double minimumLayoutSimilarity = 0.62)
    {
        settings ??= new VoyagePlannerSettings();
        strategyOptions ??= VoyageStrategyOptions.AllEnabled;
        InputPieceCount = pieces.Count;
        FocusAnalysis = VoyageFocusAnalyzer.Analyze(tileBorders, pieces, strategyOptions);
        var focusedPieces = FocusAnalysis.ApplyWeights(pieces);
        strategyOptions = FocusAnalysis.Concentrate(strategyOptions);
        FallbackStage = VoyageSolveFallbackStage.None;
        WasSlowTimedOut = false;
        RecoveredWithFastSolver = false;

        long exploredBefore = 0;
        long prunedBefore = 0;

        ConfigureAttempt(
            focusedPieces, tileBorders, strategyOptions,
            reserveCharts: true,
            disablePlacementRules: false,
            layoutPreference: layoutPreference,
            layoutPreferenceStrength: layoutPreferenceStrength,
            allowedLayoutFamilies: allowedLayoutFamilies,
            minimumLayoutSimilarity: minimumLayoutSimilarity);
        InitialReservedPieceCount = ReservedPieceCount;

        VoyageSolutionResult rawFinal = null;
        foreach (var raw in SolveCurrent(useFastSolver, settings))
        {
            rawFinal = raw;
            yield return WithOffset(raw, exploredBefore, prunedBefore);
        }

        if (!useFastSolver)
        {
            WasSlowTimedOut = _slowPlanner?.TimedOut == true;
            if (_slowPlanner?.IsCancelled == true)
                yield break;
        }

        if (HasSolution(rawFinal))
            yield break;

        exploredBefore += rawFinal?.NodesExplored ?? 0;
        prunedBefore += rawFinal?.NodesPruned ?? 0;

        // A slow search can spend its whole time budget without reaching the first complete grid.
        // The exact topology/assignment solver is the deterministic recovery for that case.
        if (!useFastSolver)
        {
            FallbackStage = VoyageSolveFallbackStage.FastAfterSlow;
            RecoveredWithFastSolver = true;
            rawFinal = null;
            foreach (var raw in SolveCurrent(useFastSolver: true, settings: settings))
            {
                rawFinal = raw;
                yield return WithOffset(raw, exploredBefore, prunedBefore);
            }

            if (HasSolution(rawFinal))
                yield break;

            exploredBefore += rawFinal?.NodesExplored ?? 0;
            prunedBefore += rawFinal?.NodesPruned ?? 0;
        }

        // Relax only ordinary reservations. VoyagePlacementRules reapplies hard economic
        // stockpiles on this attempt, so premium charts are never restored as filler.
        FallbackStage = VoyageSolveFallbackStage.ReservationsRelaxed;
        RecoveredWithFastSolver = true;
        ConfigureAttempt(
            focusedPieces, tileBorders, strategyOptions,
            reserveCharts: false,
            disablePlacementRules: false,
            layoutPreference: layoutPreference,
            layoutPreferenceStrength: layoutPreferenceStrength,
            allowedLayoutFamilies: allowedLayoutFamilies,
            minimumLayoutSimilarity: minimumLayoutSimilarity);

        rawFinal = null;
        foreach (var raw in SolveCurrent(useFastSolver: true, settings: settings))
        {
            rawFinal = raw;
            yield return WithOffset(raw, exploredBefore, prunedBefore);
        }

        if (HasSolution(rawFinal))
            yield break;

        // Do not restore hard economic reservations or discard the selected strategy merely to
        // manufacture a result. The UI should report no valid filler voyage in this situation.
        yield break;
    }

    private void ConfigureAttempt(
        List<MapPiece> pieces,
        IReadOnlyList<BorderEffect>[,] tileBorders,
        VoyageStrategyOptions strategyOptions,
        bool reserveCharts,
        bool disablePlacementRules,
        VoyageLayoutPreference layoutPreference,
        double layoutPreferenceStrength,
        VoyageLayoutFamilies allowedLayoutFamilies,
        double minimumLayoutSimilarity)
    {
        Placement = VoyagePlacementRules.Apply(
            pieces,
            tileBorders,
            strategyOptions,
            reserveCharts,
            disablePlacementRules);
        if (!disablePlacementRules && FocusAnalysis.Active.Count > 0)
        {
            var labels = (Placement.ActiveStrategies ?? [])
                .Concat(FocusAnalysis.Active.Select(x => $"Foco: {VoyageFocusAnalysis.Label(x.Kind)}"))
                .Distinct(StringComparer.Ordinal)
                .ToList();
            Placement = Placement with { ActiveStrategies = labels };
        }
        Puzzle = new VoyagePuzzle(
            Placement.Pieces,
            tileBorders,
            Placement.Locks,
            AllowSacrificeCornerBorderDeadEnds: Placement.AmuletClamHubActive,
            PreferClamsAdjacentToAmulet: Placement.PreferClamsAdjacentToAmulet,
            LayoutPreference: layoutPreference,
            LayoutPreferenceStrength: layoutPreferenceStrength,
            AllowedLayoutFamilies: allowedLayoutFamilies,
            MinimumLayoutSimilarity: minimumLayoutSimilarity,
            ForbidStrongboxesWithBrine: strategyOptions.UseBrineKingSynergy);
        Scorer = new VoyageScorer(Puzzle);
    }

    private IEnumerable<VoyageSolutionResult> SolveCurrent(
        bool useFastSolver,
        VoyagePlannerSettings settings)
    {
        CurrentAttemptUsesFastSolver = useFastSolver;
        if (useFastSolver)
        {
            _slowPlanner = null;
            return new VoyagePlannerFast().Solve(Puzzle, settings);
        }

        _slowPlanner = new VoyagePlanner();
        return _slowPlanner.Solve(Puzzle, settings);
    }

    private static bool HasSolution(VoyageSolutionResult result) =>
        result?.Solutions is { Count: > 0 };

    private static VoyageSolutionResult WithOffset(
        VoyageSolutionResult result,
        long exploredBefore,
        long prunedBefore) =>
        new(
            result.Solutions,
            exploredBefore + result.NodesExplored,
            prunedBefore + result.NodesPruned);
}
