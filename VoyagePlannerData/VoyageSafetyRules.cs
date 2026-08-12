using System.Linq;

namespace DeepwaterEngagementSuite.VoyagePlannerData;

public static class VoyageSafetyRules
{
    public static bool IsSafe(MapPiecePlacement[,] grid, bool forbidStrongboxesWithBrine)
    {
        var pieces = grid.Cast<MapPiecePlacement>()
            .Where(x => x != null)
            .Select(x => x.Piece)
            .ToList();

        var hasBrine = pieces.Any(VoyagePlacementRules.IsBrineKingsDomain);
        if (forbidStrongboxesWithBrine && hasBrine &&
            pieces.Any(VoyagePlacementRules.IsStrongboxCountChart))
            return false;

        var hasPantheon = pieces.Any(VoyagePlacementRules.IsPantheonChart);
        return !hasPantheon ||
               (!hasBrine && !pieces.Any(VoyagePlacementRules.IsRarePossessedChart));
    }
}
