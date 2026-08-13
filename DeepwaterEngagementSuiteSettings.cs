using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Windows.Forms;
using DeepwaterEngagementSuite.VoyagePlannerData;
using ExileCore;
using ExileCore.PoEMemory.Elements;
using ExileCore.PoEMemory.MemoryObjects;
using ExileCore.Shared.Attributes;
using ExileCore.Shared.Enums;
using ExileCore.Shared.Interfaces;
using ExileCore.Shared.Nodes;
using GameOffsets.Native;
using ImGuiNET;
using ItemFilterLibrary;
using Newtonsoft.Json;
using SharpDX;

namespace DeepwaterEngagementSuite;

public class DeepwaterEngagementSuiteSettings : ISettings
{
    public const MapIconsIndex DefaultOtherChestIcon = MapIconsIndex.HeistSpottedMiniBoss;
    public const MapIconsIndex DefaultSunkenLootIcon = MapIconsIndex.LootFilterSmallYellowCircle;
    public const MapIconsIndex DefaultBottledItemChestIcon = MapIconsIndex.QuestItem;
    public const MapIconsIndex DefaultGoldTreasureChestIcon = MapIconsIndex.LootFilterSmallYellowCircle;
    public const MapIconsIndex DefaultDeadmansSulphurSmallIcon = MapIconsIndex.LootFilterSmallGreenRaindrop;
    public const MapIconsIndex DefaultDeadmansSulphurBaseIcon = MapIconsIndex.LootFilterSmallGreenRaindrop;
    public const MapIconsIndex DefaultDeadmansSulphurLargeIcon = MapIconsIndex.LootFilterMediumGreenRaindrop;
    public const MapIconsIndex DefaultDeadmansSulphurHugeIcon = MapIconsIndex.LootFilterLargeGreenRaindrop;
    public const MapIconsIndex DefaultClamTreasureChestIcon = MapIconsIndex.LootFilterLargeYellowStar;
    public const MapIconsIndex DefaultCurrencyTreasureChestIcon = MapIconsIndex.RewardCurrency;
    public const MapIconsIndex DefaultCurrencyTreasureChestOpulentIcon = MapIconsIndex.LootFilterLargeYellowStar;
    public const MapIconsIndex DefaultCurrencyGemcuttersChestIcon = MapIconsIndex.RewardChestGems;
    public const MapIconsIndex DefaultUniqueWeaponChestIcon = MapIconsIndex.RewardWeapons;
    public const MapIconsIndex DefaultUniqueArmourChestIcon = MapIconsIndex.RewardArmour;
    public const MapIconsIndex DefaultUniqueJewelleryChestIcon = MapIconsIndex.RewardJewellery;
    public static readonly Color UniqueItemTint = new Color(175, 96, 37);
    public const MapIconsIndex DefaultScarabChestIcon = MapIconsIndex.RewardScarabs;
    public const MapIconsIndex DefaultStackedDecksChestIcon = MapIconsIndex.RewardDivinationCards;
    public const MapIconsIndex DefaultMapsChestIcon = MapIconsIndex.RewardMaps;
    public const MapIconsIndex DefaultAllflameEmbersChestIcon = MapIconsIndex.SanctumGoldConvert;
    public const MapIconsIndex DefaultCursedDucatDropIcon = MapIconsIndex.RewardPerandus;
    public const MapIconsIndex DefaultIzaroObjectIcon = MapIconsIndex.RewardLabyrinth;
    public const MapIconsIndex DefaultAltarCrabIcon = MapIconsIndex.RewardBestiary;
    public const MapIconsIndex DefaultAltarOctopusIcon = MapIconsIndex.RewardBreach;
    public const MapIconsIndex DefaultTormentedSpiritEncounterIcon = MapIconsIndex.LootFilterSmallGreenCircle;
    public const MapIconsIndex DefaultLanternReplenishEncounterIcon = MapIconsIndex.BlightPortalFire;

    public ToggleNode Enable { get; set; } = new ToggleNode(false);

    [Menu("Icon Settings")]
    public IconSettings IconSettings { get; set; } = new IconSettings();

    [Menu("Loot Window Settings")]
    public LootWindowSettings LootWindowSettings { get; set; } = new LootWindowSettings();

    public CurrencyReminderSettings CurrencyReminderSettings { get; set; } = new CurrencyReminderSettings();
    public BubbleSettings BubbleSettings { get; set; } = new BubbleSettings();
    public TrailSettings TrailSettings { get; set; } = new TrailSettings();

    [Menu("Bubble planner settings")]
    public PlannerSettings PlannerSettings { get; set; } = new PlannerSettings();
    public VoyageSettings VoyageSettings { get; set; } = new VoyageSettings();

    public static MapIconsIndex GetDefaultIcon(IconPickerIndex index) => index switch
    {
        IconPickerIndex.SunkenLoot => DefaultSunkenLootIcon,
        IconPickerIndex.BottledItemChest => DefaultBottledItemChestIcon,
        IconPickerIndex.GoldTreasureChest => DefaultGoldTreasureChestIcon,
        IconPickerIndex.GoldPile => DefaultGoldTreasureChestIcon,
        IconPickerIndex.ClamTreasureChest => DefaultClamTreasureChestIcon,
        IconPickerIndex.CurrencyTreasureChest => DefaultCurrencyTreasureChestIcon,
        IconPickerIndex.CurrencyTreasureChestOpulent => DefaultCurrencyTreasureChestOpulentIcon,
        IconPickerIndex.CurrencyGemcuttersChest => DefaultCurrencyGemcuttersChestIcon,
        IconPickerIndex.UniqueWeaponChest => DefaultUniqueWeaponChestIcon,
        IconPickerIndex.UniqueArmourChest => DefaultUniqueArmourChestIcon,
        IconPickerIndex.UniqueJewelleryChest => DefaultUniqueJewelleryChestIcon,
        IconPickerIndex.ScarabChest => DefaultScarabChestIcon,
        IconPickerIndex.StackedDecksChest => DefaultStackedDecksChestIcon,
        IconPickerIndex.MapsChest => DefaultMapsChestIcon,
        IconPickerIndex.AllflameEmbersChest => DefaultAllflameEmbersChestIcon,
        IconPickerIndex.CursedDucatDrop => DefaultCursedDucatDropIcon,
        IconPickerIndex.RandomDucatChest => DefaultCursedDucatDropIcon,
        IconPickerIndex.HazardBoatChest => DefaultCursedDucatDropIcon,
        IconPickerIndex.IzaroObject => DefaultIzaroObjectIcon,
        IconPickerIndex.AltarCrab => DefaultAltarCrabIcon,
        IconPickerIndex.AltarOctopus => DefaultAltarOctopusIcon,
        IconPickerIndex.TormentedSpiritEncounter => DefaultTormentedSpiritEncounterIcon,
        IconPickerIndex.LanternReplenishEncounter => DefaultLanternReplenishEncounterIcon,
        IconPickerIndex.GoldenLanternEncounter => MapIconsIndex.LabyrinthGoldKey,
        IconPickerIndex.InfusedCoralEncounter => MapIconsIndex.RewardBreach,
        IconPickerIndex.StrongboxDivination => MapIconsIndex.CorpseTypeUndead,
        IconPickerIndex.StrongboxScarab => MapIconsIndex.CorpseTypeEldritch,
        IconPickerIndex.StrongboxArcanist => MapIconsIndex.CorpseTypeBeast,
        IconPickerIndex.StrongboxOperative => MapIconsIndex.RewardScarabs,
        IconPickerIndex.StrongboxGeneric => DefaultOtherChestIcon,
        IconPickerIndex.PointerTarget => MapIconsIndex.AncestralEnemyTotem,
        IconPickerIndex.DeadMansSulphurSmall => DefaultDeadmansSulphurSmallIcon,
        IconPickerIndex.DeadMansSulphurBase => DefaultDeadmansSulphurBaseIcon,
        IconPickerIndex.DeadMansSulphurLarge => DefaultDeadmansSulphurLargeIcon,
        IconPickerIndex.DeadMansSulphurHuge => DefaultDeadmansSulphurHugeIcon,
        _ => DefaultOtherChestIcon,
    };

    public static Color? GetDefaultTint(IconPickerIndex index) => index switch
    {
        IconPickerIndex.UniqueWeaponChest or IconPickerIndex.UniqueArmourChest or IconPickerIndex.UniqueJewelleryChest => UniqueItemTint,
        IconPickerIndex.InfusedCoralEncounter => new Color(255, 90, 180),
        IconPickerIndex.DeadMansSulphurSmall or IconPickerIndex.DeadMansSulphurBase or
            IconPickerIndex.DeadMansSulphurLarge or IconPickerIndex.DeadMansSulphurHuge => new Color(80, 255, 80),
        IconPickerIndex.PointerTarget => Color.White,
        _ => null,
    };

    public static float GetDefaultIconSizeScale(IconPickerIndex index) => index switch
    {
        IconPickerIndex.CurrencyTreasureChestOpulent => 2.0f,
        IconPickerIndex.DeadMansSulphurSmall => 0.65f,
        IconPickerIndex.DeadMansSulphurLarge => 1.25f,
        IconPickerIndex.DeadMansSulphurHuge => 1.5f,
        _ => 1f,
    };

}

[Submenu(CollapsedByDefault = true)]
public class LootWindowSettings
{
    [Menu("Show loot window",
        "Summary window of discovered Deepwater targets. Off by default.")]
    public ToggleNode ShowLootWindow { get; set; } = new ToggleNode(false);
}

[Submenu(CollapsedByDefault = true)]
public class IconSettings
{
    public Dictionary<IconPickerIndex, IconDisplaySettings> IconMapping = new();

    public RangeNode<int> WorldIconSize { get; set; } = new RangeNode<int>(50, 25, 200);
    public RangeNode<int> MapIconSize { get; set; } = new RangeNode<int>(30, 15, 200);

    [Menu("Show Bottled Item icons")]
    public ToggleNode ShowBottledItemIcons { get; set; } = new ToggleNode(true);

    [Menu("Show Gold Treasure icons")]
    public ToggleNode ShowGoldTreasureIcons { get; set; } = new ToggleNode(true);

    [Menu("Show Clam Treasure icons")]
    public ToggleNode ShowClamTreasureIcons { get; set; } = new ToggleNode(true);

    [Menu("Show Currency chest icons")]
    public ToggleNode ShowCurrencyChestIcons { get; set; } = new ToggleNode(true);

    [Menu("Show Opulent Currency icons")]
    public ToggleNode ShowOpulentCurrencyIcons { get; set; } = new ToggleNode(true);

    [Menu("Show Gemcutter chest icons")]
    public ToggleNode ShowGemcutterChestIcons { get; set; } = new ToggleNode(true);

    [Menu("Show Unique Weapon icons")]
    public ToggleNode ShowUniqueWeaponIcons { get; set; } = new ToggleNode(true);

    [Menu("Show Unique Armour icons")]
    public ToggleNode ShowUniqueArmourIcons { get; set; } = new ToggleNode(true);

    [Menu("Show Unique Jewellery icons")]
    public ToggleNode ShowUniqueJewelleryIcons { get; set; } = new ToggleNode(true);

    [Menu("Show Scarab chest icons")]
    public ToggleNode ShowScarabChestIcons { get; set; } = new ToggleNode(true);

    [Menu("Show Stacked Deck icons")]
    public ToggleNode ShowStackedDeckIcons { get; set; } = new ToggleNode(true);

    [Menu("Show Maps chest icons",
        "Cartography / map chests. Off by default.")]
    public ToggleNode ShowMapsChestIcons { get; set; } = new ToggleNode(false);

    [Menu("Show Allflame Embers icons")]
    public ToggleNode ShowAllflameEmbersIcons { get; set; } = new ToggleNode(true);

    [Menu("Show Izaro icons")]
    public ToggleNode ShowIzaroIcons { get; set; } = new ToggleNode(true);

    [Menu("Show Altar (Crab) icons")]
    public ToggleNode ShowAltarCrabIcons { get; set; } = new ToggleNode(true);

    [Menu("Show Altar (Octopus) icons")]
    public ToggleNode ShowAltarOctopusIcons { get; set; } = new ToggleNode(true);

    [Menu("Show Lantern Replenish icons")]
    public ToggleNode ShowLanternReplenishIcons { get; set; } = new ToggleNode(true);

    [Menu("Show Infused Coral icons")]
    public ToggleNode ShowInfusedCoralIcons { get; set; } = new ToggleNode(true);

    [Menu("Show other chest icons")]
    public ToggleNode ShowOtherChestIcons { get; set; } = new ToggleNode(true);

    [Menu("Show Sunken Loot icons")]
    public ToggleNode ShowSunkenLootIcons { get; set; } = new ToggleNode(true);

    [Menu("Show Gold Pile icons")]
    public ToggleNode ShowGoldPileIcons { get; set; } = new ToggleNode(true);

    [Menu("Show generic/Operative strongbox icons")]
    public ToggleNode ShowAllStrongboxIcons { get; set; } = new ToggleNode(true);

    [Menu("Show Dead Man's Sulphur crystal icons")]
    public ToggleNode ShowDeadmansSulphurIcons { get; set; } = new ToggleNode(true);

    [Menu("Agrupar cristais de Sulphur",
        "Substitui dezenas de marcadores por um único ícone por aglomerado. Não altera o peso individual usado pelo Bubble Planner.")]
    public ToggleNode CompactSulphurClusters { get; set; } = new ToggleNode(true);

    [Menu("Distância para agrupar cristais",
        "Cristais dentro desta distância de grid pertencem ao mesmo aglomerado visual.")]
    public RangeNode<int> SulphurClusterRadius { get; set; } = new RangeNode<int>(24, 6, 80);

    [Menu("Transparência do ícone de Sulphur (%)")]
    public RangeNode<int> SulphurClusterOpacityPercent { get; set; } = new RangeNode<int>(55, 10, 100);

    [Menu("Tamanho máximo do aglomerado (%)",
        "O tamanho cresce com a quantidade e o porte dos cristais, até este limite.")]
    public RangeNode<int> SulphurClusterMaxSizePercent { get; set; } = new RangeNode<int>(150, 75, 250);

    [Menu("Esconder Sulphur já coberto por bolha",
        "Remove imediatamente do marcador os cristais que já estão dentro de uma lanterna colocada.")]
    public ToggleNode HideCoveredSulphurClusters { get; set; } = new ToggleNode(true);

    [Menu("Ignorar cristais no Trail",
        "Impede linhas e nomes do Trail apontando para cristais de Sulphur.")]
    public ToggleNode ExcludeSulphurFromTrail { get; set; } = new ToggleNode(true);

    [Menu("Show pointer stand-in icons",
        "Undiscovered pointer targets on the large map.")]
    public ToggleNode ShowPointerTargetIcons { get; set; } = new ToggleNode(true);

    [Menu("Show Ducat icons",
        "Cursed ducat drops, random ducat chests, and hazard boats. Off by default.")]
    public ToggleNode ShowDucatIcons { get; set; } = new ToggleNode(false);

    [Menu("Show Golden Lantern icons",
        "Off by default to reduce clutter.")]
    public ToggleNode ShowGoldenLanternIcons { get; set; } = new ToggleNode(false);

    [Menu("Show Tormented Spirit icons",
        "Off by default to reduce clutter.")]
    public ToggleNode ShowTormentedSpiritIcons { get; set; } = new ToggleNode(false);

    [Menu("Show Arcanist strongbox icons",
        "Off by default to reduce clutter.")]
    public ToggleNode ShowArcanistStrongboxIcons { get; set; } = new ToggleNode(false);

    [Menu("Show Diviner strongbox icons",
        "Off by default to reduce clutter.")]
    public ToggleNode ShowDivinerStrongboxIcons { get; set; } = new ToggleNode(false);

    [Menu("Show Scarab strongbox icons",
        "Off by default to reduce clutter.")]
    public ToggleNode ShowScarabStrongboxIcons { get; set; } = new ToggleNode(false);

    public bool IsIconEnabled(IconPickerIndex index) => index switch
    {
        IconPickerIndex.SunkenLoot => ShowSunkenLootIcons.Value,
        IconPickerIndex.BottledItemChest => ShowBottledItemIcons.Value,
        IconPickerIndex.GoldTreasureChest => ShowGoldTreasureIcons.Value,
        IconPickerIndex.GoldPile => ShowGoldPileIcons.Value,
        IconPickerIndex.ClamTreasureChest => ShowClamTreasureIcons.Value,
        IconPickerIndex.CurrencyTreasureChest => ShowCurrencyChestIcons.Value,
        IconPickerIndex.CurrencyTreasureChestOpulent => ShowOpulentCurrencyIcons.Value,
        IconPickerIndex.CurrencyGemcuttersChest => ShowGemcutterChestIcons.Value,
        IconPickerIndex.UniqueWeaponChest => ShowUniqueWeaponIcons.Value,
        IconPickerIndex.UniqueArmourChest => ShowUniqueArmourIcons.Value,
        IconPickerIndex.UniqueJewelleryChest => ShowUniqueJewelleryIcons.Value,
        IconPickerIndex.ScarabChest => ShowScarabChestIcons.Value,
        IconPickerIndex.StackedDecksChest => ShowStackedDeckIcons.Value,
        IconPickerIndex.MapsChest => ShowMapsChestIcons.Value,
        IconPickerIndex.AllflameEmbersChest => ShowAllflameEmbersIcons.Value,
        IconPickerIndex.CursedDucatDrop or
            IconPickerIndex.RandomDucatChest or
            IconPickerIndex.HazardBoatChest => ShowDucatIcons.Value,
        IconPickerIndex.IzaroObject => ShowIzaroIcons.Value,
        IconPickerIndex.AltarCrab => ShowAltarCrabIcons.Value,
        IconPickerIndex.AltarOctopus => ShowAltarOctopusIcons.Value,
        IconPickerIndex.TormentedSpiritEncounter => ShowTormentedSpiritIcons.Value,
        IconPickerIndex.LanternReplenishEncounter => ShowLanternReplenishIcons.Value,
        IconPickerIndex.GoldenLanternEncounter => ShowGoldenLanternIcons.Value,
        IconPickerIndex.InfusedCoralEncounter => ShowInfusedCoralIcons.Value,
        IconPickerIndex.StrongboxDivination => ShowDivinerStrongboxIcons.Value,
        IconPickerIndex.StrongboxScarab => ShowScarabStrongboxIcons.Value,
        IconPickerIndex.StrongboxArcanist => ShowArcanistStrongboxIcons.Value,
        IconPickerIndex.StrongboxOperative or IconPickerIndex.StrongboxGeneric => ShowAllStrongboxIcons.Value,
        IconPickerIndex.PointerTarget => ShowPointerTargetIcons.Value,
        IconPickerIndex.DeadMansSulphurSmall or IconPickerIndex.DeadMansSulphurBase or
            IconPickerIndex.DeadMansSulphurLarge or IconPickerIndex.DeadMansSulphurHuge => ShowDeadmansSulphurIcons.Value,
        IconPickerIndex.OtherChests => ShowOtherChestIcons.Value,
        _ => true,
    };
}

[Submenu(CollapsedByDefault = true)]
public class TrailSettings
{
    public ToggleNode Enabled { get; set; } = new ToggleNode(false);
    public ToggleNode DrawOnLargeMap { get; set; } = new ToggleNode(true);
    public ToggleNode DrawInWorld { get; set; } = new ToggleNode(false);
    public ToggleNode OnlyUnreachable { get; set; } = new ToggleNode(false);
    public ToggleNode ShowLabels { get; set; } = new ToggleNode(true);
    public RangeNode<int> MaxDistance { get; set; } = new RangeNode<int>(500, 10, 1000);
    public RangeNode<int> MapLineWidth { get; set; } = new RangeNode<int>(3, 1, 20);
    public RangeNode<int> WorldLineWidth { get; set; } = new RangeNode<int>(5, 1, 20);
    public ColorNode DefaultMapColor { get; set; } = new Color(255, 140, 0, 200);
    public ColorNode DefaultWorldColor { get; set; } = new Color(255, 140, 0, 200);
    public ToggleNode ShowUndiscoveredTargets { get; set; } = new ToggleNode(true);
    public ColorNode UndiscoveredColor { get; set; } = new Color(255, 255, 255, 220);
    public TrailColorSettings Colors { get; set; } = new TrailColorSettings();
}

[Submenu(CollapsedByDefault = true)]
public class TrailColorSettings
{
    public ToggleNode ShowBottledItem { get; set; } = new ToggleNode(true);
    public ColorNode BottledItem { get; set; } = new Color(255, 215, 0, 255);
    public ToggleNode ShowGoldTreasure { get; set; } = new ToggleNode(true);
    public ColorNode GoldTreasure { get; set; } = new Color(255, 215, 0, 255);
    public ToggleNode ShowClamTreasure { get; set; } = new ToggleNode(true);
    public ColorNode ClamTreasure { get; set; } = new Color(255, 255, 100, 255);
    public ToggleNode ShowCurrency { get; set; } = new ToggleNode(true);
    public ColorNode Currency { get; set; } = new Color(255, 255, 255, 255);
    public ToggleNode ShowOpulentCurrency { get; set; } = new ToggleNode(true);
    public ColorNode OpulentCurrency { get; set; } = new Color(255, 170, 0, 255);
    public ToggleNode ShowUniqueWeapon { get; set; } = new ToggleNode(true);
    public ColorNode UniqueWeapon { get; set; } = new Color(175, 96, 37, 255);
    public ToggleNode ShowUniqueArmour { get; set; } = new ToggleNode(true);
    public ColorNode UniqueArmour { get; set; } = new Color(175, 96, 37, 255);
    public ToggleNode ShowUniqueJewellery { get; set; } = new ToggleNode(true);
    public ColorNode UniqueJewellery { get; set; } = new Color(175, 96, 37, 255);
    public ToggleNode ShowScarabs { get; set; } = new ToggleNode(true);
    public ColorNode Scarabs { get; set; } = new Color(200, 150, 255, 255);
    public ToggleNode ShowStackedDecks { get; set; } = new ToggleNode(true);
    public ColorNode StackedDecks { get; set; } = new Color(100, 200, 255, 255);
    public ToggleNode ShowMaps { get; set; } = new ToggleNode(true);
    public ColorNode Maps { get; set; } = new Color(200, 200, 200, 255);
    public ToggleNode ShowAllflameEmbers { get; set; } = new ToggleNode(true);
    public ColorNode AllflameEmbers { get; set; } = new Color(255, 100, 50, 255);
    public ToggleNode ShowCursedDucat { get; set; } = new ToggleNode(true);
    public ColorNode CursedDucat { get; set; } = new Color(255, 200, 50, 255);
    public ToggleNode ShowRandomDucat { get; set; } = new ToggleNode(true);
    public ColorNode RandomDucat { get; set; } = new Color(255, 200, 50, 255);
    public ToggleNode ShowHazardBoat { get; set; } = new ToggleNode(true);
    public ColorNode HazardBoat { get; set; } = new Color(255, 200, 50, 255);
    public ToggleNode ShowIzaro { get; set; } = new ToggleNode(true);
    public ColorNode Izaro { get; set; } = new Color(255, 255, 0, 255);
    public ToggleNode ShowAltarCrab { get; set; } = new ToggleNode(true);
    public ColorNode AltarCrab { get; set; } = new Color(50, 200, 50, 255);
    public ToggleNode ShowAltarOctopus { get; set; } = new ToggleNode(true);
    public ColorNode AltarOctopus { get; set; } = new Color(150, 50, 255, 255);
    public ToggleNode ShowTormentedSpirit { get; set; } = new ToggleNode(true);
    public ColorNode TormentedSpirit { get; set; } = new Color(0, 255, 100, 255);
    public ToggleNode ShowLanternReplenish { get; set; } = new ToggleNode(true);
    public ColorNode LanternReplenish { get; set; } = new Color(100, 200, 255, 255);
    public ToggleNode ShowGoldenLantern { get; set; } = new ToggleNode(true);
    public ColorNode GoldenLantern { get; set; } = new Color(255, 215, 0, 255);
    public ToggleNode ShowInfusedCoral { get; set; } = new ToggleNode(true);
    public ColorNode InfusedCoral { get; set; } = new Color(255, 90, 180, 255);
    public ToggleNode ShowOther { get; set; } = new ToggleNode(true);
    public ColorNode Other { get; set; } = new Color(180, 180, 180, 255);

    public bool IsEnabled(IconPickerIndex type) => type switch
    {
        IconPickerIndex.BottledItemChest => ShowBottledItem.Value,
        IconPickerIndex.GoldTreasureChest => ShowGoldTreasure.Value,
        IconPickerIndex.ClamTreasureChest => ShowClamTreasure.Value,
        IconPickerIndex.CurrencyTreasureChest => ShowCurrency.Value,
        IconPickerIndex.CurrencyTreasureChestOpulent => ShowOpulentCurrency.Value,
        IconPickerIndex.UniqueWeaponChest => ShowUniqueWeapon.Value,
        IconPickerIndex.UniqueArmourChest => ShowUniqueArmour.Value,
        IconPickerIndex.UniqueJewelleryChest => ShowUniqueJewellery.Value,
        IconPickerIndex.ScarabChest => ShowScarabs.Value,
        IconPickerIndex.StackedDecksChest => ShowStackedDecks.Value,
        IconPickerIndex.MapsChest => ShowMaps.Value,
        IconPickerIndex.AllflameEmbersChest => ShowAllflameEmbers.Value,
        IconPickerIndex.CursedDucatDrop => ShowCursedDucat.Value,
        IconPickerIndex.RandomDucatChest => ShowRandomDucat.Value,
        IconPickerIndex.HazardBoatChest => ShowHazardBoat.Value,
        IconPickerIndex.IzaroObject => ShowIzaro.Value,
        IconPickerIndex.AltarCrab => ShowAltarCrab.Value,
        IconPickerIndex.AltarOctopus => ShowAltarOctopus.Value,
        IconPickerIndex.TormentedSpiritEncounter => ShowTormentedSpirit.Value,
        IconPickerIndex.LanternReplenishEncounter => ShowLanternReplenish.Value,
        IconPickerIndex.GoldenLanternEncounter => ShowGoldenLantern.Value,
        IconPickerIndex.InfusedCoralEncounter => ShowInfusedCoral.Value,
        IconPickerIndex.OtherChests => ShowOther.Value,
        _ => true,
    };

    public Color Get(IconPickerIndex type, Color fallback) => type switch
    {
        IconPickerIndex.BottledItemChest => BottledItem.Value,
        IconPickerIndex.GoldTreasureChest => GoldTreasure.Value,
        IconPickerIndex.ClamTreasureChest => ClamTreasure.Value,
        IconPickerIndex.CurrencyTreasureChest => Currency.Value,
        IconPickerIndex.CurrencyTreasureChestOpulent => OpulentCurrency.Value,
        IconPickerIndex.UniqueWeaponChest => UniqueWeapon.Value,
        IconPickerIndex.UniqueArmourChest => UniqueArmour.Value,
        IconPickerIndex.UniqueJewelleryChest => UniqueJewellery.Value,
        IconPickerIndex.ScarabChest => Scarabs.Value,
        IconPickerIndex.StackedDecksChest => StackedDecks.Value,
        IconPickerIndex.MapsChest => Maps.Value,
        IconPickerIndex.AllflameEmbersChest => AllflameEmbers.Value,
        IconPickerIndex.CursedDucatDrop => CursedDucat.Value,
        IconPickerIndex.RandomDucatChest => RandomDucat.Value,
        IconPickerIndex.HazardBoatChest => HazardBoat.Value,
        IconPickerIndex.IzaroObject => Izaro.Value,
        IconPickerIndex.AltarCrab => AltarCrab.Value,
        IconPickerIndex.AltarOctopus => AltarOctopus.Value,
        IconPickerIndex.TormentedSpiritEncounter => TormentedSpirit.Value,
        IconPickerIndex.LanternReplenishEncounter => LanternReplenish.Value,
        IconPickerIndex.GoldenLanternEncounter => GoldenLantern.Value,
        IconPickerIndex.InfusedCoralEncounter => InfusedCoral.Value,
        IconPickerIndex.OtherChests => Other.Value,
        _ => fallback,
    };
}

[Submenu(CollapsedByDefault = true)]
public class CurrencyReminderSettings
{
    public ToggleNode Enabled { get; set; } = new ToggleNode(false);
    public RangeNode<int> RequiredExaltedOrbs { get; set; } = new RangeNode<int>(20, 0, 20);
    public RangeNode<int> RequiredAlchemyOrbs { get; set; } = new RangeNode<int>(20, 0, 20);
    public RangeNode<int> RequiredChaosOrbs { get; set; } = new RangeNode<int>(20, 0, 20);
    public RangeNode<int> RequiredScouringOrbs { get; set; } = new RangeNode<int>(20, 0, 20);
    public RangeNode<int> MaxInventoryItems { get; set; } = new RangeNode<int>(30, 0, 60);
}

[Submenu(CollapsedByDefault = true)]
public class PlannerSettings
{
    public int PlannerSchemaVersion = 0;

    public Dictionary<IconPickerIndex, ChestSettings> ChestSettingsMap = new()
    {
        [IconPickerIndex.BottledItemChest] = new ChestSettings { Weight = 40 },
        [IconPickerIndex.ClamTreasureChest] = new ChestSettings { Weight = 2 },
        [IconPickerIndex.LanternReplenishEncounter] = new ChestSettings { Weight = 30 },
        [IconPickerIndex.CurrencyTreasureChestOpulent] = new ChestSettings { Weight = 50 },
        [IconPickerIndex.DeadMansSulphurSmall] = new ChestSettings { Weight = 30 },
        [IconPickerIndex.DeadMansSulphurBase] = new ChestSettings { Weight = 30 },
        [IconPickerIndex.DeadMansSulphurLarge] = new ChestSettings { Weight = 30 },
        [IconPickerIndex.DeadMansSulphurHuge] = new ChestSettings { Weight = 30 },
        [IconPickerIndex.SunkenLoot] = new ChestSettings { Weight = 8 },
        [IconPickerIndex.GoldPile] = new ChestSettings { Weight = 6 },
        [IconPickerIndex.GoldTreasureChest] = new ChestSettings { Weight = 6 },
        [IconPickerIndex.StrongboxGeneric] = new ChestSettings { Weight = 12 },
        [IconPickerIndex.StrongboxOperative] = new ChestSettings { Weight = 20 },
        [IconPickerIndex.StrongboxArcanist] = new ChestSettings { Weight = 20 },
        [IconPickerIndex.StrongboxDivination] = new ChestSettings { Weight = 20 },
        [IconPickerIndex.StrongboxScarab] = new ChestSettings { Weight = 20 },
        [IconPickerIndex.PointerTarget] = new ChestSettings { Weight = 8 },
    };

    public HotkeyNodeV2 StartSearchHotkey { get; set; } = new HotkeyNodeV2(Keys.None);
    public HotkeyNodeV2 StopSearchHotkey { get; set; } = new HotkeyNodeV2(Keys.None);
    public HotkeyNodeV2 ClearSearchHotkey { get; set; } = new HotkeyNodeV2(Keys.None);
    public HotkeyNodeV2 ConfirmEditorPlacementHotkey { get; set; } = new HotkeyNodeV2(Keys.None);

    [JsonIgnore]
    [ConditionalDisplay(nameof(IsSearchRunning), false)]
    public ButtonNode StartSearch { get; set; } = new ButtonNode();

    [JsonIgnore]
    [ConditionalDisplay(nameof(IsSearchRunning))]
    public ButtonNode StopSearch { get; set; } = new ButtonNode();

    [JsonIgnore]
    [ConditionalDisplay(nameof(HasSearchResult))]
    public ButtonNode ClearSearch { get; set; } = new ButtonNode();
    public ToggleNode PlaySoundOnFinish { get; set; } = new ToggleNode(false);
    public ToggleNode DrawPlannedBubblesOnMap { get; set; } = new ToggleNode(true);
    public ToggleNode DrawLinesToLanternsInWorld { get; set; } = new ToggleNode(true);
    public RangeNode<int> ClosestNLanterns { get; set; } = new RangeNode<int>(2, 0, 10);
    public ToggleNode MergePlannedBubbles { get; set; } = new ToggleNode(true);

    [Menu("Color for suggested bubble radius")]
    public ColorNode BubbleColor { get; set; } = new ColorNode(Color.Purple);

    public ColorNode MapLineColor { get; set; } = new ColorNode(Color.Red);
    public ColorNode WorldLineColor { get; set; } = new ColorNode(Color.Orange);

    [Menu("Color for captured entities in world")]
    public ColorNode CapturedEntityWorldFrameColor { get; set; } = new ColorNode(Color.Purple);

    [Menu("Color for captured entities on map")]
    public ColorNode CapturedEntityMapFrameColor { get; set; } = new ColorNode(Color.Purple);

    [Menu(null, "Do not show lines/circles for plan segments where a real bubble has already been placed")]
    public ToggleNode RemoveGraphicsForPlacedBubbles { get; set; } = new ToggleNode(false);

    public RangeNode<float> TextMarkerScale { get; set; } = new RangeNode<float>(2, 0, 5);

    [Menu("Lembrar recompensas descobertas",
        "Mantém no planner entidades que saíram da área carregada. O cache é limpo ao trocar de área.")]
    public ToggleNode RememberDiscoveredEntities { get; set; } = new ToggleNode(true);

    [Menu("Ler entidades adormecidas",
        "Amplia a detecção além da rede atual. Também requer Core -> Debug -> CollectSleepingEntities no ExileApi.")]
    public ToggleNode IncludeSleepingEntities { get; set; } = new ToggleNode(true);

    [Menu("Proteção de entidades em Voyages",
        "Desativa temporariamente Core -> Debug -> Collect Sleeping Entities durante Voyages. Evita ultrapassar o limite global de 10.000 entidades e restaura a opção ao entrar em um chart normal ou descarregar o plugin.")]
    public ToggleNode DisableSleepingEntityCollectionInVoyages { get; set; } = new ToggleNode(true);

    [Menu("Usar alvos ainda não revelados",
        "Usa os Pointer targets com peso baixo para atravessar trechos sem recompensas visíveis.")]
    public ToggleNode IncludeUndiscoveredPointerTargets { get; set; } = new ToggleNode(true);

    [Menu("Ajuste automático ao vivo",
        "Nos charts normais, mantém a sugestão fixa e só recalcula quando uma nova recompensa ainda fora das bolhas é carregada.")]
    public ToggleNode LiveReplanEnabled { get; set; } = new ToggleNode(true);

    [Menu("Aguardar novos objetos antes de recalcular (ms)",
        "Agrupa entidades que carregam juntas para evitar vários cálculos consecutivos.")]
    public RangeNode<int> LiveReplanDebounceMilliseconds { get; set; } = new RangeNode<int>(220, 50, 2000);

    [Menu("Intervalo mínimo entre recálculos (ms)")]
    public RangeNode<int> LiveReplanMinimumIntervalMilliseconds { get; set; } = new RangeNode<int>(450, 100, 5000);

    [Menu("Desativar Bubble Planner a partir de X lanternas",
        "Áreas com esta quantidade são tratadas como Voyage: o cálculo e todos os desenhos de sugestão são desligados para reduzir poluição e uso de CPU.")]
    public RangeNode<int> VoyageLanternThreshold { get; set; } = new RangeNode<int>(20, 10, 100);

    // Kept only as internal compatibility inputs for PathPlanner. Voyage planning is disabled.
    [JsonIgnore, IgnoreMenu]
    public RangeNode<int> VoyageExplorationSteps { get; set; } = new RangeNode<int>(0, 0, 0);

    [JsonIgnore, IgnoreMenu]
    public RangeNode<float> VoyageLanternCostPenalty { get; set; } = new RangeNode<float>(0, 0, 0);

    [JsonIgnore, IgnoreMenu]
    public ToggleNode VoyageTrimAfterLastLoot { get; set; } = new ToggleNode(false);

    [Menu("Cobertura caminhável mínima (%)",
        "Evita lanternas com grande parte da bubble fora do terreno navegável. Se não houver alternativa, o planner reduz o limite automaticamente.")]
    public RangeNode<int> MinimumWalkableCoveragePercent { get; set; } = new RangeNode<int>(70, 20, 100);

    [Menu("Penalidade por terreno desperdiçado",
        "Quanto maior, mais o planner evita bordas, paredes e água não navegável.")]
    public RangeNode<float> TerrainWastePenalty { get; set; } = new RangeNode<float>(60, 0, 200);

    [Menu("Parar quando a solução estabilizar")]
    public ToggleNode StopWhenStable { get; set; } = new ToggleNode(true);

    [Menu("Tempo estável antes de parar (ms)")]
    public RangeNode<int> StableSearchMilliseconds { get; set; } = new RangeNode<int>(550, 100, 5000);

    public RangeNode<float> MaximumGenerationTimeSeconds { get; set; } = new RangeNode<float>(3, 0, 60);
    public RangeNode<int> SearchThreads { get; set; } = new RangeNode<int>(4, 1, 10);
    public RangeNode<float> NewRandomPathInjectionRate { get; set; } = new RangeNode<float>(1f, 0, 2);
    public RangeNode<int> PathGenerationSize { get; set; } = new RangeNode<int>(48, 1, 1000);
    public RangeNode<int> ValidatedIntermediatePoints { get; set; } = new RangeNode<int>(1, 0, 5);

    public ToggleNode ShowScoreHistory { get; set; } = new ToggleNode(false);
    public ToggleNode ShowScoreHistoryAfterSearchEnds { get; set; } = new ToggleNode(false);

    internal bool HasSearchResult => SearchState != SearchState.Empty;
    internal bool IsSearchRunning => SearchState == SearchState.Searching;

    internal SearchState SearchState = SearchState.Empty;
}

[Submenu(CollapsedByDefault = true)]
public class BubbleSettings
{
    public ToggleNode ShowBubblesOnMap { get; set; } = new ToggleNode(true);
    public ToggleNode ShowBubblesInWorld { get; set; } = new ToggleNode(false);

    [Menu("Color for bubble radius")]
    public ColorNode BubbleColor { get; set; } = new ColorNode(Color.Red);

    public RangeNode<int> BubbleRadiusOverride { get; set; } = new RangeNode<int>(0, 0, 1000);

    [Menu("Merge bubble circles for planned bubbles")]
    public ToggleNode EnableBubbleRadiusMerging { get; set; } = new ToggleNode(true);

    [Menu("Hide icons of entities captured by bubbles in world")]
    public ToggleNode HideCapturedEntitiesInWorld { get; set; } = new ToggleNode(false);

    [Menu("Hide icons of entities captured by bubbles on map")]
    public ToggleNode HideCapturedEntitiesOnMap { get; set; } = new ToggleNode(false);

    [Menu("Rectangle Thickness for captured entities in world")]
    public RangeNode<int> CapturedEntityWorldFrameThickness { get; set; } = new RangeNode<int>(2, 1, 20);

    [Menu("Rectangle Thickness for captured entities on map")]
    public RangeNode<int> CapturedEntityMapFrameThickness { get; set; } = new RangeNode<int>(2, 1, 20);

    public ToggleNode MarkStartingBubble { get; set; } = new ToggleNode(true);
}

[Submenu(CollapsedByDefault = true)]
public class VoyageSettings
{
    public VoyageSettings()
    {
        ClearBorderModifiers = new ButtonNode() { OnPressed = () => { BorderModifiers.Content.Clear(); } };
        ClearChartModifiers = new ButtonNode() { OnPressed = () => { ChartModifiers.Content.Clear(); } };
    }

    [JsonIgnore] [IgnoreMenu] public List<VoyageProfileEntry> Profiles { get; set; } = new();

    public ToggleNode EnableVoyageHandling { get; set; } = new ToggleNode(true);

    [Menu(null, CollapsedByDefault = true)]
    public ContentNode<VoyageExcludedChartSettings> IgnoredCharts { get; set; } = new ContentNode<VoyageExcludedChartSettings>
    {
        EnableControls = true,
        EnableItemCollapsing = true,
        ItemFactory = () => new VoyageExcludedChartSettings(),
        ItemFilter = (o, s) => o.IFL.Value.Contains(s, StringComparison.OrdinalIgnoreCase),
    };

    [Menu("Show optimizer window")]
    public ToggleNode ShowOptimizerWindow { get; set; } = new ToggleNode(true);

    [Menu("Show score debug details",
        "Verbose optimizer tables: per-tile (row,col) score breakdown and contribution sources, " +
        "plus (row, col) labels on each Plan Your Voyage board tile. " +
        "Off by default — strategy labels stay available without this noise.")]
    public ToggleNode ShowScoreDebugDetails { get; set; } = new ToggleNode(false);

    [Menu("Solver time limit (seconds)", "Max time the solver runs before returning the best solution found so far. 0 = no limit.")]
    public RangeNode<int> SolverTimeLimitSeconds { get; set; } = new RangeNode<int>(5, 1, 120);

    [Menu("Use fast solver (optimized)", "Fast topology/assignment search. Every candidate is re-ranked with the complete per-connection and layout scorer; ignores the time limit.")]
    public ToggleNode UseFastSolver { get; set; } = new ToggleNode(true);

    [Menu("Atraso entre placements (ms)",
        "Espera adicional após limpar, pegar, colocar e rotacionar cada chart. Ajuda a interface de duas abas a registrar todos os cliques.")]
    public RangeNode<int> ChartPlacementDelayMs { get; set; } = new RangeNode<int>(35, 0, 500);

    public ToggleNode ShowAllBorderModifiers { get; set; } = new ToggleNode(false);
    public ToggleNode ShowAllChartModifiers { get; set; } = new ToggleNode(false);
    public ToggleNode ShowChartInventoryInformation { get; set; } = new ToggleNode(false);

    public ListNode ProfileSelector { get; set; } = new ListNode();
    [JsonIgnore] public ButtonNode AddProfile { get; set; } = new ButtonNode();
    [JsonIgnore] public ButtonNode ReloadProfiles { get; set; } = new ButtonNode();
    [JsonIgnore][Menu("Delete current profile (hold shift)")] public ButtonNode DeleteCurrentProfile { get; set; } = new ButtonNode();
    [JsonIgnore] public CustomNode ProfileRenameNode { get; set; } = new CustomNode();

    [JsonIgnore]
    public ButtonNode ClearBorderModifiers { get; set; }

    [Menu(null, CollapsedByDefault = true)]
    [JsonIgnore]
    public ContentNode<VoyageBorderModifier> BorderModifiers { get; set; } = new ContentNode<VoyageBorderModifier>
    {
        EnableControls = true,
        EnableItemCollapsing = true,
        ItemFactory = () => new VoyageBorderModifier(),
        ItemFilter = (o, s) => o.Id.Value.Contains(s, StringComparison.OrdinalIgnoreCase) ||
                               o.Abbreviation.Value.Contains(s, StringComparison.OrdinalIgnoreCase),
    };

    [JsonIgnore]
    public ButtonNode ClearChartModifiers { get; set; }

    [Menu(null, CollapsedByDefault = true)]
    [JsonIgnore]
    public ContentNode<VoyageChartModifier> ChartModifiers { get; set; } = new ContentNode<VoyageChartModifier>
    {
        EnableControls = true,
        EnableItemCollapsing = true,
        ItemFactory = () => new VoyageChartModifier(),
        ItemFilter = (o, s) => o.Id.Value.Contains(s, StringComparison.OrdinalIgnoreCase),
    };

    [Menu("Border loot and reroll economy",
        "Rates the current borders, converts reroll Sulphur to chaos, and chooses the preferred layout family.")]
    public VoyageEconomySettings Economy { get; set; } = new VoyageEconomySettings();

    [Menu("Formatos permitidos da Voyage",
        "Marque um ou mais formatos. O solver rejeita formatos desmarcados, exceto quando a exceção premium estiver ativa.")]
    public VoyageLayoutSettings Layouts { get; set; } = new VoyageLayoutSettings();

    [Menu("Estratégias de posicionamento",
        "Cada opção explica quando reserva charts e quando os libera. Estratégias desativadas não travam células nem guardam charts.")]
    public VoyageStrategySettingsV2 Strategies { get; set; } = new VoyageStrategySettingsV2();
}

[Submenu(CollapsedByDefault = true)]
public class VoyageLayoutSettings
{
    [Menu("Permitir: S / $",
        "Caminho contínuo e rápido, recomendado para Voyages sem recompensa direta forte.")]
    public ToggleNode AllowSnakeDollar { get; set; } = new ToggleNode(true);

    [Menu("Permitir: compacto (+ fechado)",
        "Formato fechado: o centro funciona como hub e os cantos permanecem ligados ao miolo.")]
    public ToggleNode AllowCompact { get; set; } = new ToggleNode(true);

    [Menu("Permitir: linhas retas",
        "Formato aberto/candelabro. Útil quando uma estratégia premium precisa expor vários suportes.")]
    public ToggleNode AllowStraightLines { get; set; } = new ToggleNode(false);

    [Menu("Permitir formato exigido pela estratégia premium",
        "Troca temporariamente para somente o formato exigido pelo plano completo: linhas retas para moeda rara; compacto para Message/Brine. Nunca libera formatos arbitrários.")]
    public ToggleNode IgnoreRestrictionsForPremium { get; set; } = new ToggleNode(true);

    [IgnoreMenu]
    public RangeNode<int> PremiumScoreThreshold { get; set; } = new RangeNode<int>(80, 50, 100);

    [Menu("Semelhança mínima com o formato (%)",
        "Evita aceitar uma topologia apenas vagamente parecida com a família marcada.")]
    public RangeNode<int> MinimumSimilarityPercent { get; set; } = new RangeNode<int>(62, 45, 100);

    public VoyageLayoutFamilies SelectedFamilies()
    {
        var result = VoyageLayoutFamilies.None;
        if (AllowSnakeDollar.Value) result |= VoyageLayoutFamilies.SnakeDollar;
        if (AllowCompact.Value) result |= VoyageLayoutFamilies.Compact;
        if (AllowStraightLines.Value) result |= VoyageLayoutFamilies.StraightLines;
        return result == VoyageLayoutFamilies.None ? VoyageLayoutFamilies.All : result;
    }
}

[Submenu(CollapsedByDefault = true)]
public class VoyageEconomySettings
{
    [Menu("Enable border loot analysis")]
    public ToggleNode Enabled { get; set; } = new ToggleNode(true);

    [Menu("Automatic ARPG tier colors",
        "Orange = Divine/premium, blue = rare/good, green = moderate, white = weak or no direct loot.")]
    public ToggleNode AutomaticTierColors { get; set; } = new ToggleNode(true);

    [Menu("Show loot score beside border")]
    public ToggleNode ShowBorderScore { get; set; } = new ToggleNode(true);

    [Menu("Sulphur per Chaos", "Exchange rate used only by the KEEP/REROLL recommendation.")]
    public RangeNode<int> SulphurPerChaos { get; set; } = new RangeNode<int>(130, 1, 10_000);

    [Menu("Expected score after reroll",
        "Expected potential of a random new board, from 0 to 100. Raise this only if your observed rolls are better.")]
    public RangeNode<float> ExpectedRerollScore { get; set; } = new RangeNode<float>(50, 0, 100);

    [Menu("Chaos per loot point",
        "Economic calibration. 1 means a 20-point expected improvement is valued at 20 chaos.")]
    public RangeNode<float> ChaosPerLootPoint { get; set; } = new RangeNode<float>(1, 0, 10);

    [Menu("Reroll safety margin",
        "Requires the expected improvement to beat the converted reroll cost by this multiplier.")]
    public RangeNode<float> RerollSafetyMargin { get; set; } = new RangeNode<float>(1.10f, 0.50f, 3f);

    [Menu("Rerolls already used offset",
        "Manual correction if the plugin was enabled after one or more rerolls. Runtime changes are tracked automatically.")]
    public RangeNode<int> RerollsUsedOffset { get; set; } = new RangeNode<int>(0, 0, 12);

    [Menu("Layout preference strength",
        "Bonus used to prefer S/$/compact layouts on weak boards, or straight layouts on premium combinations.")]
    public RangeNode<float> LayoutPreferenceStrength { get; set; } = new RangeNode<float>(750, 0, 5_000);

    [Menu("HC Allflame: Divine (chaos)", "Snapshot inicial de 10/08/2026; atualize manualmente quando o mercado mudar.")]
    public RangeNode<float> DivineChaos { get; set; } = new RangeNode<float>(178.7f, 0.01f, 5_000);

    [Menu("HC Allflame: Annulment (chaos)")]
    public RangeNode<float> AnnulmentChaos { get; set; } = new RangeNode<float>(29.2f, 0.01f, 5_000);

    [Menu("HC Allflame: Ancient (chaos)")]
    public RangeNode<float> AncientChaos { get; set; } = new RangeNode<float>(6.54f, 0.01f, 5_000);

    [Menu("HC Allflame: Exalted (chaos)")]
    public RangeNode<float> ExaltedChaos { get; set; } = new RangeNode<float>(2.33f, 0.01f, 5_000);

    [Menu("HC Allflame: GCP (chaos)")]
    public RangeNode<float> GemcuttersChaos { get; set; } = new RangeNode<float>(2.46f, 0.01f, 5_000);

    public BorderEconomyOptions ToOptions() => new(
        SulphurPerChaos: SulphurPerChaos.Value,
        ExpectedRerollScore: ExpectedRerollScore.Value,
        ChaosPerLootPoint: ChaosPerLootPoint.Value,
        RerollSafetyMargin: RerollSafetyMargin.Value,
        DivineChaos: DivineChaos.Value,
        AnnulmentChaos: AnnulmentChaos.Value,
        AncientChaos: AncientChaos.Value,
        ExaltedChaos: ExaltedChaos.Value,
        GemcuttersChaos: GemcuttersChaos.Value);
}

[Submenu(CollapsedByDefault = true)]
public class VoyageStrategySettings
{
    [Menu("FOCO AUTOMÁTICO (recomendado)",
        "Analisa bordas + implicits + charts disponíveis, escolhe poucos temas coerentes e fortalece seus pesos. " +
        "Evita misturar muitas estratégias medianas na mesma Voyage.")]
    public ToggleNode AutomaticFocus { get; set; } = new ToggleNode(true);

    [Menu("Máximo de focos ativos",
        "O padrão é 1: cada Voyage concentra todos os recursos em um único objetivo garantido.")]
    public RangeNode<int> MaxActiveFocuses { get; set; } = new RangeNode<int>(1, 1, 3);

    [Menu("Pontuação mínima de um foco",
        "Temas abaixo desta nota são ignorados. Aumente para ser mais seletivo.")]
    public RangeNode<int> MinimumFocusScore { get; set; } = new RangeNode<int>(45, 0, 300);

    [Menu("Força mínima do foco secundário (%)",
        "O segundo foco só entra se sua nota alcançar esta porcentagem do melhor foco.")]
    public RangeNode<int> SecondaryFocusRatioPercent { get; set; } = new RangeNode<int>(68, 40, 100);

    [Menu("Bônus dos charts do foco (%)",
        "Aumenta o peso de pack size/raros, Sulphur, strongboxes etc. quando pertencem ao foco escolhido.")]
    public RangeNode<int> FocusWeightBonusPercent { get; set; } = new RangeNode<int>(85, 0, 300);

    [Menu("Peso dos temas fora do foco (%)",
        "Reduz recompensas desconectadas do plano atual; não altera o mod Default nem mods não classificados.")]
    public RangeNode<int> OffFocusMultiplierPercent { get; set; } = new RangeNode<int>(55, 10, 100);

    [Menu("Usar combo: Amuleto único + Clams",
        "Liga: coloca o Amuleto Único T2 no centro e trava 2–3 Clam-infested Shelf ao redor. " +
        "O combo é ignorado quando existe uma estratégia mais valiosa de moeda/treasure. " +
        "Desliga: o amuleto continua restrito ao centro, mas sem reservar um hub inteiro.")]
    public ToggleNode UniqueAmuletClamCross { get; set; } = new ToggleNode(true);

    [Menu("Usar combo: Moeda de monstros raros",
        "Divine/Exalted/Annulment/Ancient usa Sea Pillars no tile premiado, Strongboxes adjacentes e rare monsters globais. " +
        "Brine King é uma estratégia separada e nunca recebe suporte de Strongboxes.")]
    public ToggleNode RareMonstersDrop { get; set; } = new ToggleNode(true);

    [Menu("RARE CURRENCY: Sea Pillars + Strongboxes",
        "Quando uma borda dá Divine/Exalted/Annulment/Ancient por rare monster: coloca Sea Pillars no tile premiado, " +
        "cerca-o com o máximo de Additional/Diviner/Arcanist/Operative Strongboxes e usa Increased Rare Monsters in all Voyage Areas nos demais slots.")]
    public ToggleNode RareCurrencyStrongboxEngine { get; set; } = new ToggleNode(true);

    [Menu("Guardar até: Sea Pillars",
        "Preserva Sea Pillars para a próxima borda de currency por rare monster. Reservas são restauradas automaticamente se impedirem uma solução.")]
    public RangeNode<int> SaveSeaPillars { get; set; } = new RangeNode<int>(3, 0, 10);

    [Menu("Usar combo: Não consumir chart",
        "Em bordas fortes de chance de não consumir, usa Soul Eater e depois Anchorfield/Clams. " +
        "Só ocupa o board quando não existe um foco econômico superior.")]
    public ToggleNode NoConsumeAnchorfield { get; set; } = new ToggleNode(true);

    [Menu("Usar o centro para rewards especiais",
        "No centro livre, prioriza Operative Strongbox, Lost Message, Amulet T1, Belt ou Ring. " +
        "Amulet T2/Belt/Ring continuam centro-only para não desperdiçar adjacências.")]
    public ToggleNode CenterSpecialty { get; set; } = new ToggleNode(true);

    [Menu("Proteger: Brine King's Domain",
        "Estratégia própria: Brine King recebe borders de pack size/rare monsters; ao redor entram Adjacent Increased Rare Monsters ou Giant Starfish. " +
        "Os demais slots priorizam Increased Rare Monsters in all Voyage Areas. Strongboxes são excluídas deste combo.")]
    public ToggleNode ProtectBrineKing { get; set; } = new ToggleNode(true);

    [Menu("Guardar até: Brine King's Domain",
        "Quantidade máxima preservada quando não existe border/Giant Starfish compatível.")]
    public RangeNode<int> SaveBrineKing { get; set; } = new RangeNode<int>(6, 0, 30);

    [Menu("Proteger: charts de Strongbox",
        "Guarda Additional/Diviner/Arcanist/Operative Strongboxes e só libera para bordas em que rares dropam Divine, Exalted, Annulment ou Ancient.")]
    public ToggleNode ReserveStrongboxesForValuableCurrency { get; set; } = new ToggleNode(true);

    [Menu("Guardar até: charts de Strongbox")]
    public RangeNode<int> SaveStrongboxes { get; set; } = new RangeNode<int>(10, 0, 30);

    [Menu("Proteger: Increased Rare Monsters em todas as áreas",
        "Reserva os implicits globais para a estratégia de moeda rara ou para um Brine King totalmente suportado.")]
    public ToggleNode ReserveGlobalRareForPremiumStrategies { get; set; } = new ToggleNode(true);

    [Menu("Guardar até: Increased Rare Monsters globais")]
    public RangeNode<int> SaveGlobalRare { get; set; } = new RangeNode<int>(8, 0, 30);

    [Menu("Voyage dedicada: Messages in a Bottle",
        "Guarda os charts adjacentes e, ao atingir o mínimo, usa todos juntos ao redor de um único tile para gerar uma Voyage de objetivo único.")]
    public ToggleNode DedicatedLostMessageStrategy { get; set; } = new ToggleNode(true);

    [Menu("Mínimo de charts de Message para gastar")]
    public RangeNode<int> MinimumLostMessageCharts { get; set; } = new RangeNode<int>(3, 2, 6);

    [Menu("Guardar até: charts de Message")]
    public RangeNode<int> SaveLostMessageCharts { get; set; } = new RangeNode<int>(8, 0, 20);

    [Menu("Proteger: Sulphur global",
        "Guarda Increased Dead Man's Sulphur in all Voyage Areas e só libera quando houver border de Sulphur.")]
    public ToggleNode ReserveSulphurForSulphurBorder { get; set; } = new ToggleNode(true);

    [Menu("Guardar até: charts de Sulphur global")]
    public RangeNode<int> SaveSulphurCharts { get; set; } = new RangeNode<int>(8, 0, 30);

    [Menu("Guardar: Kishara's Rest", "Retira esses charts do solver enquanto ainda houver 9 charts utilizáveis.")]
    public ToggleNode SaveKishara { get; set; } = new ToggleNode(true);

    [Menu("Guardar: No Equipment Drops", "Preserva esses charts para uma combinação dedicada.")]
    public ToggleNode SaveNoEquipment { get; set; } = new ToggleNode(true);

    [Menu("Guardar: Fractured items", "Preserva esses charts para uma combinação dedicada.")]
    public ToggleNode SaveFractured { get; set; } = new ToggleNode(true);

    [Menu("Guardar: Golden Lanterns", "Preserva esses charts para uma combinação dedicada.")]
    public ToggleNode SaveGoldenLanterns { get; set; } = new ToggleNode(true);

    [Menu("Guardar: Pantheon", "Preserva esses charts para uma combinação dedicada.")]
    public ToggleNode SavePantheon { get; set; } = new ToggleNode(true);

    [Menu("Guardar: Soul Eater", "Preserva Soul Eater em vez de usá-lo no combo de não consumir.")]
    public ToggleNode SaveSoulEater { get; set; } = new ToggleNode(false);

    [Menu("Guardar: Rare Fracture", "Preserva esses charts para uma combinação dedicada.")]
    public ToggleNode SaveRareFracture { get; set; } = new ToggleNode(true);

    [Menu("Guardar: Rare Possessed", "Preserva esses charts para uma combinação dedicada.")]
    public ToggleNode SaveRarePossessed { get; set; } = new ToggleNode(true);

    public VoyageStrategyOptions ToOptions() => new(
        UniqueAmuletClamCross: UniqueAmuletClamCross.Value,
        RareMonstersDrop: RareMonstersDrop.Value,
        RareCurrencyStrongboxEngine: RareCurrencyStrongboxEngine.Value,
        SaveSeaPillars: SaveSeaPillars.Value,
        NoConsumeAnchorfield: NoConsumeAnchorfield.Value,
        CenterSpecialty: CenterSpecialty.Value,
        AutomaticFocus: AutomaticFocus.Value,
        MaxActiveFocuses: MaxActiveFocuses.Value,
        MinimumFocusScore: MinimumFocusScore.Value,
        SecondaryFocusRatio: SecondaryFocusRatioPercent.Value / 100d,
        FocusWeightBonus: FocusWeightBonusPercent.Value / 100d,
        OffFocusMultiplier: OffFocusMultiplierPercent.Value / 100d,
        ProtectBrineKing: ProtectBrineKing.Value,
        UseBrineKingSynergy: ProtectBrineKing.Value,
        SaveBrineKing: SaveBrineKing.Value,
        ReserveStrongboxesForValuableCurrency: ReserveStrongboxesForValuableCurrency.Value,
        SaveStrongboxes: SaveStrongboxes.Value,
        ReserveGlobalRareForPremiumStrategies: ReserveGlobalRareForPremiumStrategies.Value,
        SaveGlobalRare: SaveGlobalRare.Value,
        DedicatedLostMessageStrategy: DedicatedLostMessageStrategy.Value,
        MinimumLostMessageCharts: MinimumLostMessageCharts.Value,
        SaveLostMessageCharts: SaveLostMessageCharts.Value,
        ReserveSulphurForSulphurBorder: ReserveSulphurForSulphurBorder.Value,
        SaveSulphurCharts: SaveSulphurCharts.Value,
        SaveKishara: SaveKishara.Value,
        SaveNoEquipment: SaveNoEquipment.Value,
        SaveFractured: SaveFractured.Value,
        SaveGoldenLanterns: SaveGoldenLanterns.Value,
        SavePantheon: SavePantheon.Value,
        SaveSoulEater: SaveSoulEater.Value,
        SaveRareFracture: SaveRareFracture.Value,
        SaveRarePossessed: SaveRarePossessed.Value);
}

[Submenu(CollapsedByDefault = false)]
public class VoyageStrategySettingsV2
{
    [Menu("Planejamento automático (recomendado)",
        "Escolhe exatamente uma estratégia. Só gasta charts valiosos quando o pacote completo existe; caso contrário monta uma Fast Voyage.")]
    public ToggleNode AutomaticFocus { get; set; } = new ToggleNode(true);

    [Menu("Proteger estoques premium",
        "Reserva Operative, Diviner, Strongboxes, rare monsters globais/adjacentes, Sea Pillars, Message e Sulphur 25%. O fallback não consome esses estoques.")]
    public ToggleNode ProtectPremiumCharts { get; set; } = new ToggleNode(true);

    [Menu("Moeda rara: mínimo de Strongboxes",
        "Além de Sea Pillars e da borda de Divine/Annulment/Ancient/Exalted, exige esta quantidade de suportes adjacentes.")]
    public RangeNode<int> MinimumRareCurrencyStrongboxes { get; set; } = new RangeNode<int>(3, 2, 4);

    [Menu("Moeda rara: mínimo de rare monsters globais")]
    public RangeNode<int> MinimumRareCurrencyGlobalRare { get; set; } = new RangeNode<int>(5, 3, 6);

    [Menu("Tamanho do conjunto Operative/Diviner",
        "Guarda cada família separadamente e usa o conjunto inteiro em uma Voyage dedicada.")]
    public RangeNode<int> DedicatedStrongboxSetSize { get; set; } = new RangeNode<int>(9, 6, 9);

    [Menu("Messages necessários para gastar",
        "O padrão usa oito charts adjacentes e um chart-alvo.")]
    public RangeNode<int> MinimumLostMessageCharts { get; set; } = new RangeNode<int>(8, 4, 8);

    [Menu("Charts de Sulphur necessários",
        "Só ativa com a borda de Sulphur e esta quantidade de charts que atingem o percentual mínimo.")]
    public RangeNode<int> MinimumSulphurCharts { get; set; } = new RangeNode<int>(9, 6, 9);

    [Menu("Sulphur mínimo por chart (%)")]
    public RangeNode<int> MinimumSulphurPercent { get; set; } = new RangeNode<int>(25, 15, 25);

    [Menu("Fast Voyage com charts de descarte",
        "Quando nenhum pacote estiver completo, favorece Barrels, Imprisoned Monsters, Soul Eater e Tormented Spirits. O formato continua obedecendo S/$.")]
    public ToggleNode PreferFastVoyageFillers { get; set; } = new ToggleNode(true);

    public VoyageStrategyOptions ToOptions() => new(
        AutomaticFocus: AutomaticFocus.Value,
        ProtectPremiumCharts: ProtectPremiumCharts.Value,
        MinimumRareCurrencyStrongboxes: MinimumRareCurrencyStrongboxes.Value,
        MinimumRareCurrencyGlobalRare: MinimumRareCurrencyGlobalRare.Value,
        DedicatedStrongboxSetSize: DedicatedStrongboxSetSize.Value,
        MinimumLostMessageCharts: MinimumLostMessageCharts.Value,
        SaveLostMessageCharts: 8,
        MinimumSulphurCharts: MinimumSulphurCharts.Value,
        MinimumSulphurPercent: MinimumSulphurPercent.Value,
        SaveSulphurCharts: 9,
        SaveStrongboxes: 9,
        SaveGlobalRare: 9,
        PreferFastVoyageFillers: PreferFastVoyageFillers.Value,
        RareMonstersDrop: false,
        RareCurrencyStrongboxEngine: false,
        UseBrineKingSynergy: false,
        UniqueAmuletClamCross: false,
        NoConsumeAnchorfield: false,
        CenterSpecialty: false,
        SaveKishara: false,
        SaveNoEquipment: false,
        SaveFractured: false,
        SaveGoldenLanterns: false,
        SavePantheon: false,
        SaveSoulEater: false,
        SaveRareFracture: false,
        SaveRarePossessed: false);
}

[Submenu(CollapsedByDefault = true)]
public class VoyageExcludedChartSettings
{
    private static readonly ConcurrentDictionary<string, ItemQuery<ChartData>> FilterCache = [];

    public VoyageExcludedChartSettings()
    {
        Status.DrawDelegate = () =>
        {
            if (Query.FailedToCompile)
            {
                ImGui.Text($"Compilation failed: {Query.Error}");
            }
        };
    }

    [JsonIgnore]
    public CustomNode Status { get; set; } = new CustomNode();

    [Menu("IFL")]
    public TextNode IFL { get; set; } = new TextNode("false");
    public ToggleNode Enabled { get; set; } = new ToggleNode(true);

    [IgnoreMenu]
    [JsonIgnore]
    public ItemQuery<ChartData> Query => FilterCache.GetOrAdd(IFL.Value, ItemQuery.Load<ChartData>);

    public override string ToString()
    {
        return $"{(Enabled ? "" : "[Disabled]")}{IFL.Value}###";
    }
}

public class ChartData : ItemData
{
    public Vector2i Pos { get; }

    public ChartData(Entity queriedItem, GameController gc, Vector2i pos) 
        : base(queriedItem, gc)
    {
        Pos = pos;
    }

    public ChartData(Entity queriedItem, Entity groundItem, GameController gameController, Vector2i pos) 
        : base(queriedItem, groundItem, gameController)
    {
        Pos = pos;
    }
}

public class VoyageProfileEntry
{
    public string Name;
    public VoyageProfile Profile;
}

[Submenu(CollapsedByDefault = true)]
public class VoyageBorderModifier
{
    public TextNode Id { get; set; } = new TextNode("");    
    public TextNode Abbreviation { get; set; } = new TextNode("");

    [Menu(null, "For ordinary per-connection borders this is the multiplier per single connection: effective = 1 + (multiplier - 1) x connections. Quantity-per-connection reads its fixed bonus and penalty directly from game memory.")]
    public RangeNode<float> ValueMultiplier { get; set; } = new RangeNode<float>(1, 0, 10);

    [Menu(null, "Comma-separated reward categories this border boosts (e.g. 'Monsters, RareMonsters'). " +
                "'All' matches every chart modifier, 'None' makes the border inert for scoring (flat value). " +
                "Empty = All (legacy behavior). Categories: Monsters, MagicMonsters, RareMonsters, Essences, Strongboxes, " +
                "Uniques, Currency, Scarabs, Gold, Equipment, Experience, Resources, Sulphur, Lanterns, Rarity")]
    public TextNode Tags { get; set; } = new TextNode("");

    [Menu("Per connection", "Multiplier scales with the connection count of the chart placed on the affected tile ('... per Chart connection' borders)")]
    public ToggleNode PerConnection { get; set; } = new ToggleNode(false);

    [Menu("Affects placed chart", "Multiplies the modifiers of the chart placed on the adjacent tile (e.g. 'increased effect of adjacent Charts', chart refunds) instead of rewards landing on that tile")]
    public ToggleNode AffectsPlacedChart { get; set; } = new ToggleNode(false);

    public ColorNode HighlightColor { get; set; } = Color.Cyan;

    public override string ToString()
    {
        var tags = ModifierTagParser.Parse(Tags.Value, ModifierTag.All);
        return $"{Id.Value} x{ValueMultiplier.Value}{(PerConnection.Value ? "/conn" : "")}{(AffectsPlacedChart.Value ? " [chart]" : "")} ({tags})###";
    }
}

[Submenu(CollapsedByDefault = true)]
public class VoyageChartModifier
{
    public TextNode Id { get; set; } = new TextNode("");
    public TextNode Label { get; set; } = new TextNode("");
    public RangeNode<float> Weight { get; set; } = new RangeNode<float>(0, 0, 100);
    public ToggleNode IsGlobal { get; set; } = new ToggleNode(false);

    [Menu(null, "Comma-separated reward categories this modifier's reward belongs to. Empty/'None' = " +
                "not boosted by any category-specific border (only by 'All' borders). Categories: Monsters, MagicMonsters, " +
                "RareMonsters, Essences, Strongboxes, Uniques, Currency, Scarabs, Gold, Equipment, Experience, Resources, Sulphur, Lanterns, Rarity")]
    public TextNode Tags { get; set; } = new TextNode("");

    public ColorNode HighlightColor { get; set; } = Color.Violet;

    public override string ToString()
    {
        var tags = ModifierTagParser.Parse(Tags.Value, ModifierTag.None);
        return $"{Id.Value} {Weight.Value} ({tags})###";
    }
}

public enum SearchState
{
    Empty,
    Searching,
    Stopped,
}
