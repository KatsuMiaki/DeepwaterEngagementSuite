using System;
using System.Collections.Generic;
using System.Linq;

namespace DeepwaterEngagementSuite.VoyagePlannerData;

public enum VoyageFocusKind
{
    RareCurrency,
    OperativeFleet,
    DivinerFleet,
    Sulphur,
    MessageBottles,
    BrineRareDensity,
    FastVoyage,
}

public sealed record VoyageFocusChoice(VoyageFocusKind Kind, double Score, string Reason);

/// <summary>
/// A voyage has exactly one economic plan. A premium plan is only enabled after its complete
/// support package exists; otherwise the planner builds a cheap voyage and keeps that package.
/// </summary>
public sealed record VoyageFocusAnalysis(
    IReadOnlyList<VoyageFocusChoice> Active,
    double FocusBonus,
    double OffFocusMultiplier)
{
    public static VoyageFocusAnalysis Disabled { get; } = new([], 0, 1);

    public bool Has(VoyageFocusKind kind) => Active.Any(x => x.Kind == kind);

    public VoyageLayoutFamilies RequiredLayoutFamilies => Active.FirstOrDefault()?.Kind switch
    {
        VoyageFocusKind.RareCurrency => VoyageLayoutFamilies.StraightLines,
        VoyageFocusKind.MessageBottles => VoyageLayoutFamilies.Compact,
        VoyageFocusKind.BrineRareDensity => VoyageLayoutFamilies.Compact,
        _ => VoyageLayoutFamilies.None,
    };

    public VoyageStrategyOptions Concentrate(VoyageStrategyOptions options)
    {
        if (options == null || !options.AutomaticFocus)
            return options ?? VoyageStrategyOptions.AllEnabled;

        return options with
        {
            RareMonstersDrop = Has(VoyageFocusKind.RareCurrency) || Has(VoyageFocusKind.BrineRareDensity),
            RareCurrencyStrongboxEngine = Has(VoyageFocusKind.RareCurrency),
            UseBrineKingSynergy = Has(VoyageFocusKind.BrineRareDensity),
            DedicatedMessageActive = Has(VoyageFocusKind.MessageBottles),
            SulphurStrategyActive = Has(VoyageFocusKind.Sulphur),
            OperativeFleetActive = Has(VoyageFocusKind.OperativeFleet),
            DivinerFleetActive = Has(VoyageFocusKind.DivinerFleet),
            FastVoyageActive = Has(VoyageFocusKind.FastVoyage),
            NoConsumeAnchorfield = false,
            CenterSpecialty = false,
            UniqueAmuletClamCross = false,
            GroundLootStrategyActive = false,
            SaveKishara = false,
            SaveNoEquipment = false,
            SaveFractured = false,
            SaveGoldenLanterns = false,
            SavePantheon = false,
            SaveSoulEater = false,
            SaveRareFracture = false,
            SaveRarePossessed = false,
        };
    }

    public string Summary => Active.Count == 0 ? "Planejamento manual" : Label(Active[0].Kind);

    public List<MapPiece> ApplyWeights(IReadOnlyList<MapPiece> pieces)
    {
        if (Active.Count == 0 || FocusBonus <= 0)
            return pieces.ToList();

        return pieces.Select(piece => new MapPiece(
            piece.Id,
            piece.Type,
            piece.BaseConnections,
            piece.Modifiers.Select(mod => mod with { Weight = mod.Weight * WeightMultiplier(mod) }).ToList(),
            piece.Name)).ToList();
    }

    private double WeightMultiplier(Modifier mod)
    {
        if (mod.Name.Equals("Default", StringComparison.OrdinalIgnoreCase) || mod.Weight == 0)
            return 1;
        if (Matches(Active[0].Kind, mod))
            return 1 + FocusBonus;
        return IsClassified(mod) ? Math.Clamp(OffFocusMultiplier, 0.05, 1) : 1;
    }

    private static bool Matches(VoyageFocusKind focus, Modifier mod)
    {
        var name = mod.Name ?? "";
        var tags = mod.Tags;
        return focus switch
        {
            VoyageFocusKind.RareCurrency or VoyageFocusKind.BrineRareDensity =>
                tags.HasFlag(ModifierTag.RareMonsters) || tags.HasFlag(ModifierTag.Monsters) ||
                Contains(name, "IncreasedRare") || Contains(name, "PackSize") ||
                Contains(name, "Starfish") || Contains(name, "Strongbox"),
            VoyageFocusKind.OperativeFleet => Contains(name, "OperativeBox"),
            VoyageFocusKind.DivinerFleet => Contains(name, "DivinerBox"),
            VoyageFocusKind.Sulphur =>
                tags.HasFlag(ModifierTag.Sulphur) || Contains(name, "ResourceFound") || Contains(name, "Sulphur"),
            VoyageFocusKind.MessageBottles => Contains(name, "LostMessage"),
            VoyageFocusKind.FastVoyage => IsFastFiller(name),
            _ => false,
        };
    }

    private static bool IsClassified(Modifier mod) =>
        mod.Tags != ModifierTag.None || Contains(mod.Name, "Strongbox") ||
        Contains(mod.Name, "IncreasedRare") || Contains(mod.Name, "ResourceFound") ||
        Contains(mod.Name, "LostMessage") || Contains(mod.Name, "Starfish") || IsFastFiller(mod.Name);

    private static bool IsFastFiller(string name) =>
        Contains(name, "AdjacentBarrels") || Contains(name, "TormentCages") ||
        Contains(name, "Imprison") || Contains(name, "SoulEater") ||
        Contains(name, "MonstersPossessed") || Contains(name, "Pantheon");

    public static string Label(VoyageFocusKind kind) => kind switch
    {
        VoyageFocusKind.RareCurrency => "Moeda rara: Sea Pillars + 3 Strongboxes + 5 globais",
        VoyageFocusKind.OperativeFleet => "9 Operative Strongboxes",
        VoyageFocusKind.DivinerFleet => "9 Diviner Strongboxes",
        VoyageFocusKind.Sulphur => "9x Sulphur 25% + border de Sulphur",
        VoyageFocusKind.MessageBottles => "Messages in a Bottle: 8 suportes + 1 alvo",
        VoyageFocusKind.BrineRareDensity => "Brine King + pacote completo de raros",
        VoyageFocusKind.FastVoyage => "Fast Voyage S/$ (charts de descarte)",
        _ => kind.ToString(),
    };

    private static bool Contains(string value, string token) =>
        value?.Contains(token, StringComparison.OrdinalIgnoreCase) == true;
}

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
        var borders = EnumerateBorders(tileBorders).Select(x => x.Name ?? "").ToList();
        var setSize = Math.Clamp(options.DedicatedStrongboxSetSize, 1, 9);
        var operative = pieces.Count(VoyagePlacementRules.IsOperativeBoxChart);
        var diviner = pieces.Count(VoyagePlacementRules.IsDivinerBoxChart);
        var genericBoxes = pieces.Count(p => VoyagePlacementRules.IsStrongboxCountChart(p) &&
                                             !VoyagePlacementRules.IsOperativeBoxChart(p) &&
                                             !VoyagePlacementRules.IsDivinerBoxChart(p));
        var rareCurrencyBoxes = genericBoxes + Math.Max(0, operative - setSize) + Math.Max(0, diviner - setSize);
        var globalRare = pieces.Count(VoyagePlacementRules.IsRareVoyageChart);
        var seaPillars = pieces.Count(VoyagePlacementRules.IsSeaPillars);
        var messages = pieces.Count(VoyagePlacementRules.IsLostMessageChart);
        var highSulphur = pieces.Count(p =>
            VoyagePlacementRules.IsHighValueSulphurChart(p, options.MinimumSulphurPercent));
        var brines = pieces.Count(VoyagePlacementRules.IsBrineKingsDomain);
        var adjacentRare = pieces.Count(p => VoyagePlacementRules.IsAdjacentRareChart(p) ||
                                                  VoyagePlacementRules.IsStarfishChart(p));

        var rareBorder = borders.Select(RareCurrencyBorderScore).DefaultIfEmpty(0).Max();
        var sulphurBorder = borders.Any(n => Contains(n, "SulphurDrops"));
        var rareBoostBorder = borders.Any(n => Contains(n, "IncreasedRareMonsters") ||
                                                Contains(n, "RareMonstersPerConnection") ||
                                                Contains(n, "PackSize"));

        var candidates = new List<VoyageFocusChoice>();
        if (rareBorder > 0 && seaPillars > 0 &&
            rareCurrencyBoxes >= Math.Clamp(options.MinimumRareCurrencyStrongboxes, 1, 4) &&
            globalRare >= Math.Clamp(options.MinimumRareCurrencyGlobalRare, 1, 8))
        {
            candidates.Add(new VoyageFocusChoice(VoyageFocusKind.RareCurrency, 1_000 + rareBorder,
                $"Sea Pillars; {rareCurrencyBoxes} Strongboxes livres; {globalRare} globais de raros"));
        }

        if (operative >= setSize)
            candidates.Add(new VoyageFocusChoice(VoyageFocusKind.OperativeFleet, 820 + operative,
                $"{operative} charts de Operative Strongboxes"));
        if (diviner >= setSize)
            candidates.Add(new VoyageFocusChoice(VoyageFocusKind.DivinerFleet, 800 + diviner,
                $"{diviner} charts de Diviner Strongboxes"));
        if (sulphurBorder && highSulphur >= Math.Clamp(options.MinimumSulphurCharts, 1, 9))
            candidates.Add(new VoyageFocusChoice(VoyageFocusKind.Sulphur, 760 + highSulphur,
                $"{highSulphur} charts com pelo menos {options.MinimumSulphurPercent}%"));
        if (messages >= Math.Clamp(options.MinimumLostMessageCharts, 2, 8))
            candidates.Add(new VoyageFocusChoice(VoyageFocusKind.MessageBottles, 700 + messages,
                $"{messages} charts de Message in a Bottle"));
        if (brines > 0 && rareBoostBorder && globalRare >= 5 && adjacentRare >= 3)
            candidates.Add(new VoyageFocusChoice(VoyageFocusKind.BrineRareDensity, 650 + adjacentRare,
                $"Brine King; {adjacentRare} suportes adjacentes; {globalRare} globais"));

        if (candidates.Count == 0)
            candidates.Add(new VoyageFocusChoice(VoyageFocusKind.FastVoyage, 1,
                "nenhum pacote premium completo; preservar charts-chave"));

        var selected = candidates.OrderByDescending(x => x.Score).ThenBy(x => x.Kind).First();
        var focusBonus = selected.Kind == VoyageFocusKind.FastVoyage && !options.PreferFastVoyageFillers
            ? 0
            : Math.Clamp(options.FocusWeightBonus, 0, 3);
        return new VoyageFocusAnalysis(
            [selected],
            focusBonus,
            Math.Clamp(options.OffFocusMultiplier, 0.05, 1));
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

    private static double RareCurrencyBorderScore(string name)
    {
        if (!Contains(name, "RareMonster")) return 0;
        if (Contains(name, "Divine")) return 300;
        if (Contains(name, "Annulment")) return 180;
        if (Contains(name, "Ancient")) return 90;
        if (Contains(name, "Exalted")) return 70;
        return 0;
    }

    private static bool Contains(string value, string token) =>
        value?.Contains(token, StringComparison.OrdinalIgnoreCase) == true;
}
