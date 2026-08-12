using System.Collections.Generic;

namespace DeepwaterEngagementSuite.VoyagePlannerData;

public record VoyagePuzzle(
    List<MapPiece> AvailablePieces,
    IReadOnlyList<BorderEffect>[,] TileBorders,
    List<LockedPlacement> LockedPlacements,
    bool AllowSacrificeCornerBorderDeadEnds = false,
    bool PreferClamsAdjacentToAmulet = false,
    VoyageLayoutPreference LayoutPreference = VoyageLayoutPreference.SnakeOrCompact,
    double LayoutPreferenceStrength = 0,
    VoyageLayoutFamilies AllowedLayoutFamilies = VoyageLayoutFamilies.All,
    double MinimumLayoutSimilarity = 0.62,
    bool ForbidStrongboxesWithBrine = false);
