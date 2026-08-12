using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace DeepwaterEngagementSuite;

[JsonConverter(typeof(StringEnumConverter))]
public enum IconPickerIndex
{
    OtherChests,
    SunkenLoot,
    BottledItemChest,
    GoldTreasureChest,
    GoldPile,
    ClamTreasureChest,
    CurrencyTreasureChest,
    CurrencyTreasureChestOpulent,
    CurrencyGemcuttersChest,
    UniqueWeaponChest,
    UniqueArmourChest,
    UniqueJewelleryChest,
    ScarabChest,
    StackedDecksChest,
    MapsChest,
    AllflameEmbersChest,
    CursedDucatDrop,
    RandomDucatChest,
    HazardBoatChest,
    IzaroObject,
    AltarCrab,
    AltarOctopus,
    TormentedSpiritEncounter,
    LanternReplenishEncounter,
    GoldenLanternEncounter,
    InfusedCoralEncounter,
    StrongboxDivination,
    StrongboxScarab,
    StrongboxArcanist,
    StrongboxOperative,
    StrongboxGeneric,
    PointerTarget,
    DeadMansSulphurSmall,
    DeadMansSulphurBase,
    DeadMansSulphurLarge,
    DeadMansSulphurHuge,
}
