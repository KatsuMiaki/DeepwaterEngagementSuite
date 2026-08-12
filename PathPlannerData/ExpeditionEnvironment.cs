using GameOffsets.Native;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace DeepwaterEngagementSuite.PathPlannerData;

public record ExpeditionEnvironment(
    List<(Vector2, IExpeditionLoot)> Loot,
    float BubbleRadius,
    int MaxBubbles,
    Func<Vector2, bool> IsValidPlacement,
    Func<Vector2, float> GetWalkableCoverage,
    List<(Vector2i Position, float Radius)> Bubbles,
    bool IsVoyage,
    int TotalRemainingBubbles);
