using System;
using System.Collections.Generic;
using System.Linq;

namespace DeepwaterEngagementSuite.VoyagePlannerData;

public enum BorderLootTier
{
    Weak,
    Moderate,
    Rare,
    Divine,
}

public sealed record BorderEconomyOptions(
    int SulphurPerChaos = 130,
    double ExpectedRerollScore = 50,
    double ChaosPerLootPoint = 1,
    double RerollSafetyMargin = 1.1,
    double DivineChaos = 178.7,
    double AnnulmentChaos = 29.2,
    double AncientChaos = 6.54,
    double ExaltedChaos = 2.33,
    double GemcuttersChaos = 2.46,
    double ChaosOrbChaos = 1.0);

public sealed record BorderLootEntry(
    int Row,
    int Col,
    string Name,
    double Score,
    BorderLootTier Tier,
    string Reason);

public sealed record BorderLootAnalysis(
    double Score,
    BorderLootTier Tier,
    IReadOnlyList<BorderLootEntry> Entries,
    IReadOnlyList<string> Combos,
    bool HasPremiumChartImplicit,
    bool HasPremiumCombo,
    VoyageLayoutPreference LayoutPreference,
    int RerollsUsed,
    long NextRerollSulphur,
    double NextRerollChaos,
    double ExpectedUpgradePoints,
    double ExpectedUpgradeChaos,
    bool RecommendReroll,
    string RecommendationReason)
{
    public BorderLootEntry Find(int row, int col, string name) =>
        Entries.FirstOrDefault(x => x.Row == row && x.Col == col &&
                                    x.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
}

/// <summary>
/// Rates border mods by loot ceiling and, more importantly, by whether the available charts can
/// exploit them. The board score is intentionally dominated by the best two or three edges:
/// current successful Voyage strategies concentrate rewards on a small number of juiced tiles.
/// </summary>
public static class BorderLootAnalyzer
{
    private const string Prefix = "DeepwaterBorder";

    public static BorderLootAnalysis Analyze(
        IReadOnlyList<BorderEffect>[,] tileBorders,
        IReadOnlyList<MapPiece> pieces,
        int rerollsUsed,
        BorderEconomyOptions options)
    {
        pieces ??= [];
        options ??= new BorderEconomyOptions();
        rerollsUsed = Math.Clamp(rerollsUsed, 0, 20);

        var hasOperative = pieces.Any(VoyagePlacementRules.IsOperativeBoxChart);
        var hasBoxes = pieces.Any(VoyagePlacementRules.IsStrongboxCountChart);
        var hasRareVoyage = pieces.Any(VoyagePlacementRules.IsRareVoyageChart);
        var hasRare = pieces.Any(p => VoyagePlacementRules.IsOrbRareComboChart(p) ||
                                      VoyagePlacementRules.IsPelagic(p) ||
                                      VoyagePlacementRules.IsBrineKingsDomain(p) ||
                                      VoyagePlacementRules.IsStarfishChart(p) ||
                                      VoyagePlacementRules.IsRareVoyageChart(p));
        var hasNoEquipment = pieces.Any(VoyagePlacementRules.IsNoEquipmentChart);
        var hasLostMessage = pieces.Any(VoyagePlacementRules.IsLostMessageChart);
        var hasGolden = pieces.Any(VoyagePlacementRules.IsGoldenLanternsChart);
        var hasBrineKing = pieces.Any(VoyagePlacementRules.IsBrineKingsDomain);
        var hasSulphurCharts = pieces.SelectMany(p => p.Modifiers).Any(m =>
            m.Tags.HasFlag(ModifierTag.Sulphur) ||
            m.Name.Contains("ResourceFound", StringComparison.OrdinalIgnoreCase) ||
            m.Name.Contains("Sulphur", StringComparison.OrdinalIgnoreCase));
        var hasUnique = pieces.Any(p => VoyagePlacementRules.IsUniqueBeltChart(p) ||
                                        VoyagePlacementRules.IsUniqueRingChart(p) ||
                                        VoyagePlacementRules.IsUniqueAmuletChart(p));
        var hasPremiumAdjacency = pieces.Any(p => p.Modifiers.Any(m => !m.IsGlobal && m.Weight >= 80));
        var hasPremiumImplicit = hasOperative || hasBoxes || hasRareVoyage || hasLostMessage ||
                                 hasUnique || hasBrineKing || hasSulphurCharts || hasPremiumAdjacency;

        var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        for (var r = 0; r < 3; r++)
        for (var c = 0; c < 3; c++)
        foreach (var border in tileBorders?[r, c] ?? [])
            names.Add(border.Name);

        var hasMinimumMagic = names.Contains("DeepwaterBorderMonstersAtLeastMagic");
        var hasDeckConversion = names.Contains("DeepwaterBorderCurrencyToStackedDecks");
        var hasScarabBorder = names.Any(IsScarabBorder);
        var hasCurrencyBorder = names.Any(IsCurrencyBorder);
        var hasRareOrbBorder = names.Any(IsRareOrbBorder);
        var hasRarityBorder = names.Any(n => n.StartsWith("DeepwaterBorderMoreRarity", StringComparison.OrdinalIgnoreCase));
        var hasChartEffect = names.Any(n => n.StartsWith("DeepwaterBorderChartEffect", StringComparison.OrdinalIgnoreCase));
        var hasSulphurBorder = names.Any(n => n.Contains("SulphurDrops", StringComparison.OrdinalIgnoreCase));

        var combos = new List<string>();
        if (hasRareOrbBorder && hasRare)
            combos.Add("Orb drop + Rare Monsters");
        if (hasScarabBorder && (hasOperative || hasBoxes))
            combos.Add("Scarabs + Strongboxes");
        if (hasCurrencyBorder && (hasLostMessage || hasBoxes))
            combos.Add("Currency + Chests");
        if (hasDeckConversion && hasMinimumMagic && hasNoEquipment)
            combos.Add("Stacked Deck conversion + Magic + No Equipment");
        if (hasRarityBorder && hasUnique)
            combos.Add("Rarity + Unique rewards");
        if (hasChartEffect && hasPremiumAdjacency)
            combos.Add("Chart effect + premium adjacency");
        if (hasSulphurBorder && hasSulphurCharts)
            combos.Add("Sulphur border + Sulphur charts");
        if (hasSulphurBorder && hasChartEffect && hasSulphurCharts)
            combos.Add("Sulphur + chart effect + Sulphur charts");
        if (hasRareOrbBorder && hasBrineKing)
            combos.Add("Rare currency + Brine King's Domain");
        if (hasGolden && hasRarityBorder)
            combos.Add("Golden Lanterns + Rarity");

        var entries = new List<BorderLootEntry>();
        for (var r = 0; r < 3; r++)
        for (var c = 0; c < 3; c++)
        foreach (var border in tileBorders?[r, c] ?? [])
        {
            var (baseScore, reason) = BaseScore(border.Name, options);
            var synergy = 0d;

            if (IsRareOrbBorder(border.Name) && hasRare)
                synergy += 12;
            if (IsScarabBorder(border.Name) && hasOperative)
                synergy += 15;
            else if (IsScarabBorder(border.Name) && hasBoxes)
                synergy += 8;
            if (IsCurrencyBorder(border.Name) && (hasLostMessage || hasBoxes))
                synergy += 10;
            if (border.Name.Equals("DeepwaterBorderCurrencyToStackedDecks", StringComparison.OrdinalIgnoreCase) &&
                hasMinimumMagic && hasNoEquipment)
                synergy += 25;
            if (border.Name.StartsWith("DeepwaterBorderMoreRarity", StringComparison.OrdinalIgnoreCase) && hasUnique)
                synergy += 8;
            if (border.Name.StartsWith("DeepwaterBorderChartEffect", StringComparison.OrdinalIgnoreCase) && hasPremiumAdjacency)
                synergy += 10;
            if (border.Name.Equals("DeepwaterBorderGoldenLanterns", StringComparison.OrdinalIgnoreCase) && hasGolden)
                synergy += 10;
            if (border.Name.Equals("DeepwaterBorderSulphurDrops", StringComparison.OrdinalIgnoreCase) && hasSulphurCharts)
                synergy += hasChartEffect ? 24 : 14;
            if (IsRareOrbBorder(border.Name) && hasBrineKing)
                synergy += 15;

            var score = Math.Clamp(baseScore + synergy, 0, 100);
            if (synergy > 0)
                reason += $"; +{synergy:0} synergy";
            entries.Add(new BorderLootEntry(r, c, border.Name, score, TierFor(score), reason));
        }

        var ordered = entries.Select(e => e.Score).OrderByDescending(x => x).ToArray();
        var first = ordered.ElementAtOrDefault(0);
        var second = ordered.ElementAtOrDefault(1);
        var third = ordered.ElementAtOrDefault(2);
        var topWeighted = first * 0.50 + second * 0.30 + third * 0.20;
        var breadth = ordered.Take(6).DefaultIfEmpty().Average();
        var comboBonus = Math.Min(18, combos.Count * 6);
        var scoreBoard = Math.Clamp(Math.Max(first * 0.80, topWeighted * 0.85 + breadth * 0.15) + comboBonus, 0, 100);

        var hasPremiumCombo = combos.Count > 0 &&
                              (scoreBoard >= 55 || combos.Any(c => c.StartsWith("Orb drop", StringComparison.Ordinal)));
        var layout = hasPremiumCombo || (scoreBoard >= 80 && hasPremiumImplicit)
            ? VoyageLayoutPreference.StraightLines
            : VoyageLayoutPreference.SnakeOrCompact;

        var sulphurPerChaos = Math.Max(1, options.SulphurPerChaos);
        var nextCost = 3_000L * (1L << Math.Min(rerollsUsed, 20));
        var nextCostChaos = (double)nextCost / sulphurPerChaos;
        var upgradePoints = Math.Max(0, options.ExpectedRerollScore - scoreBoard);
        var upgradeChaos = upgradePoints * Math.Max(0, options.ChaosPerLootPoint);
        var requiredChaos = nextCostChaos * Math.Max(0.1, options.RerollSafetyMargin);
        var reroll = !hasPremiumCombo && scoreBoard < 80 && upgradeChaos > requiredChaos;

        string recommendation;
        if (hasPremiumCombo)
            recommendation = "KEEP: compatible premium combo found";
        else if (scoreBoard >= 80)
            recommendation = "KEEP: Divine-tier border potential";
        else if (upgradePoints <= 0)
            recommendation = "KEEP: board is at or above the expected reroll result";
        else if (reroll)
            recommendation = $"REROLL: expected upgrade ({upgradeChaos:0.0}c) exceeds guarded cost ({requiredChaos:0.0}c)";
        else
            recommendation = $"KEEP: next reroll cost ({nextCostChaos:0.0}c) exceeds expected upgrade ({upgradeChaos:0.0}c)";

        return new BorderLootAnalysis(
            scoreBoard, TierFor(scoreBoard), entries, combos,
            hasPremiumImplicit, hasPremiumCombo, layout,
            rerollsUsed, nextCost, nextCostChaos,
            upgradePoints, upgradeChaos, reroll, recommendation);
    }

    public static BorderLootTier TierFor(double score) => score switch
    {
        >= 80 => BorderLootTier.Divine,
        >= 55 => BorderLootTier.Rare,
        >= 30 => BorderLootTier.Moderate,
        _ => BorderLootTier.Weak,
    };

    private static bool IsRareOrbBorder(string name) => name is not null &&
        (name.Equals("DeepwaterBorderRareMonsterDivine", StringComparison.OrdinalIgnoreCase) ||
         name.Equals("DeepwaterBorderRareMonsterExalted", StringComparison.OrdinalIgnoreCase) ||
         name.Equals("DeepwaterBorderRareMonsterAnnulment", StringComparison.OrdinalIgnoreCase) ||
         name.Equals("DeepwaterBorderRareMonsterAncient", StringComparison.OrdinalIgnoreCase));

    private static bool IsScarabBorder(string name) => name is not null &&
        (name.StartsWith("DeepwaterBorderMoreScarabs", StringComparison.OrdinalIgnoreCase) ||
         name.Equals("DeepwaterBorderRareMonsterScarab", StringComparison.OrdinalIgnoreCase));

    private static bool IsCurrencyBorder(string name) => name is not null &&
        (name.StartsWith("DeepwaterBorderMoreCurrency", StringComparison.OrdinalIgnoreCase) ||
         name.Equals("DeepwaterBorderCurrencyToStackedDecks", StringComparison.OrdinalIgnoreCase) ||
         IsRareOrbBorder(name));

    private static (double Score, string Reason) BaseScore(string rawName, BorderEconomyOptions options)
    {
        var n = rawName ?? "";
        if (n.Equals(Prefix + "RareMonsterDivine", StringComparison.OrdinalIgnoreCase)) return CurrencyBorder(options.DivineChaos, options, "Divine");
        if (n.Equals(Prefix + "RareMonsterExalted", StringComparison.OrdinalIgnoreCase)) return CurrencyBorder(options.ExaltedChaos, options, "Exalted");
        if (n.Equals(Prefix + "RareMonsterAncient", StringComparison.OrdinalIgnoreCase)) return CurrencyBorder(options.AncientChaos, options, "Ancient Orb");
        if (n.Equals(Prefix + "RareMonsterAnnulment", StringComparison.OrdinalIgnoreCase)) return CurrencyBorder(options.AnnulmentChaos, options, "Annulment");
        if (n.Equals(Prefix + "RareMonsterScarab", StringComparison.OrdinalIgnoreCase)) return (65, "scarab per rare");
        if (n.Equals(Prefix + "CurrencyToStackedDecks", StringComparison.OrdinalIgnoreCase)) return (58, "currency conversion combo");
        if (n.Equals(Prefix + "RareMonsterChaos", StringComparison.OrdinalIgnoreCase)) return CurrencyBorder(options.ChaosOrbChaos, options, "Chaos Orb");
        if (n.Equals(Prefix + "RareMonsterGemcutters", StringComparison.OrdinalIgnoreCase)) return CurrencyBorder(options.GemcuttersChaos, options, "GCP");
        if (n.Equals(Prefix + "RareMonsterSupport", StringComparison.OrdinalIgnoreCase)) return (48, "support gem chance");
        if (n.Equals(Prefix + "RareMonsterRegal", StringComparison.OrdinalIgnoreCase)) return (31, "regal per rare");
        if (n.Equals(Prefix + "RareMonsterVaal", StringComparison.OrdinalIgnoreCase) ||
            n.Equals(Prefix + "RareMonsterChromatic", StringComparison.OrdinalIgnoreCase) ||
            n.Equals(Prefix + "RareMonsterRegret", StringComparison.OrdinalIgnoreCase) ||
            n.Equals(Prefix + "RareMonsterBlessed", StringComparison.OrdinalIgnoreCase)) return (20, "low-value currency per rare");

        if (n.StartsWith(Prefix + "MoreScarabs", StringComparison.OrdinalIgnoreCase))
            return Tiered(n, 46, 61, 73, "more scarabs");
        if (n.StartsWith(Prefix + "MoreCurrency", StringComparison.OrdinalIgnoreCase))
            return Tiered(n, 42, 56, 68, "more currency");
        if (n.StartsWith(Prefix + "ChartEffect", StringComparison.OrdinalIgnoreCase))
            return Tiered(n, 40, 58, 68, "increased chart effect");
        if (n.StartsWith(Prefix + "MoreRarity", StringComparison.OrdinalIgnoreCase))
            return Tiered(n, 32, 45, 55, "more rarity (needs matching reward)");
        if (n.StartsWith(Prefix + "ChanceToNotConsumeChart", StringComparison.OrdinalIgnoreCase))
            return Tiered(n, 42, 58, 64, "chart preservation");
        if (n.StartsWith(Prefix + "QuantityPerConnection", StringComparison.OrdinalIgnoreCase))
            return Tiered(n, 42, 55, 64, "quantity with connection penalty");
        if (n.StartsWith(Prefix + "RareMonstersPerConnection", StringComparison.OrdinalIgnoreCase))
            return Tiered(n, 38, 51, 60, "rare monsters per connection");
        if (n.StartsWith(Prefix + "IncreasedRareMonsters", StringComparison.OrdinalIgnoreCase))
            return Tiered(n, 28, 37, 46, "increased rare monsters");
        if (n.StartsWith(Prefix + "TreasureAnchors", StringComparison.OrdinalIgnoreCase))
            return Tiered(n, 35, 55, 62, "currency treasure anchors");
        if (n.StartsWith(Prefix + "PackSize", StringComparison.OrdinalIgnoreCase))
            return Tiered(n, 24, 31, 38, "pack size without direct reward");

        if (n.Equals(Prefix + "GoldenLanterns", StringComparison.OrdinalIgnoreCase)) return (38, "player quantity/rarity; weak for boxes alone");
        if (n.Equals(Prefix + "SulphurDrops", StringComparison.OrdinalIgnoreCase)) return (36, "funds future rerolls/crafting");
        if (n.Equals(Prefix + "InfiniteLanterns", StringComparison.OrdinalIgnoreCase)) return (34, "navigation safety");
        if (n.Equals(Prefix + "GiantOctopus", StringComparison.OrdinalIgnoreCase)) return (33, "resource encounter");
        if (n.Equals(Prefix + "RandomDucatChest", StringComparison.OrdinalIgnoreCase)) return (31, "ducat chest");
        if (n.StartsWith(Prefix + "EquipmentToGold", StringComparison.OrdinalIgnoreCase)) return (25, "equipment conversion");
        if (n.Equals(Prefix + "MonstersAtLeastMagic", StringComparison.OrdinalIgnoreCase)) return (30, "combo enabler");
        if (n.StartsWith(Prefix + "Additional", StringComparison.OrdinalIgnoreCase)) return (20, "extra monsters without direct reward");
        if (n.StartsWith(Prefix + "ExpGain", StringComparison.OrdinalIgnoreCase)) return (8, "experience, not loot");
        return (15, "low or unclassified loot potential");
    }

    private static (double Score, string Reason) CurrencyBorder(
        double chaosValue,
        BorderEconomyOptions options,
        string currency)
    {
        chaosValue = Math.Max(0.01, chaosValue);
        var reference = Math.Max(chaosValue, options.DivineChaos);
        var normalized = Math.Log(1 + chaosValue) / Math.Log(1 + reference);
        var score = Math.Clamp(20 + normalized * 80, 20, 100);
        return (score, $"{currency} por raro (~{chaosValue:0.##}c no snapshot configurado)");
    }

    private static (double Score, string Reason) Tiered(
        string name, double tier1, double tier2, double tier3, string reason)
    {
        var tier = name.Length > 0 && char.IsDigit(name[^1]) ? name[^1] - '0' : 1;
        var score = tier switch { >= 3 => tier3, 2 => tier2, _ => tier1 };
        return (score, $"{reason} T{tier}");
    }
}
