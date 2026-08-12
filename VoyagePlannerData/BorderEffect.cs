using System;

namespace DeepwaterEngagementSuite.VoyagePlannerData;

/// <summary>
/// A single border modifier touching one grid tile.
/// </summary>
/// <param name="Name">Raw border mod id (for display/debugging).</param>
/// <param name="Tags">Which reward categories this border boosts. <see cref="ModifierTag.All"/> matches everything.</param>
/// <param name="Multiplier">
/// Base multiplier. For per-connection borders this is the multiplier per single connection;
/// the effective multiplier is 1 + (Multiplier - 1) * connectionCount.
/// Quantity-per-connection borders instead use the independent additive fields below.
/// </param>
/// <param name="PerConnection">Scales with the connection count of the piece placed on the affected tile.</param>
/// <param name="AffectsPlacedChart">
/// True for borders that boost the chart placed on the tile (e.g. "increased effect of adjacent
/// Charts", chart refunds) — these multiply all of that chart's own modifiers, wherever their
/// value lands. False for borders that boost rewards materializing on the tile itself.
/// </param>
public record BorderEffect(
    string Name,
    ModifierTag Tags,
    double Multiplier,
    bool PerConnection,
    bool AffectsPlacedChart,
    double BaseAdditive = 0,
    double PerConnectionAdditive = 0)
{
    public bool HasAdditiveConnectionFormula =>
        BaseAdditive != 0 || PerConnectionAdditive != 0;

    /// <summary>
    /// Returns the effective multiplier for the chart currently occupying the affected tile.
    /// Most per-connection borders use the legacy multiplier formula. Quantity-per-connection
    /// borders carry two independent stats (a fixed increase and a reduction per connection),
    /// so they use 1 + base + perConnection * connections instead.
    /// </summary>
    public double EffectiveMultiplier(int connections)
    {
        connections = Math.Clamp(connections, 0, 4);
        if (HasAdditiveConnectionFormula)
            return Math.Max(0, 1 + BaseAdditive + PerConnectionAdditive * connections);
        return PerConnection
            ? Math.Max(0, 1 + (Multiplier - 1) * connections)
            : Math.Max(0, Multiplier);
    }
}
