using System;
using System.Collections.Generic;
using System.Numerics;

namespace DeepwaterEngagementSuite.VoyagePlannerData;

public enum VoyageLayoutPreference
{
    SnakeOrCompact,
    StraightLines,
}

public enum VoyageLayoutKind
{
    Other,
    SnakeS,
    Dollar,
    Compact,
    StraightLines,
}

[Flags]
public enum VoyageLayoutFamilies
{
    None = 0,
    SnakeDollar = 1 << 0,
    Compact = 1 << 1,
    StraightLines = 1 << 2,
    All = SnakeDollar | Compact | StraightLines,
}

public readonly record struct VoyageLayoutRating(
    VoyageLayoutKind Kind,
    double Similarity,
    double Bonus);

/// <summary>
/// Scores the twelve possible internal edges of the 3x3 board against useful traversal templates.
/// Weak boards prefer S/$/compact paths so the default lantern budget is not wasted on branches.
/// Premium border/chart combinations prefer a straight candelabra-style layout that exposes hubs.
/// </summary>
public static class VoyageLayoutScorer
{
    private const int GridSize = 3;
    private static readonly int[] SnakeTemplates = BuildTransforms(
        Horizontal(0, 0) | Horizontal(0, 1) |
        Horizontal(1, 0) | Horizontal(1, 1) |
        Horizontal(2, 0) | Horizontal(2, 1) |
        Vertical(0, 0) | Vertical(1, 2));

    private static readonly int[] DollarTemplates = BuildTransforms(
        SnakeTemplates[0] | Vertical(0, 1) | Vertical(1, 1));

    private static readonly int[] CompactTemplates = BuildTransforms(
        Horizontal(0, 0) | Horizontal(0, 1) |
        Horizontal(1, 0) | Horizontal(1, 1) |
        Horizontal(2, 0) | Horizontal(2, 1) |
        Vertical(0, 0) | Vertical(1, 0) |
        Vertical(0, 1) | Vertical(1, 1) |
        Vertical(0, 2) | Vertical(1, 2));

    private static readonly int[] StraightTemplates = BuildTransforms(
        Vertical(0, 0) | Vertical(1, 0) |
        Vertical(0, 1) | Vertical(1, 1) |
        Vertical(0, 2) | Vertical(1, 2) |
        Horizontal(1, 0) | Horizontal(1, 1));

    public static VoyageLayoutRating Rate(
        MapPiecePlacement[,] grid,
        VoyageLayoutPreference preference,
        double strength)
    {
        var mask = BuildEdgeMask(grid);
        return RateMask(mask, preference, strength);
    }

    public static VoyageLayoutRating RateTopology(
        IReadOnlyList<int> topology,
        VoyageLayoutPreference preference,
        double strength)
    {
        var mask = BuildEdgeMask(topology);
        return RateMask(mask, preference, strength);
    }

    public static double MaximumBonus(double strength) => Math.Max(0, strength);

    public static bool IsAllowed(
        MapPiecePlacement[,] grid,
        VoyageLayoutFamilies allowed,
        double minimumSimilarity = 0.62) =>
        IsAllowedMask(BuildEdgeMask(grid), allowed, minimumSimilarity);

    public static bool IsAllowedTopology(
        IReadOnlyList<int> topology,
        VoyageLayoutFamilies allowed,
        double minimumSimilarity = 0.62) =>
        IsAllowedMask(BuildEdgeMask(topology), allowed, minimumSimilarity);

    public static VoyageLayoutRating Classify(MapPiecePlacement[,] grid) =>
        ClassifyMask(BuildEdgeMask(grid));

    private static bool IsAllowedMask(int actual, VoyageLayoutFamilies allowed, double minimumSimilarity)
    {
        if (allowed is VoyageLayoutFamilies.None or VoyageLayoutFamilies.All)
            return true;

        var closest = ClassifyMask(actual);
        if (closest.Similarity < Math.Clamp(minimumSimilarity, 0.45, 1))
            return false;

        return closest.Kind switch
        {
            VoyageLayoutKind.SnakeS or VoyageLayoutKind.Dollar => allowed.HasFlag(VoyageLayoutFamilies.SnakeDollar),
            VoyageLayoutKind.Compact => allowed.HasFlag(VoyageLayoutFamilies.Compact),
            VoyageLayoutKind.StraightLines => allowed.HasFlag(VoyageLayoutFamilies.StraightLines),
            _ => false,
        };
    }

    private static VoyageLayoutRating ClassifyMask(int actual)
    {
        var best = Best(actual, SnakeTemplates, VoyageLayoutKind.SnakeS, 1.0);
        var dollar = Best(actual, DollarTemplates, VoyageLayoutKind.Dollar, 1.0);
        if (dollar.Similarity > best.Similarity) best = dollar;
        var compact = Best(actual, CompactTemplates, VoyageLayoutKind.Compact, 1.0);
        if (compact.Similarity > best.Similarity) best = compact;
        var straight = Best(actual, StraightTemplates, VoyageLayoutKind.StraightLines, 1.0);
        if (straight.Similarity > best.Similarity) best = straight;
        return new VoyageLayoutRating(best.Kind, best.Similarity, 0);
    }

    private static VoyageLayoutRating RateMask(
        int actual,
        VoyageLayoutPreference preference,
        double strength)
    {
        strength = Math.Max(0, strength);
        var candidates = preference == VoyageLayoutPreference.StraightLines
            ? new[] { Best(actual, StraightTemplates, VoyageLayoutKind.StraightLines, 1.0) }
            : new[]
            {
                Best(actual, SnakeTemplates, VoyageLayoutKind.SnakeS, 1.0),
                Best(actual, DollarTemplates, VoyageLayoutKind.Dollar, 0.97),
                Best(actual, CompactTemplates, VoyageLayoutKind.Compact, 0.92),
            };

        var best = candidates[0];
        for (var i = 1; i < candidates.Length; i++)
        {
            if (candidates[i].WeightedSimilarity > best.WeightedSimilarity)
                best = candidates[i];
        }

        // Avoid rewarding a topology merely because it shares a few common grid edges.
        var normalized = best.WeightedSimilarity <= 0.55
            ? 0
            : (best.WeightedSimilarity - 0.55) / 0.45;
        normalized = Math.Clamp(normalized, 0, 1);
        return new VoyageLayoutRating(best.Kind, best.Similarity, strength * normalized * normalized);
    }

    private static (VoyageLayoutKind Kind, double Similarity, double WeightedSimilarity) Best(
        int actual,
        IEnumerable<int> templates,
        VoyageLayoutKind kind,
        double preferenceWeight)
    {
        double best = 0;
        foreach (var target in templates)
        {
            var union = BitOperations.PopCount((uint)(actual | target));
            var intersection = BitOperations.PopCount((uint)(actual & target));
            var similarity = union == 0 ? 0 : (double)intersection / union;
            best = Math.Max(best, similarity);
        }

        return (kind, best, best * preferenceWeight);
    }

    private static int BuildEdgeMask(MapPiecePlacement[,] grid)
    {
        var mask = 0;
        for (var r = 0; r < GridSize; r++)
        for (var c = 0; c < GridSize; c++)
        {
            var conn = grid[r, c]?.Connections ?? Direction.None;
            if (c < GridSize - 1 && conn.HasFlag(Direction.Right))
                mask |= Horizontal(r, c);
            if (r < GridSize - 1 && conn.HasFlag(Direction.Up))
                mask |= Vertical(r, c);
        }

        return mask;
    }

    private static int BuildEdgeMask(IReadOnlyList<int> topology)
    {
        var mask = 0;
        for (var r = 0; r < GridSize; r++)
        for (var c = 0; c < GridSize; c++)
        {
            var conn = topology[r * GridSize + c];
            if (c < GridSize - 1 && (conn & (int)Direction.Right) != 0)
                mask |= Horizontal(r, c);
            if (r < GridSize - 1 && (conn & (int)Direction.Up) != 0)
                mask |= Vertical(r, c);
        }

        return mask;
    }

    private static int Horizontal(int row, int col) => 1 << (row * 2 + col);
    private static int Vertical(int row, int col) => 1 << (6 + row * 3 + col);

    private static int[] BuildTransforms(int mask)
    {
        var found = new HashSet<int>();
        var current = mask;
        for (var i = 0; i < 4; i++)
        {
            found.Add(current);
            found.Add(ReflectHorizontal(current));
            current = RotateClockwise(current);
        }

        var result = new int[found.Count];
        found.CopyTo(result);
        return result;
    }

    private static int RotateClockwise(int mask) => Transform(mask, (r, c) => (c, 2 - r));
    private static int ReflectHorizontal(int mask) => Transform(mask, (r, c) => (r, 2 - c));

    private static int Transform(int mask, Func<int, int, (int R, int C)> transform)
    {
        var result = 0;
        for (var r = 0; r < GridSize; r++)
        for (var c = 0; c < GridSize; c++)
        {
            if (c < GridSize - 1 && (mask & Horizontal(r, c)) != 0)
                result |= EdgeAfterTransform(r, c, r, c + 1, transform);
            if (r < GridSize - 1 && (mask & Vertical(r, c)) != 0)
                result |= EdgeAfterTransform(r, c, r + 1, c, transform);
        }

        return result;
    }

    private static int EdgeAfterTransform(
        int r1, int c1, int r2, int c2,
        Func<int, int, (int R, int C)> transform)
    {
        var a = transform(r1, c1);
        var b = transform(r2, c2);
        if (a.R == b.R)
            return Horizontal(a.R, Math.Min(a.C, b.C));
        return Vertical(Math.Min(a.R, b.R), a.C);
    }
}
