using System;
using System.Collections.Generic;
using System.Linq;

namespace DeepwaterEngagementSuite.VoyagePlannerData;

public enum VoyageFocusKind
{
    RareCurrency,
    BrineRareDensity,
    Sulphur,
    MessageBottles,
    GroundLootConversion,
    Strongboxes,
    MonsterDensity,
    NoConsume,
    UniqueRarity,
}

public sealed record VoyageFocusChoice(VoyageFocusKind Kind, double Score, string Reason);

public sealed record VoyageFocusAnalysis(
    IReadOnlyList<VoyageFocusChoice> Active,
    double FocusBonus,
    double OffFocusMultiplier)
{
    public static VoyageFocusAnalysis Disabled { get; } = new([], 0, 1);

    public bool Has(VoyageFocusKind kind) => Active.Any(x => x.Kind == kind);

    public VoyageStrategyOptions Concentrate(VoyageStrategyOptions options)
    {
        if (options == null || !options.AutomaticFocus)
            return options ?? VoyageStrategyOptions.AllEnabled;

        if (Active.Count == 0)
        {
            return options with
            {
                RareMonstersDrop = false,
                RareCurrencyStrongboxEngine = false,
                NoConsumeAnchorfield = false,
                CenterSpecialty = false,
                UniqueAmuletClamCross = false,
                UseBrineKingSynergy = false,
                DedicatedMessageActive = false,
                SulphurStrategyActive = false,
                GroundLootStrategyActive = false,
            };
        }

        var rarePlan = Has(VoyageFocusKind.RareCurrency) || Has(VoyageFocusKind.BrineRareDensity) ||
                       Has(VoyageFocusKind.GroundLootConversion);
        var groundLoot = Has(VoyageFocusKind.GroundLootConversion);
        return options with
        {
            RareMonstersDrop = options.RareMonstersDrop && rarePlan,
            RareCurrencyStrongboxEngine = options.RareCurrencyStrongboxEngine &&
                                            Has(VoyageFocusKind.RareCurrency),
            NoConsumeAnchorfield = options.NoConsumeAnchorfield && Has(VoyageFocusKind.NoConsume),
            CenterSpecialty = options.CenterSpecialty &&
                              (Has(VoyageFocusKind.Strongboxes) || Has(VoyageFocusKind.UniqueRarity) ||
                               Has(VoyageFocusKind.MessageBottles)),
            UniqueAmuletClamCross = options.UniqueAmuletClamCross && Has(VoyageFocusKind.UniqueRarity),
            UseBrineKingSynergy = options.ProtectBrineKing && Has(VoyageFocusKind.BrineRareDensity),
            DedicatedMessageActive = options.DedicatedLostMessageStrategy && Has(VoyageFocusKind.MessageBottles),
            SulphurStrategyActive = Has(VoyageFocusKind.Sulphur),
            GroundLootStrategyActive = groundLoot,
            SaveNoEquipment = options.SaveNoEquipment && !groundLoot,
            SaveGoldenLanterns = options.SaveGoldenLanterns && !groundLoot,
            SaveRarePossessed = options.SaveRarePossessed && !groundLoot,
            SavePantheon = options.SavePantheon || Has(VoyageFocusKind.BrineRareDensity) || groundLoot,
        };
    }

    public string Summary => Active.Count == 0
        ? "Livre (sem foco automático)"
        : string.Join(" + ", Active.Select(x => Label(x.Kind)));

    public List<MapPiece> ApplyWeights(IReadOnlyList<MapPiece> pieces)
    {
        if (Active.Count == 0 || FocusBonus <= 0)
            return pieces.ToList();

        return pieces.Select(piece => new MapPiece(
            piece.Id,
            piece.Type,
            piece.BaseConnections,
            piece.Modifiers.Select(mod => mod with
            {
                Weight = mod.Weight * WeightMultiplier(mod)
            }).ToList(),
            piece.Name)).ToList();
    }

    private double WeightMultiplier(Modifier mod)
    {
        if (mod.Name.Equals("Default", StringComparison.OrdinalIgnoreCase) || mod.Weight == 0)
            return 1;

        var matches = Active.Count(choice => Matches(choice.Kind, mod));
        if (matches > 0)
            return 1 + FocusBonus + Math.Max(0, matches - 1) * 0.15;

        return IsClassified(mod) ? Math.Clamp(OffFocusMultiplier, 0.1, 1) : 1;
    }

    private static bool IsClassified(Modifier mod) =>
        mod.Tags != ModifierTag.None ||
        Contains(mod.Name, "PackSize") || Contains(mod.Name, "ResourceFound") ||
        Contains(mod.Name, "Sulphur") || Contains(mod.Name, "Strongbox") ||
        Contains(mod.Name, "LostMessage") || Contains(mod.Name, "NoEquipment") ||
        Contains(mod.Name, "Possessed") || Contains(mod.Name, "GoldenLantern") ||
        Contains(mod.Name, "Starfish");

    private static bool Matches(VoyageFocusKind focus, Modifier mod)
    {
        var tags = mod.Tags;
        return focus switch
        {
            VoyageFocusKind.RareCurrency =>
                tags.HasFlag(ModifierTag.RareMonsters) ||
                tags.HasFlag(ModifierTag.Monsters) ||
                tags.HasFlag(ModifierTag.Strongboxes) ||
                Contains(mod.Name, "PackSize") || Contains(mod.Name, "Starfish") ||
                Contains(mod.Name, "IncreasedRare") || Contains(mod.Name, "Strongbox") ||
                Contains(mod.Name, "DivinerBox") || Contains(mod.Name, "ArcanistBox") ||
                Contains(mod.Name, "OperativeBox"),
            VoyageFocusKind.BrineRareDensity =>
                !tags.HasFlag(ModifierTag.Strongboxes) && !Contains(mod.Name, "Strongbox") &&
                (tags.HasFlag(ModifierTag.RareMonsters) || tags.HasFlag(ModifierTag.Monsters) ||
                 Contains(mod.Name, "IncreasedRare") || Contains(mod.Name, "Starfish") ||
                 Contains(mod.Name, "PackSize")),
            VoyageFocusKind.Sulphur =>
                tags.HasFlag(ModifierTag.Sulphur) || Contains(mod.Name, "Sulphur") ||
                Contains(mod.Name, "ResourceFound"),
            VoyageFocusKind.MessageBottles => Contains(mod.Name, "LostMessage"),
            VoyageFocusKind.GroundLootConversion =>
                Contains(mod.Name, "NoEquipment") || Contains(mod.Name, "Possessed") ||
                Contains(mod.Name, "GoldenLantern") || Contains(mod.Name, "Starfish") ||
                tags.HasFlag(ModifierTag.RareMonsters) || tags.HasFlag(ModifierTag.Lanterns),
            VoyageFocusKind.Strongboxes =>
                tags.HasFlag(ModifierTag.Strongboxes) || tags.HasFlag(ModifierTag.Scarabs) ||
                Contains(mod.Name, "Strongbox") || Contains(mod.Name, "DivinerBox") ||
                Contains(mod.Name, "ArcanistBox") || Contains(mod.Name, "OperativeBox"),
            VoyageFocusKind.MonsterDensity =>
                tags.HasFlag(ModifierTag.Monsters) || tags.HasFlag(ModifierTag.MagicMonsters) ||
                tags.HasFlag(ModifierTag.RareMonsters) || Contains(mod.Name, "PackSize"),
            VoyageFocusKind.NoConsume => false,
            VoyageFocusKind.UniqueRarity =>
                tags.HasFlag(ModifierTag.Uniques) || tags.HasFlag(ModifierTag.Rarity),
            _ => false,
        };
    }

    public static string Label(VoyageFocusKind kind) => kind switch
    {
        VoyageFocusKind.RareCurrency => "Moeda de monstros raros",
        VoyageFocusKind.BrineRareDensity => "Brine King + densidade de raros",
        VoyageFocusKind.Sulphur => "Dead Man's Sulphur",
        VoyageFocusKind.MessageBottles => "Messages in a Bottle",
        VoyageFocusKind.GroundLootConversion => "Conversão de ground loot",
        VoyageFocusKind.Strongboxes => "Strongboxes / Scarabs",
        VoyageFocusKind.MonsterDensity => "Pack size / densidade",
        VoyageFocusKind.NoConsume => "Não consumir charts",
        VoyageFocusKind.UniqueRarity => "Únicos / raridade",
        _ => kind.ToString(),
    };

    private static bool Contains(string value, string token) =>
        value?.Contains(token, StringComparison.OrdinalIgnoreCase) == true;
}

/// <summary>
/// Chooses a deliberately small set of mutually useful reward themes. Scores combine borders,
/// global chart implicits and the charts available to support them; the runner then amplifies the
/// selected themes and suppresses unrelated rewards.
/// </summary>
public static class VoyageFocusAnalyzer
{
    public static VoyageFocusAnalysis Analyze(
        IReadOnlyList<BorderEffect>[,] tileBorders,
        IReadOnlyList<MapPiece> pieces,
        VoyageStrategyOptions options)
    {
        if (options?.AutomaticFocus != true)
            return VoyageFocusAnalysis.Disabled;

        pieces ??= [];
        var score = Enum.GetValues<VoyageFocusKind>().ToDictionary(x => x, _ => 0d);
        var reasons = Enum.GetValues<VoyageFocusKind>().ToDictionary(x => x, _ => new List<string>());
        var borderNames = EnumerateBorders(tileBorders).Select(x => x.Name ?? "").ToList();

        var rareOrbStrength = borderNames.Sum(RareCurrencyBorderScore);
        var hasChartEffect = borderNames.Any(n => Contains(n, "ChartEffect"));
        var hasSulphurBorder = borderNames.Any(n => Contains(n, "SulphurDrops"));
        var hasScarabBorder = borderNames.Any(n => Contains(n, "MoreScarabs") || Contains(n, "RareMonsterScarab"));
        var hasCurrencyBorder = borderNames.Any(n => Contains(n, "MoreCurrency") || RareCurrencyBorderScore(n) > 0);
        var hasRareBoostBorder = borderNames.Any(n => Contains(n, "IncreasedRareMonsters") ||
                                                      Contains(n, "RareMonstersPerConnection") ||
                                                      Contains(n, "PackSize"));

        Add(VoyageFocusKind.RareCurrency, rareOrbStrength, rareOrbStrength > 0 ? "border de moeda por raro" : null);
        Add(VoyageFocusKind.Strongboxes, hasScarabBorder ? 80 : 0, hasScarabBorder ? "border de Scarabs" : null);
        Add(VoyageFocusKind.Strongboxes, hasCurrencyBorder ? 30 : 0, hasCurrencyBorder ? "border de Currency" : null);
        Add(VoyageFocusKind.NoConsume,
            borderNames.Count(n => Contains(n, "ChanceToNotConsumeChart")) >= 2 ? 55 : 0,
            "duas ou mais bordas de não consumir chart");
        Add(VoyageFocusKind.UniqueRarity,
            borderNames.Any(n => Contains(n, "MoreRarity")) ? 55 : 0,
            "border de raridade");

        var brines = pieces.Count(VoyagePlacementRules.IsBrineKingsDomain);
        var starfish = pieces.Count(VoyagePlacementRules.IsStarfishChart);
        var rareCharts = pieces.Count(p => VoyagePlacementRules.IsRareVoyageChart(p) ||
                                           VoyagePlacementRules.IsOrbRareComboChart(p));
        var boxes = pieces.Count(VoyagePlacementRules.IsStrongboxCountChart);
        var messages = pieces.Count(VoyagePlacementRules.IsLostMessageChart);
        var adjacentRare = pieces.Count(VoyagePlacementRules.IsAdjacentRareChart);
        var noEquipment = pieces.Count(VoyagePlacementRules.IsNoEquipmentChart);
        var possessed = pieces.Count(VoyagePlacementRules.IsRarePossessedChart);
        var golden = pieces.Count(VoyagePlacementRules.IsGoldenLanternsChart);
        var seaPillars = pieces.Count(VoyagePlacementRules.IsSeaPillars);
        var sulphurMods = pieces.SelectMany(p => p.Modifiers)
            .Where(IsSulphurModifier).ToList();
        var packMods = pieces.SelectMany(p => p.Modifiers)
            .Where(m => Contains(m.Name, "PackSize") || m.Tags.HasFlag(ModifierTag.Monsters)).ToList();
        var uniqueCharts = pieces.Count(p => VoyagePlacementRules.IsUniqueAmuletChart(p) ||
                                             VoyagePlacementRules.IsUniqueBeltChart(p) ||
                                             VoyagePlacementRules.IsUniqueRingChart(p));

        if (rareOrbStrength > 0)
            Add(VoyageFocusKind.RareCurrency, starfish * 13 + rareCharts * 10 + boxes * 18 + seaPillars * 28,
                boxes > 0 ? $"{boxes} charts de strongbox" : starfish > 0 ? $"{starfish} Giant Starfish" : null);
        if (brines > 0 && (hasRareBoostBorder || rareCharts + adjacentRare + starfish >= 2))
            Add(VoyageFocusKind.BrineRareDensity,
                90 + brines * 30 + rareCharts * 14 + adjacentRare * 16 + starfish * 18,
                $"{brines} Brine King; {rareCharts} globais; {adjacentRare + starfish} adjacentes");
        if (hasScarabBorder)
            Add(VoyageFocusKind.Strongboxes, boxes * 13, boxes > 0 ? $"{boxes} charts de strongbox" : null);
        if (hasSulphurBorder)
        {
            Add(VoyageFocusKind.Sulphur, sulphurMods.Count > 0 ? 140 : 0,
                sulphurMods.Count > 0 ? "border de Sulphur + charts globais" : null);
            Add(VoyageFocusKind.Sulphur,
                sulphurMods.Sum(m => Math.Max(4, Math.Abs(m.Value1) * 0.45 + m.Weight * 0.35)),
                sulphurMods.Count > 0 ? $"{sulphurMods.Count} mods de Sulphur" : null);
        }
        if (messages >= Math.Max(2, options.MinimumLostMessageCharts))
            Add(VoyageFocusKind.MessageBottles, 125 + messages * 32,
                $"{messages} charts de Message in a Bottle");
        var fullGroundLootCombo = noEquipment > 0 && possessed > 0 &&
                                  (golden + starfish >= 2) && seaPillars > 0;
        if (fullGroundLootCombo)
            Add(VoyageFocusKind.GroundLootConversion,
                150 + noEquipment * 20 + possessed * 25 + (golden + starfish) * 14,
                "No Equipment + Possessed + Golden/Starfish + Sea Pillars");
        Add(VoyageFocusKind.UniqueRarity, uniqueCharts * 14,
            uniqueCharts > 0 ? $"{uniqueCharts} rewards de unique" : null);

        if (rareOrbStrength > 0 && (starfish + rareCharts + boxes + seaPillars) > 0)
            Add(VoyageFocusKind.RareCurrency, 110, "combo raro + multiplicadores de raros");
        if (rareOrbStrength > 0 && boxes > 0)
        {
            Add(VoyageFocusKind.RareCurrency, 150, "strongboxes geram raros no tile premiado");
            Add(VoyageFocusKind.Strongboxes, 115, "engine de moeda por raro + strongboxes");
        }
        if (hasSulphurBorder && sulphurMods.Count > 0)
            Add(VoyageFocusKind.Sulphur, 85, "border + charts de Sulphur");
        if (hasSulphurBorder && hasChartEffect && sulphurMods.Count > 0)
            Add(VoyageFocusKind.Sulphur, 80, "Sulphur + chart effect");
        if (hasScarabBorder && boxes > 0)
            Add(VoyageFocusKind.Strongboxes, 85, "Scarabs + strongboxes");
        if (rareOrbStrength > 0 && packMods.Count > 0)
            Add(VoyageFocusKind.RareCurrency, 35, "pack size alimenta moeda por raro");

        var ordered = score
            .Select(kv => new VoyageFocusChoice(
                kv.Key,
                kv.Value,
                string.Join("; ", reasons[kv.Key].Distinct().Take(3))))
            .Where(x => x.Score >= Math.Max(1, options.MinimumFocusScore))
            .OrderByDescending(x => x.Score)
            .ThenBy(x => x.Kind)
            .ToList();

        if (ordered.Count == 0)
            return VoyageFocusAnalysis.Disabled;

        if (rareOrbStrength > 0)
        {
            var rareCurrency = ordered.FirstOrDefault(x => x.Kind == VoyageFocusKind.RareCurrency);
            if (rareCurrency != null)
            {
                return new VoyageFocusAnalysis(
                    [rareCurrency],
                    Math.Clamp(options.FocusWeightBonus, 0, 3),
                    Math.Clamp(options.OffFocusMultiplier, 0.1, 1));
            }
        }

        var max = rareOrbStrength > 0 ? 1 : Math.Clamp(options.MaxActiveFocuses, 1, 3);
        var selected = new List<VoyageFocusChoice> { ordered[0] };
        foreach (var candidate in ordered.Skip(1))
        {
            if (selected.Count >= max)
                break;
            if (candidate.Score < ordered[0].Score * Math.Clamp(options.SecondaryFocusRatio, 0.4, 1))
                continue;
            if (!Compatible(selected[0].Kind, candidate.Kind, rareOrbStrength > 0))
                continue;
            selected.Add(candidate);
        }

        return new VoyageFocusAnalysis(
            selected,
            Math.Clamp(options.FocusWeightBonus, 0, 3),
            Math.Clamp(options.OffFocusMultiplier, 0.1, 1));

        void Add(VoyageFocusKind kind, double value, string reason)
        {
            if (value <= 0)
                return;
            score[kind] += value;
            if (!string.IsNullOrWhiteSpace(reason))
                reasons[kind].Add(reason);
        }
    }

    private static IEnumerable<BorderEffect> EnumerateBorders(IReadOnlyList<BorderEffect>[,] borders)
    {
        if (borders == null)
            yield break;
        for (var row = 0; row < 3; row++)
        for (var col = 0; col < 3; col++)
        foreach (var border in borders[row, col] ?? [])
            yield return border;
    }

    private static bool IsSulphurModifier(Modifier mod) =>
        mod.Tags.HasFlag(ModifierTag.Sulphur) || Contains(mod.Name, "Sulphur") ||
        Contains(mod.Name, "ResourceFound");

    private static double RareCurrencyBorderScore(string name)
    {
        if (!Contains(name, "RareMonster"))
            return 0;
        if (Contains(name, "Divine")) return 180;
        if (Contains(name, "Annulment")) return 135;
        if (Contains(name, "Ancient")) return 95;
        if (Contains(name, "Exalted")) return 75;
        if (Contains(name, "Chaos")) return 55;
        if (Contains(name, "Scarab")) return 65;
        return Contains(name, "Regal") || Contains(name, "Gemcutter") ? 35 : 18;
    }

    private static bool Compatible(VoyageFocusKind primary, VoyageFocusKind secondary, bool hasRareOrb)
    {
        if (primary == secondary)
            return false;
        if ((primary == VoyageFocusKind.RareCurrency && secondary == VoyageFocusKind.MonsterDensity) ||
            (secondary == VoyageFocusKind.RareCurrency && primary == VoyageFocusKind.MonsterDensity))
            return true;
        if ((primary == VoyageFocusKind.RareCurrency && secondary == VoyageFocusKind.Strongboxes) ||
            (secondary == VoyageFocusKind.RareCurrency && primary == VoyageFocusKind.Strongboxes))
            return hasRareOrb;
        if ((primary == VoyageFocusKind.Sulphur && secondary == VoyageFocusKind.MonsterDensity) ||
            (secondary == VoyageFocusKind.Sulphur && primary == VoyageFocusKind.MonsterDensity))
            return true;
        return primary is VoyageFocusKind.NoConsume or VoyageFocusKind.UniqueRarity ||
               secondary is VoyageFocusKind.NoConsume or VoyageFocusKind.UniqueRarity;
    }

    private static bool Contains(string value, string token) =>
        value?.Contains(token, StringComparison.OrdinalIgnoreCase) == true;
}
