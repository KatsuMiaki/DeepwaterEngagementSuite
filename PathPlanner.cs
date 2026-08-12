using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using DeepwaterEngagementSuite.PathPlannerData;
using ExileCore.Shared.Helpers;
using GameOffsets.Native;

namespace DeepwaterEngagementSuite;

public class PathPlanner
{
    public record PerPointLootScore(Vector2i Point, double ScoreDiff, int NewRelics, int Loot);

    public record DetailedLootScore(List<PerPointLootScore> PerPointScore, double TotalScore, ExpeditionEnvironment Environment);

    private readonly Dictionary<object, double> _lootValueTable = new(ReferenceEqualityComparer.Instance);
    private readonly PlannerSettings _settings;
    private readonly int _validatedPoints;
    private readonly Random _random = new(Random.Shared.Next());
    private int _pathsBuilt;

    private readonly record struct PlannerBubble(Vector2 Center, float Radius);

    public PathPlanner(PlannerSettings settings)
    {
        _settings = settings;
        _validatedPoints = _settings.ValidatedIntermediatePoints.Value + 1;
    }

    public double GetScore(List<Vector2i> state, ExpeditionEnvironment environment)
    {
        var lootList = GetInitiallyCoveredLoot(environment);
        var score = 0.0;
        foreach (var lantern in state)
        {
            var localScore = environment.Loot
                .Where(x => x.Item1.DistanceLessThanOrEqual(lantern, environment.BubbleRadius))
                .Where(x => lootList.Add(x.Item2))
                .Sum(x => GetLootWeight(x.Item2));

            score += localScore - GetTerrainPenalty(lantern, environment) - GetLanternCost(environment);
        }

        return score;
    }

    public DetailedLootScore GetDetailedScore(List<Vector2i> state, ExpeditionEnvironment environment)
    {
        var lootList = GetInitiallyCoveredLoot(environment);
        var scorePerPoint = new List<PerPointLootScore>();
        var score = 0.0;
        foreach (var lantern in state)
        {
            var newLoot = 0;
            var localScore = 0.0;
            foreach (var (_, loot) in environment.Loot
                         .Where(x => x.Item1.DistanceLessThanOrEqual(lantern, environment.BubbleRadius))
                         .Where(x => lootList.Add(x.Item2)))
            {
                newLoot++;
                localScore += GetLootWeight(loot);
            }

            localScore -= GetTerrainPenalty(lantern, environment) + GetLanternCost(environment);
            scorePerPoint.Add(new PerPointLootScore(lantern, localScore, 0, newLoot));
            score += localScore;
        }

        return new DetailedLootScore(scorePerPoint, score, environment);
    }

    public void Init(ExpeditionEnvironment environment)
    {
        _lootValueTable.Clear();
        foreach (var (_, loot) in environment.Loot)
        {
            _lootValueTable[loot] = loot switch
            {
                Chest { Type: var type } => _settings.ChestSettingsMap.GetValueOrDefault(type, new ChestSettings()).Weight,
                _ => 0,
            };
        }

        _lootValueTable.TrimExcess();
    }

    public IEnumerable<PathState> GetBestPathSeries(ExpeditionEnvironment environment)
    {
        if (environment.MaxBubbles <= 0 || environment.Bubbles.Count == 0)
        {
            yield return new PathState([], 0);
            yield break;
        }

        List<Vector2i> bestPath = [];
        var bestScore = double.NegativeInfinity;
        var pathsPerGeneration = Math.Clamp(_settings.PathGenerationSize.Value, 4, 64);

        while (true)
        {
            for (var i = 0; i < pathsPerGeneration; i++)
            {
                var path = BuildSmartPath(environment);
                var score = GetScore(path, environment);
                if (score > bestScore || bestPath.Count == 0)
                {
                    bestScore = score;
                    bestPath = path;
                }
            }

            yield return new PathState(bestPath, bestScore);
        }
    }

    private List<Vector2i> BuildSmartPath(ExpeditionEnvironment environment)
    {
        var points = new List<Vector2i>(environment.MaxBubbles);
        var anchors = environment.Bubbles
            .Select(x => new PlannerBubble(ToVector2(x.Position), x.Radius))
            .ToList();
        var deterministic = _pathsBuilt++ == 0;
        var lastRealLootStep = -1;

        for (var step = 0; step < environment.MaxBubbles; step++)
        {
            var alreadyCovered = GetCoveredLoot(environment, points);
            var targets = environment.Loot
                .Where(x => !alreadyCovered.Contains(x.Item2))
                .OrderByDescending(x => GetLootWeight(x.Item2))
                .Take(48)
                .ToList();

            var candidates = GenerateCandidates(targets, anchors, environment);
            var scored = candidates
                .Where(x => environment.IsValidPlacement(ToVector2(x)))
                .Where(x => IsConnected(x, anchors, environment.BubbleRadius))
                .Where(x => anchors.All(a => Vector2.Distance(a.Center, ToVector2(x)) >= environment.BubbleRadius * 0.25f))
                .Select(x =>
                {
                    var coverage = environment.GetWalkableCoverage(ToVector2(x));
                    var value = ScoreCandidate(x, coverage, targets, anchors, step, environment);
                    return (Point: x, Value: value, Coverage: coverage);
                })
                .OrderByDescending(x => x.Value)
                .ToList();

            if (scored.Count == 0)
                break;

            var minimumCoverage = _settings.MinimumWalkableCoveragePercent.Value / 100f;
            var efficient = scored.Where(x => x.Coverage >= minimumCoverage).ToList();
            var selectable = efficient.Count > 0
                ? efficient
                : scored.Where(x => x.Coverage >= Math.Min(0.35f, minimumCoverage)).ToList();

            if (selectable.Count == 0)
                selectable = scored;

            var poolSize = Math.Min(selectable.Count, deterministic ? 1 : 4);
            var selectedIndex = poolSize == 1 || _random.NextDouble() < 0.62
                ? 0
                : _random.Next(poolSize);
            var selected = selectable[selectedIndex].Point;

            points.Add(selected);
            anchors.Add(new PlannerBubble(ToVector2(selected), environment.BubbleRadius));

            if (targets.Any(x => IsRealLoot(x.Item2) &&
                                 Vector2.Distance(x.Item1, ToVector2(selected)) <= environment.BubbleRadius))
            {
                lastRealLootStep = points.Count - 1;
            }
        }

        if (environment.IsVoyage && _settings.VoyageTrimAfterLastLoot.Value)
        {
            var usefulLength = lastRealLootStep >= 0
                ? lastRealLootStep + 1
                : Math.Min(points.Count, _settings.VoyageExplorationSteps.Value);
            return points.Take(usefulLength).ToList();
        }

        return points;
    }

    private HashSet<Vector2i> GenerateCandidates(
        List<(Vector2, IExpeditionLoot)> targets,
        List<PlannerBubble> anchors,
        ExpeditionEnvironment environment)
    {
        var result = new HashSet<Vector2i>();
        var radius = environment.BubbleRadius;

        foreach (var (target, _) in targets)
        {
            var nearest = anchors.MinBy(x => Vector2.Distance(x.Center, target));
            var offset = target - nearest.Center;
            var distance = offset.Length();
            var direction = distance > 0.001f ? offset / distance : Vector2.UnitX;
            var maxConnectionDistance = (nearest.Radius + radius) * 0.90f;

            if (distance <= maxConnectionDistance)
                AddCandidate(result, target);

            var travel = Math.Clamp(distance - radius * 0.55f, radius * 0.45f, maxConnectionDistance);
            var directed = nearest.Center + direction * travel;
            AddCandidate(result, directed);

            var perpendicular = new Vector2(-direction.Y, direction.X) * radius * 0.22f;
            AddCandidate(result, directed + perpendicular);
            AddCandidate(result, directed - perpendicular);
        }

        var angularSamples = Math.Clamp(12 + _validatedPoints * 4, 12, 36);
        var frontierAnchors = anchors.TakeLast(Math.Min(anchors.Count, 5));
        foreach (var anchor in frontierAnchors)
        {
            var connectionDistance = (anchor.Radius + radius) * (0.78f + (float)_random.NextDouble() * 0.12f);
            var phase = (float)_random.NextDouble() * MathF.Tau;
            for (var i = 0; i < angularSamples; i++)
            {
                var angle = phase + MathF.Tau * i / angularSamples;
                var direction = new Vector2(MathF.Cos(angle), MathF.Sin(angle));
                AddCandidate(result, anchor.Center + direction * connectionDistance);
            }
        }

        return result;
    }

    private double ScoreCandidate(
        Vector2i candidate,
        float coverage,
        List<(Vector2, IExpeditionLoot)> targets,
        List<PlannerBubble> anchors,
        int step,
        ExpeditionEnvironment environment)
    {
        var position = ToVector2(candidate);
        var radius = environment.BubbleRadius;
        var remainingSteps = Math.Max(0, environment.MaxBubbles - step - 1);
        var maxAdvance = radius * 1.8f;
        var immediate = 0.0;
        var future1 = 0.0;
        var future2 = 0.0;
        var future3 = 0.0;
        foreach (var (targetPosition, loot) in targets)
        {
            var distance = Vector2.Distance(targetPosition, position);
            var weight = GetLootWeight(loot);
            if (distance <= radius)
            {
                immediate += weight;
                continue;
            }

            var remainingDistance = Math.Max(0, distance - radius);
            var stepsNeeded = (int)Math.Ceiling(remainingDistance / maxAdvance);
            if (stepsNeeded > remainingSteps)
                continue;

            var potential = weight / (stepsNeeded + 1);
            if (potential > future1)
            {
                future3 = future2;
                future2 = future1;
                future1 = potential;
            }
            else if (potential > future2)
            {
                future3 = future2;
                future2 = potential;
            }
            else if (potential > future3)
            {
                future3 = potential;
            }
        }

        var future = (future1 + future2 + future3) * 0.35;

        var nearestAnchor = anchors.Min(x => Vector2.Distance(x.Center, position));
        var extension = Math.Clamp(nearestAnchor / Math.Max(1f, radius * 1.8f), 0f, 1f) * 5;
        var coverageBonus = coverage * 8;

        return immediate + future + extension + coverageBonus -
               GetTerrainPenalty(candidate, environment) - GetLanternCost(environment);
    }

    private HashSet<IExpeditionLoot> GetInitiallyCoveredLoot(ExpeditionEnvironment environment) =>
        environment.Loot
            .Where(x => environment.Bubbles.Any(b =>
                b.Position.DistanceLessThanOrEqual(x.Item1.TruncateToVector2I(), b.Radius)))
            .Select(x => x.Item2)
            .ToHashSet();

    private HashSet<IExpeditionLoot> GetCoveredLoot(ExpeditionEnvironment environment, List<Vector2i> points)
    {
        var covered = GetInitiallyCoveredLoot(environment);
        foreach (var point in points)
        {
            foreach (var (_, loot) in environment.Loot
                         .Where(x => x.Item1.DistanceLessThanOrEqual(point, environment.BubbleRadius)))
            {
                covered.Add(loot);
            }
        }

        return covered;
    }

    private double GetTerrainPenalty(Vector2i point, ExpeditionEnvironment environment)
    {
        var coverage = environment.GetWalkableCoverage(ToVector2(point));
        var minimum = _settings.MinimumWalkableCoveragePercent.Value / 100f;
        var waste = 1f - coverage;
        var belowMinimum = Math.Max(0f, minimum - coverage);
        return (waste + belowMinimum * 2f) * _settings.TerrainWastePenalty.Value;
    }

    private double GetLanternCost(ExpeditionEnvironment environment) =>
        environment.IsVoyage ? _settings.VoyageLanternCostPenalty.Value : 0;

    private static bool IsRealLoot(IExpeditionLoot loot) =>
        loot is not Chest { Type: IconPickerIndex.PointerTarget };

    private double GetLootWeight(IExpeditionLoot loot) => _lootValueTable.GetValueOrDefault(loot, 0);

    private static bool IsConnected(Vector2i point, List<PlannerBubble> anchors, float radius)
    {
        var position = ToVector2(point);
        return anchors.Any(anchor =>
            Vector2.Distance(anchor.Center, position) <= (anchor.Radius + radius) * 0.96f);
    }

    private static void AddCandidate(HashSet<Vector2i> candidates, Vector2 point) =>
        candidates.Add(new Vector2i((int)MathF.Round(point.X), (int)MathF.Round(point.Y)));

    private static Vector2 ToVector2(Vector2i point) => new(point.X, point.Y);
}
