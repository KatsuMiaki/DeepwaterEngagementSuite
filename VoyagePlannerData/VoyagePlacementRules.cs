using System;
using System.Collections.Generic;
using System.Linq;

namespace DeepwaterEngagementSuite.VoyagePlannerData;

public static class VoyagePlacementRules
{
    public const string NotConsume1 = "DeepwaterBorderChanceToNotConsumeChart1";
    public const string NotConsume2 = "DeepwaterBorderChanceToNotConsumeChart2";
    public const string RareDivine = "DeepwaterBorderRareMonsterDivine";
    public const string RareExalted = "DeepwaterBorderRareMonsterExalted";
    public const string RareAnnul = "DeepwaterBorderRareMonsterAnnulment";
    public const string RareAncient = "DeepwaterBorderRareMonsterAncient";
    public const string TreasureAnchors1 = "DeepwaterBorderTreasureAnchors1";
    public const string TreasureAnchors2 = "DeepwaterBorderTreasureAnchors2";

    public const string VoyageIncreasedRareMonsters = "MapDeepwaterChartVoyageIncreasedRareMonsters";
    public const string VoyageResourceFoundPrefix = "MapDeepwaterChartVoyageResourceFound";
    public const string VoyageNoEquipmentDrops = "MapDeepwaterChartVoyageNoEquipmentDrops";
    public const string VoyageSoulEater = "MapDeepwaterChartVoyageSoulEater";
    public const string VoyageRareFracture = "MapDeepwaterChartVoyageRareFracture";
    public const string VoyageMonstersPossessed = "MapDeepwaterChartVoyageMonstersPossessed";
    public const string AdjacentFracturedPrefix = "MapDeepwaterChartAdjacentFractured";
    public const string AdjacentGoldenLanternsPrefix = "MapDeepwaterChartAdjacentGoldenLanterns";
    public const string AdjacentPantheonPrefix = "MapDeepwaterChartAdjacentPantheon";
    public const string AdjacentStrongboxesPrefix = "MapDeepwaterChartAdjacentStrongboxes";
    public const string AdjacentStarfishPrefix = "MapDeepwaterChartAdjacentStarfish";
    public const string AdjacentIncreasedRarePrefix = "MapDeepwaterChartAdjacentIncreasedRareMonsters";
    public const string AdjacentDivinerBoxPrefix = "MapDeepwaterChartAdjacentDivinerBox";
    public const string AdjacentArcanistBoxPrefix = "MapDeepwaterChartAdjacentArcanistBox";
    public const string AdjacentOperativeBoxPrefix = "MapDeepwaterChartAdjacentOperativeBox";
    public const string AdjacentLostMessagePrefix = "MapDeepwaterChartAdjacentLostMessage";
    public const string AdjacentUniqueAmuletPrefix = "MapDeepwaterChartAdjacentUniqueAmulet";
    public const string AdjacentUniqueBeltPrefix = "MapDeepwaterChartAdjacentUniqueBelt";
    public const string AdjacentUniqueRingPrefix = "MapDeepwaterChartAdjacentUniqueRing";

    public const int CenterRow = 1;
    public const int CenterCol = 1;

    public const int MaxSavedBoxes = 9;
    public const int MaxSavedStarfish = 9;
    public const int MaxSavedRareVoyage = 9;
    public const int MaxSavedPelagic = 2;
    public const int MaxSavedUniqueAmulet2 = 1;
    public const int MaxSavedClamsForAmulet = 3;

    public const string PelagicRoomName = "Pelagic Abyss";
    public const string ClamRoomName = "Clam-infested Shelf";
    public const string AnchorfieldRoomName = "Anchorfield";
    public const string KisharaRoomName = "Kishara's Rest";
    public const string BrineKingRoomName = "Brine King's Domain";
    public const string SeaPillarsRoomName = "Sea Pillars";

    private static readonly (int Dr, int Dc)[] Ortho = [(1, 0), (-1, 0), (0, 1), (0, -1)];

    public sealed record Result(
        List<MapPiece> Pieces,
        List<LockedPlacement> Locks,
        int SavedPelagicCount,
        int SavedFarmCount,
        int SavedStrongboxCount,
        int SavedStarfishCount,
        int SavedRareVoyageCount,
        int SavedAdjacentRareCount,
        int SavedOperativeBoxCount,
        int SavedDivinerBoxCount,
        int SavedLostMessageCount,
        int SavedSulphurCount,
        int SavedKisharaCount,
        int SavedNoEquipmentCount,
        int SavedFracturedCount,
        int SavedGoldenLanternsCount,
        int SavedPantheonCount,
        int SavedSoulEaterCount,
        int SavedRareFractureCount,
        int SavedRarePossessedCount,
        int SavedClamCount,
        int SavedUniqueAmuletCount,
        int SavedUniqueBeltCount,
        int SavedUniqueRingCount,
        int SavedBrineKingCount,
        int SavedSeaPillarsCount,
        bool AmuletClamHubActive = false,
        bool PreferClamsAdjacentToAmulet = false,
        bool NoConsumeActive = false,
        bool BrineKingSynergyActive = false,
        IReadOnlyList<string> ActiveStrategies = null);

    public const double ClamAdjacentToAmuletMultiplier = 1_000_000d;

    public static readonly (int Row, int Col)[] SacrificeCorners = [(2, 0), (2, 2), (0, 2)];

    public static bool IsSacrificeCorner(int row, int col) =>
        (row, col) is (2, 0) or (2, 2) or (0, 2);

    public static Result Apply(
        IReadOnlyList<MapPiece> pieces,
        IReadOnlyList<BorderEffect>[,] tileBorders,
        VoyageStrategyOptions options = null,
        bool reserveCharts = true,
        bool disablePlacementRules = false)
    {
        if (disablePlacementRules)
        {
            return new Result(
                Pieces: pieces.ToList(),
                Locks: [],
                SavedPelagicCount: 0,
                SavedFarmCount: 0,
                SavedStrongboxCount: 0,
                SavedStarfishCount: 0,
                SavedRareVoyageCount: 0,
                SavedAdjacentRareCount: 0,
                SavedOperativeBoxCount: 0,
                SavedDivinerBoxCount: 0,
                SavedLostMessageCount: 0,
                SavedSulphurCount: 0,
                SavedKisharaCount: 0,
                SavedNoEquipmentCount: 0,
                SavedFracturedCount: 0,
                SavedGoldenLanternsCount: 0,
                SavedPantheonCount: 0,
                SavedSoulEaterCount: 0,
                SavedRareFractureCount: 0,
                SavedRarePossessedCount: 0,
                SavedClamCount: 0,
                SavedUniqueAmuletCount: 0,
                SavedUniqueBeltCount: 0,
                SavedUniqueRingCount: 0,
                SavedBrineKingCount: 0,
                SavedSeaPillarsCount: 0,
                ActiveStrategies: []);
        }

        options ??= VoyageStrategyOptions.AllEnabled;
        var working = pieces.ToList();
        var locks = new List<LockedPlacement>();
        var usedPieceIds = new HashSet<int>();
        var lockedCells = new HashSet<(int Row, int Col)>();

        void LockCell(int row, int col, MapPiece piece, int? rotation = null)
        {
            if (!usedPieceIds.Add(piece.Id))
                return;
            locks.Add(new LockedPlacement(row, col, piece.Id, rotation));
            lockedCells.Add((row, col));
        }

        bool CellFree(int row, int col) => !lockedCells.Contains((row, col));

        int SaveByPredicate(bool enabled, Func<MapPiece, bool> pred)
        {
            if (!enabled || !reserveCharts)
                return 0;
            var saved = 0;
            foreach (var id in working.Where(pred).Select(p => p.Id).ToList())
            {
                if (!TrySavePiece(working, id))
                    break;
                saved++;
            }
            return saved;
        }

        var savedKishara = SaveByPredicate(options.SaveKishara, IsKishara);
        var savedNoEquipment = SaveByPredicate(options.SaveNoEquipment, IsNoEquipmentChart);
        var savedFractured = SaveByPredicate(options.SaveFractured, IsFracturedChart);
        var savedGoldenLanterns = SaveByPredicate(options.SaveGoldenLanterns, IsGoldenLanternsChart);
        var savedPantheon = SaveByPredicate(options.SavePantheon, IsPantheonChart);
        var savedSoulEater = SaveByPredicate(options.SaveSoulEater, IsSoulEaterChart);
        var savedRareFracture = SaveByPredicate(options.SaveRareFracture, IsRareFractureChart);
        var savedRarePossessed = SaveByPredicate(options.SaveRarePossessed, IsRarePossessedChart);

        // Premium stockpiles are hard reservations. They are not restored by the generic solver
        // fallback: an incomplete premium set must never be burned merely to complete a filler run.
        var setSize = Math.Clamp(options.DedicatedStrongboxSetSize, 1, 9);
        var savedOperative = !options.ProtectPremiumCharts || options.OperativeFleetActive ? 0 : RemoveUnused(
            working, usedPieceIds, p => IsOperativeBoxChart(p) && !IsDivinerBoxChart(p),
            OperativeBoxScore, maxSave: setSize, force: true);
        var savedDiviner = !options.ProtectPremiumCharts || options.DivinerFleetActive ? 0 : RemoveUnused(
            working, usedPieceIds, p => IsDivinerBoxChart(p) && !IsOperativeBoxChart(p),
            DivinerBoxScore, maxSave: setSize, force: true);
        var savedLostMessage = !options.ProtectPremiumCharts || options.DedicatedMessageActive ? 0 : RemoveUnused(
            working, usedPieceIds, IsLostMessageChart, LostMessageScore,
            maxSave: Math.Clamp(options.SaveLostMessageCharts, 0, 8), force: true);
        var savedSulphur = !options.ProtectPremiumCharts || options.SulphurStrategyActive ? 0 : RemoveUnused(
            working, usedPieceIds, p => IsHighValueSulphurChart(p, options.MinimumSulphurPercent),
            SulphurChartScore, maxSave: Math.Clamp(options.SaveSulphurCharts, 0, 9), force: true);
        var rareCurrencyActive = options.RareCurrencyStrongboxEngine;
        var rareDensityActive = rareCurrencyActive || options.UseBrineKingSynergy;
        var savedStrongbox = !options.ProtectPremiumCharts || rareCurrencyActive ? 0 : RemoveUnused(
            working, usedPieceIds,
            p => IsStrongboxCountChart(p) && !IsOperativeBoxChart(p) && !IsDivinerBoxChart(p),
            BoxValue1Score, maxSave: Math.Clamp(options.SaveStrongboxes, 0, 9), force: true);
        var savedRareVoyage = !options.ProtectPremiumCharts || rareDensityActive ? 0 : RemoveUnused(
            working, usedPieceIds, IsRareVoyageChart, RareVoyageScore,
            maxSave: Math.Clamp(options.SaveGlobalRare, 0, 9), force: true);
        var savedAdjacentRare = !options.ProtectPremiumCharts || rareDensityActive ? 0 : RemoveUnused(
            working, usedPieceIds, IsAdjacentRareChart, AdjacentRareScore,
            maxSave: 9, force: true);
        var savedStarfish = !options.ProtectPremiumCharts || rareDensityActive ? 0 : RemoveUnused(
            working, usedPieceIds, IsStarfishChart, StarfishScore,
            maxSave: MaxSavedStarfish, force: true);
        var savedSeaPillars = !options.ProtectPremiumCharts || rareCurrencyActive ? 0 : RemoveUnused(
            working, usedPieceIds, IsSeaPillars, SeaPillarsScore,
            maxSave: Math.Clamp(options.SaveSeaPillars, 0, 9), force: true);

        var divineCenters = EnumerateCells()
            .Where(c => OrbPriority(BordersAt(tileBorders, c.Row, c.Col)) == 4)
            .Select(c => (c.Row, c.Col))
            .ToList();

        var exaltedCenters = EnumerateCells()
            .Where(c => OrbPriority(BordersAt(tileBorders, c.Row, c.Col)) == 3)
            .Select(c => (c.Row, c.Col))
            .ToList();

        var annulCenters = EnumerateCells()
            .Where(c => OrbPriority(BordersAt(tileBorders, c.Row, c.Col)) == 2)
            .Select(c => (c.Row, c.Col))
            .ToList();

        var ancientCenters = EnumerateCells()
            .Where(c => OrbPriority(BordersAt(tileBorders, c.Row, c.Col)) == 1)
            .Select(c => (c.Row, c.Col))
            .ToList();

        var orbCenters = divineCenters.Select(c => (c.Row, c.Col, Priority: 4))
            .Concat(exaltedCenters.Select(c => (c.Row, c.Col, Priority: 3)))
            .Concat(annulCenters.Select(c => (c.Row, c.Col, Priority: 2)))
            .Concat(ancientCenters.Select(c => (c.Row, c.Col, Priority: 1)))
            .OrderByDescending(x => x.Priority)
            .ToList();

        var clamCountAtStart = working.Count(p => !usedPieceIds.Contains(p.Id) && IsClamChart(p));
        var surplusClams = clamCountAtStart > MaxSavedClamsForAmulet;
        var hasOrbs = orbCenters.Count > 0;
        var strongTreasure = BoardHasStrongTreasureAnchors(tileBorders);
        var operativeFleetActive = false;
        if (options.OperativeFleetActive)
        {
            foreach (var cell in EnumerateCells())
            {
                var chart = TakeBest(working, usedPieceIds, IsOperativeBoxChart, OperativeBoxScore);
                if (chart == null)
                    break;
                LockCell(cell.Row, cell.Col, chart);
            }
            operativeFleetActive = locks.Count == 9;
        }

        var divinerFleetActive = false;
        if (options.DivinerFleetActive)
        {
            foreach (var cell in EnumerateCells())
            {
                var chart = TakeBest(working, usedPieceIds, IsDivinerBoxChart, DivinerBoxScore);
                if (chart == null)
                    break;
                LockCell(cell.Row, cell.Col, chart);
            }
            divinerFleetActive = locks.Count == 9;
        }

        // The rewarded tile is the kill chamber. Sea Pillars supplies natural density there;
        // adjacent charts then inject rollable strongboxes into the same rare-currency area.
        var seaPillarsStrongboxEngineActive = false;
        if (options.RareMonstersDrop && options.RareCurrencyStrongboxEngine && hasOrbs)
        {
            var target = orbCenters.FirstOrDefault(c => CellFree(c.Row, c.Col));
            if (target.Priority > 0)
            {
                var seaPillars = TakeBest(working, usedPieceIds, IsSeaPillars, SeaPillarsScore);
                if (seaPillars != null)
                {
                    LockCell(target.Row, target.Col, seaPillars);
                    seaPillarsStrongboxEngineActive = true;

                    foreach (var neighbor in FreeNeighbors(target.Row, target.Col, CellFree))
                    {
                        var box = TakeBest(working, usedPieceIds, IsStrongboxCountChart, BoxValue1Score);
                        if (box == null)
                            break;
                        LockCell(neighbor.Row, neighbor.Col, box);
                    }

                    foreach (var cell in EnumerateCells()
                                 .Where(c => CellFree(c.Row, c.Col))
                                 .OrderByDescending(c => HasChartEffectBorder(BordersAt(tileBorders, c.Row, c.Col)))
                                 .ThenByDescending(c => InGridDegree(c.Row, c.Col)))
                    {
                        var globalRare = TakeBest(working, usedPieceIds, IsRareVoyageChart, RareVoyageScore);
                        if (globalRare == null)
                            break;
                        LockCell(cell.Row, cell.Col, globalRare);
                    }
                }
            }
        }

        var brineKingSynergyActive = false;
        if (options.ProtectBrineKing && options.UseBrineKingSynergy && options.RareMonstersDrop)
        {
            var rareBoostTargets = EnumerateCells()
                .Where(c => CellFree(c.Row, c.Col) && HasRareMonsterBoostBorder(BordersAt(tileBorders, c.Row, c.Col)))
                .OrderByDescending(c => OrbPriority(BordersAt(tileBorders, c.Row, c.Col)))
                .ThenByDescending(c => InGridDegree(c.Row, c.Col))
                .ToList();

            foreach (var brine in working.Where(IsBrineKingsDomain)
                         .OrderByDescending(BrineKingScore).ToList())
            {
                if (usedPieceIds.Contains(brine.Id) || rareBoostTargets.Count == 0)
                    continue;
                var target = rareBoostTargets[0];
                rareBoostTargets.RemoveAt(0);
                LockCell(target.Row, target.Col, brine);
                orbCenters.RemoveAll(c => c.Row == target.Row && c.Col == target.Col);
                brineKingSynergyActive = true;

                // Brine King does not use the Strongbox engine. Its own monster population is
                // amplified by adjacent rare-monster/Starfish charts instead.
                foreach (var neighbor in FreeNeighbors(target.Row, target.Col, CellFree))
                {
                    var support = TakeBest(working, usedPieceIds, IsAdjacentRareChart, AdjacentRareScore)
                                  ?? TakeBest(working, usedPieceIds, IsStarfishChart, StarfishScore);
                    if (support == null)
                        break;
                    LockCell(neighbor.Row, neighbor.Col, support);
                }
            }

            foreach (var cell in EnumerateCells()
                         .Where(c => CellFree(c.Row, c.Col))
                         .OrderByDescending(c => HasChartEffectBorder(BordersAt(tileBorders, c.Row, c.Col)))
                         .ThenByDescending(c => InGridDegree(c.Row, c.Col)))
            {
                var globalRare = TakeBest(working, usedPieceIds, IsRareVoyageChart, RareVoyageScore);
                if (globalRare == null)
                    break;
                LockCell(cell.Row, cell.Col, globalRare);
            }
        }

        var messageStrategyActive = false;
        if (options.DedicatedMessageActive && CellFree(CenterRow, CenterCol))
        {
            var messages = working
                .Where(p => !usedPieceIds.Contains(p.Id) && IsLostMessageChart(p))
                .OrderByDescending(LostMessageScore)
                .Take(8)
                .ToList();
            if (messages.Count >= Math.Max(2, options.MinimumLostMessageCharts))
            {
                var target = working
                    .Where(p => !usedPieceIds.Contains(p.Id) && !IsLostMessageChart(p) && !IsPantheonChart(p))
                    .OrderByDescending(p => p.LocalModifier + p.GlobalModifier)
                    .FirstOrDefault();
                if (target != null)
                {
                    LockCell(CenterRow, CenterCol, target);
                    var primaryMessages = messages.Take(4).ToList();
                    foreach (var pair in FreeNeighbors(CenterRow, CenterCol, CellFree).Zip(primaryMessages))
                        LockCell(pair.First.Row, pair.First.Col, pair.Second);
                    foreach (var message in messages.Skip(primaryMessages.Count))
                    {
                        var cell = EnumerateCells().FirstOrDefault(c => CellFree(c.Row, c.Col));
                        if (!CellFree(cell.Row, cell.Col))
                            break;
                        LockCell(cell.Row, cell.Col, message);
                    }
                    messageStrategyActive = true;
                }
            }
        }

        var sulphurStrategyActive = false;
        if (options.SulphurStrategyActive)
        {
            foreach (var cell in EnumerateCells()
                         .Where(c => CellFree(c.Row, c.Col))
                         .OrderByDescending(c => HasChartEffectBorder(BordersAt(tileBorders, c.Row, c.Col)))
                         .ThenByDescending(c => InGridDegree(c.Row, c.Col)))
            {
                var sulphur = TakeBest(working, usedPieceIds,
                    p => IsHighValueSulphurChart(p, options.MinimumSulphurPercent), SulphurChartScore);
                if (sulphur == null)
                    break;
                LockCell(cell.Row, cell.Col, sulphur);
                sulphurStrategyActive = true;
            }
        }

        var groundLootStrategyActive = false;
        if (options.GroundLootStrategyActive)
        {
            var target = EnumerateCells()
                .Where(c => CellFree(c.Row, c.Col))
                .OrderByDescending(c => HasRareMonsterBoostBorder(BordersAt(tileBorders, c.Row, c.Col)))
                .ThenByDescending(c => InGridDegree(c.Row, c.Col))
                .FirstOrDefault();
            var seaPillars = TakeBest(working, usedPieceIds, IsSeaPillars, SeaPillarsScore);
            if (seaPillars != null)
            {
                LockCell(target.Row, target.Col, seaPillars);
                groundLootStrategyActive = true;
                foreach (var neighbor in FreeNeighbors(target.Row, target.Col, CellFree))
                {
                    var support = TakeBest(working, usedPieceIds, IsStarfishChart, StarfishScore)
                                  ?? TakeBest(working, usedPieceIds, IsGoldenLanternsChart, p => p.LocalModifier + p.GlobalModifier);
                    if (support == null)
                        break;
                    LockCell(neighbor.Row, neighbor.Col, support);
                }
            }

            foreach (var global in working
                         .Where(p => !usedPieceIds.Contains(p.Id) &&
                                     (IsNoEquipmentChart(p) || IsRarePossessedChart(p)))
                         .OrderByDescending(p => p.LocalModifier + p.GlobalModifier)
                         .ToList())
            {
                var cell = EnumerateCells().FirstOrDefault(c => CellFree(c.Row, c.Col));
                if (!CellFree(cell.Row, cell.Col))
                    break;
                LockCell(cell.Row, cell.Col, global);
            }
        }

        var amuletCrossLocked = false;
        var preferClamsAdjacentToAmulet = false;
        var amuletCenterLocked = false;
        if (CellFree(CenterRow, CenterCol))
        {
            if (options.UniqueAmuletClamCross && !strongTreasure && !hasOrbs)
            {
                amuletCrossLocked = TryLockAmuletClamHub(
                    working, usedPieceIds, CellFree, LockCell);
            }
            else if (!options.UniqueAmuletClamCross)
            {
                preferClamsAdjacentToAmulet = TryLockUniqueAmulet2Center(
                    working, usedPieceIds, LockCell);
                amuletCenterLocked = preferClamsAdjacentToAmulet;
            }
        }

        var savedPelagic = 0;
        var pelagicLocked = false;
        if (options.RareMonstersDrop)
        {
            foreach (var pelagic in working.Where(IsPelagic)
                         .OrderByDescending(p => p.LocalModifier + p.GlobalModifier).ToList())
            {
                if (usedPieceIds.Contains(pelagic.Id))
                    continue;

                var target = orbCenters.FirstOrDefault(c => CellFree(c.Row, c.Col));
                if (target.Priority > 0)
                {
                    LockCell(target.Row, target.Col, pelagic);
                    orbCenters.RemoveAll(c => c.Row == target.Row && c.Col == target.Col);
                    pelagicLocked = true;
                }
                else if (reserveCharts && savedPelagic < MaxSavedPelagic && TrySavePiece(working, pelagic.Id))
                {
                    savedPelagic++;
                }
            }
        }

        if (options.RareMonstersDrop && hasOrbs && !seaPillarsStrongboxEngineActive)
        {
            var fallbackTarget = divineCenters.Select(c => (c.Row, c.Col, Priority: 4))
                .Concat(exaltedCenters.Select(c => (c.Row, c.Col, Priority: 3)))
                .Concat(annulCenters.Select(c => (c.Row, c.Col, Priority: 2)))
                .Concat(ancientCenters.Select(c => (c.Row, c.Col, Priority: 1)))
                .OrderByDescending(x => x.Priority)
                .ThenByDescending(x => InGridDegree(x.Row, x.Col))
                .FirstOrDefault();

            foreach (var n in FreeNeighbors(fallbackTarget.Row, fallbackTarget.Col, CellFree))
            {
                var support = TakeBest(working, usedPieceIds, IsStrongboxCountChart, BoxValue1Score)
                              ?? TakeBest(working, usedPieceIds, IsStarfishChart, StarfishScore)
                              ?? TakeBest(working, usedPieceIds, IsAdjacentRareChart, AdjacentRareScore);
                if (support == null)
                    break;
                LockCell(n.Row, n.Col, support);
            }

            foreach (var cell in EnumerateCells()
                         .Where(c => CellFree(c.Row, c.Col))
                         .OrderByDescending(c => HasChartEffectBorder(BordersAt(tileBorders, c.Row, c.Col)))
                         .ThenByDescending(c => InGridDegree(c.Row, c.Col)))
            {
                var rare = TakeBest(working, usedPieceIds, IsRareVoyageChart, RareVoyageScore);
                if (rare == null)
                    break;
                LockCell(cell.Row, cell.Col, rare);
            }
        }

        if (options.CenterSpecialty && CellFree(CenterRow, CenterCol))
        {
            var centerPiece = TakeBest(working, usedPieceIds, IsOperativeBoxChart, OperativeBoxScore)
                              ?? TakeBest(working, usedPieceIds, IsLostMessageChart, LostMessageScore)
                              ?? TakeBest(working, usedPieceIds, IsUniqueAmulet1Chart, UniqueAmuletScore)
                              ?? TakeBest(working, usedPieceIds, IsUniqueBeltChart, UniqueBeltScore)
                              ?? TakeBest(working, usedPieceIds, IsUniqueRingChart, UniqueRingScore);
            if (centerPiece != null)
                LockCell(CenterRow, CenterCol, centerPiece);
        }

        var noConsumeActive = false;
        if (options.NoConsumeAnchorfield &&
            !strongTreasure &&
            !hasOrbs &&
            !amuletCrossLocked)
        {
            foreach (var cell in EnumerateCells().Where(c =>
                         CellFree(c.Row, c.Col) &&
                         IsStrongNoConsume(BordersAt(tileBorders, c.Row, c.Col))))
            {
                var farm = TakeBest(working, usedPieceIds, IsSoulEaterChart, SoulEaterScore)
                           ?? TakeBest(working, usedPieceIds, IsAnchorfieldChart, FarmPriority);
                if (farm == null && surplusClams)
                    farm = TakeBest(working, usedPieceIds, IsClamChart, ClamScore);
                if (farm == null) break;
                LockCell(cell.Row, cell.Col, farm);
                noConsumeActive = true;
            }
        }

        var savedFarm = reserveCharts && options.NoConsumeAnchorfield
            ? RemoveUnused(working, usedPieceIds, IsAnchorfieldChart, FarmPriority)
            : 0;

        var savedBrineKing = options.ProtectPremiumCharts && options.ProtectBrineKing && !options.UseBrineKingSynergy
            ? RemoveUnused(working, usedPieceIds, IsBrineKingsDomain, BrineKingScore,
                maxSave: Math.Clamp(options.SaveBrineKing, 0, 9), force: true)
            : 0;

        var savedUniqueAmulet = 0;
        var savedClam = 0;
        if (reserveCharts && options.UniqueAmuletClamCross && !amuletCrossLocked)
        {
            savedUniqueAmulet = RemoveUnused(working, usedPieceIds, IsUniqueAmulet2Chart,
                UniqueAmuletScore, maxSave: MaxSavedUniqueAmulet2, force: true);
            savedClam = RemoveUnused(working, usedPieceIds, IsClamChart, ClamScore,
                maxSave: MaxSavedClamsForAmulet, force: true);
        }

        if (reserveCharts && options.UniqueAmuletClamCross && surplusClams)
        {
            if (preferClamsAdjacentToAmulet)
            {
                var freeOrtho = FreeNeighbors(CenterRow, CenterCol, CellFree).Count();
                var keep = Math.Max(0, freeOrtho);
                var clamCandidates = working
                    .Where(p => !usedPieceIds.Contains(p.Id) && IsClamChart(p))
                    .OrderByDescending(ClamScore)
                    .ThenByDescending(p => p.LocalModifier + p.GlobalModifier)
                    .Select(p => p.Id)
                    .ToList();
                foreach (var id in clamCandidates.Skip(keep))
                {
                    if (!TrySavePiece(working, id, force: true))
                        break;
                    savedClam++;
                }
            }
            else
            {
                savedClam += RemoveUnused(working, usedPieceIds, IsClamChart, ClamScore, force: true);
            }
        }

        var centerTakenByCenterOnly = locks.Any(lp =>
            lp.Row == CenterRow &&
            lp.Col == CenterCol &&
            pieces.FirstOrDefault(p => p.Id == lp.PieceId) is { } locked &&
            IsCenterOnlyUniqueChart(locked));
        var amulet2Waiting = working.Any(p =>
            !usedPieceIds.Contains(p.Id) && IsUniqueAmulet2Chart(p));
        var keepBeltRing = CellFree(CenterRow, CenterCol) && !centerTakenByCenterOnly && !amulet2Waiting
            ? 1
            : 0;
        var savedUniqueBelt = 0;
        var savedUniqueRing = 0;
        if (reserveCharts && options.CenterSpecialty)
        {
            foreach (var piece in working
                         .Where(p => !usedPieceIds.Contains(p.Id) &&
                                     (IsUniqueBeltChart(p) || IsUniqueRingChart(p)))
                         .OrderByDescending(CenterOnlyUniqueScore)
                         .ThenByDescending(p => p.LocalModifier + p.GlobalModifier)
                         .Skip(keepBeltRing)
                         .ToList())
            {
                if (!TrySavePiece(working, piece.Id, force: true))
                    break;
                if (IsUniqueBeltChart(piece))
                    savedUniqueBelt++;
                else
                    savedUniqueRing++;
            }
        }

        var activeStrategies = new List<string>();
        if (options.RareMonstersDrop)
        {
            if (divineCenters.Count > 0)
                activeStrategies.Add("Divine");
            if (exaltedCenters.Count > 0)
                activeStrategies.Add("Exalted");
            if (annulCenters.Count > 0)
                activeStrategies.Add("Annul");
            if (ancientCenters.Count > 0)
                activeStrategies.Add("Ancient");
        }
        if (pelagicLocked)
            activeStrategies.Add("Pelagic");
        if (amuletCrossLocked)
            activeStrategies.Add("Amulet Hub");
        else if (preferClamsAdjacentToAmulet)
            activeStrategies.Add("Amulet Soft");
        else if (amuletCenterLocked)
            activeStrategies.Add("Amulet");
        if (noConsumeActive)
            activeStrategies.Add("No-consume");
        if (brineKingSynergyActive)
            activeStrategies.Add("Brine King + Rare Monsters");
        if (seaPillarsStrongboxEngineActive)
            activeStrategies.Add("Sea Pillars + Strongbox Rare Engine");
        if (operativeFleetActive)
            activeStrategies.Add("9 Operative Strongboxes: Voyage dedicada");
        if (divinerFleetActive)
            activeStrategies.Add("9 Diviner Strongboxes: Voyage dedicada");
        if (messageStrategyActive)
            activeStrategies.Add("Messages in a Bottle: foco único");
        if (sulphurStrategyActive)
            activeStrategies.Add("Dead Man's Sulphur: foco único");
        if (groundLootStrategyActive)
            activeStrategies.Add("Ground loot conversion: combo completo");

        return new Result(
            working, locks,
            savedPelagic, savedFarm, savedStrongbox, savedStarfish, savedRareVoyage,
            savedAdjacentRare, savedOperative, savedDiviner, savedLostMessage, savedSulphur, savedKishara,
            savedNoEquipment, savedFractured, savedGoldenLanterns, savedPantheon,
            savedSoulEater, savedRareFracture, savedRarePossessed,
            savedClam, savedUniqueAmulet,
            savedUniqueBelt, savedUniqueRing, savedBrineKing, savedSeaPillars,
            AmuletClamHubActive: amuletCrossLocked,
            PreferClamsAdjacentToAmulet: preferClamsAdjacentToAmulet,
            NoConsumeActive: noConsumeActive,
            BrineKingSynergyActive: brineKingSynergyActive,
            ActiveStrategies: activeStrategies);
    }

    private static bool BoardHasStrongTreasureAnchors(IReadOnlyList<BorderEffect>[,] tileBorders)
    {
        var treasureT1 = 0;
        var treasureT2 = 0;
        foreach (var (row, col) in EnumerateCells())
        {
            foreach (var b in BordersAt(tileBorders, row, col))
            {
                if (b.Name.Equals(TreasureAnchors1, StringComparison.OrdinalIgnoreCase))
                    treasureT1++;
                else if (b.Name.Equals(TreasureAnchors2, StringComparison.OrdinalIgnoreCase))
                    treasureT2++;
            }
        }

        return IsStrongTreasureAnchorsCounts(treasureT1, treasureT2);
    }

    public static int ClamHubCountForAmulet(MapPiece amulet2)
    {
        var connections = amulet2.BaseConnections.CountConnections();
        if (connections <= 0)
            return 0;
        if (connections <= 2)
            return 2;
        return MaxSavedClamsForAmulet;
    }

    private static bool TryLockUniqueAmulet2Center(
        List<MapPiece> working,
        HashSet<int> usedPieceIds,
        Action<int, int, MapPiece, int?> lockCell)
    {
        var amulet2 = TakeBest(working, usedPieceIds, IsUniqueAmulet2Chart, UniqueAmuletScore);
        if (amulet2 == null)
            return false;
        lockCell(CenterRow, CenterCol, amulet2, null);
        return true;
    }

    private static bool TryLockAmuletClamHub(
        List<MapPiece> working,
        HashSet<int> usedPieceIds,
        Func<int, int, bool> cellFree,
        Action<int, int, MapPiece, int?> lockCell)
    {
        var amulet2 = TakeBest(working, usedPieceIds, IsUniqueAmulet2Chart, UniqueAmuletScore);
        if (amulet2 == null)
            return false;

        var clamCount = ClamHubCountForAmulet(amulet2);
        if (clamCount <= 0)
            return false;

        var freeOrtho = FreeNeighbors(CenterRow, CenterCol, cellFree).ToList();
        if (freeOrtho.Count < clamCount)
            return false;

        var clamSlots = freeOrtho
            .OrderBy(c => c.Row == CenterRow - 1 && c.Col == CenterCol ? 1 : 0)
            .Take(clamCount)
            .ToList();

        var clams = working
            .Where(p => !usedPieceIds.Contains(p.Id) && IsClamChart(p))
            .OrderByDescending(ClamScore)
            .ThenByDescending(p => p.LocalModifier + p.GlobalModifier)
            .Take(clamCount)
            .ToList();
        if (clams.Count < clamCount)
            return false;

        lockCell(CenterRow, CenterCol, amulet2, null);
        for (var i = 0; i < clamCount; i++)
            lockCell(clamSlots[i].Row, clamSlots[i].Col, clams[i], null);
        return true;
    }

    public static bool IsClamChart(MapPiece piece) =>
        piece.Name.Contains(ClamRoomName, StringComparison.OrdinalIgnoreCase);

    public static bool IsAnchorfieldChart(MapPiece piece) =>
        piece.Name.Contains(AnchorfieldRoomName, StringComparison.OrdinalIgnoreCase);

    public static bool IsFarmChart(MapPiece piece) =>
        IsClamChart(piece) || IsAnchorfieldChart(piece);

    public static bool IsPelagic(MapPiece piece) =>
        piece.Name.Contains(PelagicRoomName, StringComparison.OrdinalIgnoreCase);

    public static bool IsKishara(MapPiece piece) =>
        piece.Name.Contains(KisharaRoomName, StringComparison.OrdinalIgnoreCase);

    public static bool IsBrineKingsDomain(MapPiece piece) =>
        piece.Name.Contains(BrineKingRoomName, StringComparison.OrdinalIgnoreCase);

    public static bool IsSeaPillars(MapPiece piece) =>
        piece?.Name?.Contains(SeaPillarsRoomName, StringComparison.OrdinalIgnoreCase) == true;

    public static bool IsNoEquipmentChart(MapPiece piece) =>
        piece.Modifiers.Any(m =>
            m.Name.Equals(VoyageNoEquipmentDrops, StringComparison.OrdinalIgnoreCase));

    public static bool IsSoulEaterChart(MapPiece piece) =>
        piece.Modifiers.Any(m =>
            m.Name.Equals(VoyageSoulEater, StringComparison.OrdinalIgnoreCase));

    public static bool IsRareFractureChart(MapPiece piece) =>
        piece.Modifiers.Any(m =>
            m.Name.Equals(VoyageRareFracture, StringComparison.OrdinalIgnoreCase));

    public static bool IsRarePossessedChart(MapPiece piece) =>
        piece.Modifiers.Any(m =>
            m.Name.Equals(VoyageMonstersPossessed, StringComparison.OrdinalIgnoreCase));

    public static bool IsFracturedChart(MapPiece piece) =>
        piece.Modifiers.Any(m => IsFamily(m.Name, AdjacentFracturedPrefix));

    public static bool IsGoldenLanternsChart(MapPiece piece) =>
        piece.Modifiers.Any(m => IsFamily(m.Name, AdjacentGoldenLanternsPrefix));

    public static bool IsPantheonChart(MapPiece piece) =>
        piece.Modifiers.Any(m => IsFamily(m.Name, AdjacentPantheonPrefix));

    public static bool IsAdjacentStrongboxesChart(MapPiece piece) =>
        piece.Modifiers.Any(m => IsFamily(m.Name, AdjacentStrongboxesPrefix));

    public static bool IsPremiumBoxChart(MapPiece piece) =>
        piece.Modifiers.Any(m =>
            IsFamily(m.Name, AdjacentDivinerBoxPrefix) ||
            IsFamily(m.Name, AdjacentArcanistBoxPrefix) ||
            IsFamily(m.Name, AdjacentOperativeBoxPrefix));

    public static bool IsStrongboxCountChart(MapPiece piece) =>
        IsAdjacentStrongboxesChart(piece) || IsPremiumBoxChart(piece);

    public static bool IsOperativeBoxChart(MapPiece piece) =>
        piece.Modifiers.Any(m => IsFamily(m.Name, AdjacentOperativeBoxPrefix));

    public static bool IsDivinerBoxChart(MapPiece piece) =>
        piece.Modifiers.Any(m => IsFamily(m.Name, AdjacentDivinerBoxPrefix));

    public static bool IsStarfishChart(MapPiece piece) =>
        piece.Modifiers.Any(m => IsFamily(m.Name, AdjacentStarfishPrefix));

    public static bool IsAdjacentRareChart(MapPiece piece) =>
        piece.Modifiers.Any(m => IsFamily(m.Name, AdjacentIncreasedRarePrefix));

    public static bool IsAdjacentRareSaveChart(MapPiece piece) =>
        piece.Modifiers.Any(m =>
            IsFamily(m.Name, AdjacentIncreasedRarePrefix) &&
            TierFromFamily(m.Name, AdjacentIncreasedRarePrefix) >= 2);

    public static bool IsRareVoyageChart(MapPiece piece) =>
        piece.Modifiers.Any(m =>
            m.Name.Equals(VoyageIncreasedRareMonsters, StringComparison.OrdinalIgnoreCase));

    public static bool IsSulphurChart(MapPiece piece) =>
        piece.Modifiers.Any(m =>
            IsFamily(m.Name, VoyageResourceFoundPrefix) ||
            m.Tags.HasFlag(ModifierTag.Sulphur));

    public static int SulphurPercent(MapPiece piece) =>
        piece.Modifiers
            .Where(m => IsFamily(m.Name, VoyageResourceFoundPrefix) ||
                        m.Tags.HasFlag(ModifierTag.Sulphur))
            .Select(m => Math.Abs(m.Value1))
            .DefaultIfEmpty(0)
            .Max();

    public static bool IsHighValueSulphurChart(MapPiece piece, int minimumPercent = 25) =>
        IsSulphurChart(piece) && SulphurPercent(piece) >= minimumPercent;

    public static bool IsLostMessageChart(MapPiece piece) =>
        piece.Modifiers.Any(m => IsFamily(m.Name, AdjacentLostMessagePrefix));

    public static bool IsUniqueAmuletChart(MapPiece piece) =>
        piece.Modifiers.Any(m => IsFamily(m.Name, AdjacentUniqueAmuletPrefix));

    public static bool IsUniqueAmulet1Chart(MapPiece piece) =>
        piece.Modifiers.Any(m =>
            IsFamily(m.Name, AdjacentUniqueAmuletPrefix) &&
            TierFromFamily(m.Name, AdjacentUniqueAmuletPrefix) == 1);

    public static bool IsUniqueAmulet2Chart(MapPiece piece) =>
        piece.Modifiers.Any(m =>
            IsFamily(m.Name, AdjacentUniqueAmuletPrefix) &&
            TierFromFamily(m.Name, AdjacentUniqueAmuletPrefix) == 2);

    public static bool IsUniqueBeltChart(MapPiece piece) =>
        piece.Modifiers.Any(m => IsFamily(m.Name, AdjacentUniqueBeltPrefix));

    public static bool IsUniqueRingChart(MapPiece piece) =>
        piece.Modifiers.Any(m => IsFamily(m.Name, AdjacentUniqueRingPrefix));

    public static bool IsCenterOnlyUniqueChart(MapPiece piece) =>
        IsUniqueAmulet2Chart(piece) || IsUniqueBeltChart(piece) || IsUniqueRingChart(piece);

    public static bool IsOrbRareGlobalChart(MapPiece piece) =>
        IsRareVoyageChart(piece);

    public static bool IsOrbRareComboChart(MapPiece piece) =>
        IsAdjacentRareChart(piece) || IsRareVoyageChart(piece);

    public static bool IsSpecialtyComboModifier(string rawName)
    {
        if (string.IsNullOrEmpty(rawName))
            return false;
        return IsFamily(rawName, AdjacentStrongboxesPrefix)
               || IsFamily(rawName, AdjacentDivinerBoxPrefix)
               || IsFamily(rawName, AdjacentArcanistBoxPrefix)
               || IsFamily(rawName, AdjacentOperativeBoxPrefix)
               || IsFamily(rawName, AdjacentStarfishPrefix)
               || IsFamily(rawName, AdjacentLostMessagePrefix)
               || (IsFamily(rawName, AdjacentUniqueAmuletPrefix) &&
                   TierFromFamily(rawName, AdjacentUniqueAmuletPrefix) == 2)
               || IsFamily(rawName, AdjacentUniqueBeltPrefix)
               || IsFamily(rawName, AdjacentUniqueRingPrefix);
    }

    public static bool IsIncreasedRareStrategyModifier(string rawName)
    {
        if (string.IsNullOrEmpty(rawName))
            return false;
        if (rawName.Equals(VoyageIncreasedRareMonsters, StringComparison.OrdinalIgnoreCase))
            return true;
        return IsFamily(rawName, AdjacentIncreasedRarePrefix) &&
               TierFromFamily(rawName, AdjacentIncreasedRarePrefix) >= 2;
    }

    public static bool HasStrategyOrb(IEnumerable<string> borderNames)
    {
        if (borderNames == null)
            return false;
        foreach (var name in borderNames)
        {
            if (string.IsNullOrEmpty(name))
                continue;
            if (name.Equals(RareDivine, StringComparison.OrdinalIgnoreCase) ||
                name.Equals(RareExalted, StringComparison.OrdinalIgnoreCase) ||
                name.Equals(RareAnnul, StringComparison.OrdinalIgnoreCase) ||
                name.Equals(RareAncient, StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }

    public static HashSet<int> SelectInventorySpecialtyIndices(
        IReadOnlyList<string> roomNames,
        IReadOnlyList<IReadOnlyList<(string RawName, int Value1)>> modsPerChart)
    {
        var marked = new HashSet<int>();
        var starfish = new List<(int Index, int Value1)>();
        var adjRareT2 = new List<(int Index, double Score)>();
        var voyageRare = new List<(int Index, double Score)>();
        var boxes = new List<(int Index, int Value1)>();
        var count = Math.Min(roomNames.Count, modsPerChart.Count);

        for (var i = 0; i < count; i++)
        {
            if (TrySpecialtyRoomLabel(roomNames[i], out _))
                marked.Add(i);

            var mods = modsPerChart[i];
            if (mods == null || mods.Count == 0)
                continue;

            var always = false;
            foreach (var (raw, _) in mods)
            {
                if (string.IsNullOrEmpty(raw))
                    continue;
                if (IsFamily(raw, AdjacentLostMessagePrefix) ||
                    IsFamily(raw, AdjacentOperativeBoxPrefix) ||
                    (IsFamily(raw, AdjacentUniqueAmuletPrefix) &&
                     TierFromFamily(raw, AdjacentUniqueAmuletPrefix) == 2) ||
                    IsFamily(raw, AdjacentUniqueBeltPrefix) ||
                    IsFamily(raw, AdjacentUniqueRingPrefix))
                {
                    always = true;
                    break;
                }
            }

            if (always)
                marked.Add(i);

            var starfishV = MaxFamilyValue1(mods, AdjacentStarfishPrefix);
            if (starfishV > 0)
                starfish.Add((i, starfishV));

            var adjRareTier = 0;
            foreach (var (raw, _) in mods)
            {
                if (IsFamily(raw, AdjacentIncreasedRarePrefix))
                    adjRareTier = Math.Max(adjRareTier, TierFromFamily(raw, AdjacentIncreasedRarePrefix));
            }

            if (adjRareTier >= 2)
                adjRareT2.Add((i, adjRareTier * 1_000.0));

            foreach (var (raw, _) in mods)
            {
                if (raw.Equals(VoyageIncreasedRareMonsters, StringComparison.OrdinalIgnoreCase))
                {
                    voyageRare.Add((i, 1));
                    break;
                }
            }

            var boxV = BoxPoolValue1(mods);
            if (boxV > 0)
                boxes.Add((i, boxV));
        }

        var supportMarked = 0;
        foreach (var (index, _) in starfish
                     .OrderByDescending(x => x.Value1)
                     .Take(MaxSavedStarfish))
        {
            marked.Add(index);
            supportMarked++;
        }

        var rareSlots = Math.Max(0, MaxSavedStarfish - supportMarked);
        if (rareSlots > 0)
        {
            foreach (var (index, _) in adjRareT2
                         .OrderByDescending(x => x.Score)
                         .Take(rareSlots))
                marked.Add(index);
        }

        foreach (var (index, _) in voyageRare.Take(MaxSavedRareVoyage))
            marked.Add(index);

        foreach (var (index, _) in boxes
                     .OrderByDescending(x => x.Value1)
                     .Take(MaxSavedBoxes))
            marked.Add(index);

        return marked;
    }

    public static bool TrySpecialtyRoomLabel(string roomName, out string label)
    {
        label = null;
        if (string.IsNullOrEmpty(roomName))
            return false;
        if (roomName.Contains(PelagicRoomName, StringComparison.OrdinalIgnoreCase))
        {
            label = "Pelagic";
            return true;
        }

        if (roomName.Contains(BrineKingRoomName, StringComparison.OrdinalIgnoreCase))
        {
            label = "Brine King";
            return true;
        }

        if (roomName.Contains(SeaPillarsRoomName, StringComparison.OrdinalIgnoreCase))
        {
            label = "Sea Pillars";
            return true;
        }

        if (roomName.Contains(KisharaRoomName, StringComparison.OrdinalIgnoreCase))
        {
            label = "Kishara";
            return true;
        }

        if (roomName.Contains(AnchorfieldRoomName, StringComparison.OrdinalIgnoreCase))
        {
            label = "Anchorfield";
            return true;
        }

        if (roomName.Contains(ClamRoomName, StringComparison.OrdinalIgnoreCase))
        {
            label = "Clam Shelf";
            return true;
        }

        return false;
    }

    public static bool IsStrategyBorder(string rawName)
    {
        if (string.IsNullOrEmpty(rawName))
            return false;
        return rawName.Equals(RareDivine, StringComparison.OrdinalIgnoreCase)
               || rawName.Equals(RareExalted, StringComparison.OrdinalIgnoreCase)
               || rawName.Equals(RareAnnul, StringComparison.OrdinalIgnoreCase)
               || rawName.Equals(RareAncient, StringComparison.OrdinalIgnoreCase);
    }

    public static bool IsTreasureAnchorsBorder(string rawName)
    {
        if (string.IsNullOrEmpty(rawName))
            return false;
        return rawName.Equals(TreasureAnchors1, StringComparison.OrdinalIgnoreCase)
               || rawName.Equals(TreasureAnchors2, StringComparison.OrdinalIgnoreCase);
    }

    private static double FarmPriority(MapPiece p) =>
        p.LocalModifier + p.GlobalModifier;

    private static double SoulEaterScore(MapPiece p) =>
        p.Modifiers.Where(m => m.Name.Equals(VoyageSoulEater, StringComparison.OrdinalIgnoreCase))
            .Sum(m => m.Weight)
        + p.LocalModifier + p.GlobalModifier;

    private static double ClamScore(MapPiece p) =>
        p.LocalModifier + p.GlobalModifier;

    private static double BoxValue1Score(MapPiece p) =>
        Math.Max(
            MaxFamilyValue1Score(p, AdjacentStrongboxesPrefix),
            Math.Max(
                MaxFamilyValue1Score(p, AdjacentDivinerBoxPrefix),
                Math.Max(
                    MaxFamilyValue1Score(p, AdjacentArcanistBoxPrefix),
                    MaxFamilyValue1Score(p, AdjacentOperativeBoxPrefix))));

    private static double OperativeBoxScore(MapPiece p) =>
        MaxFamilyValue1Score(p, AdjacentOperativeBoxPrefix);

    private static double DivinerBoxScore(MapPiece p) =>
        MaxFamilyValue1Score(p, AdjacentDivinerBoxPrefix);

    private static double StarfishScore(MapPiece p) =>
        MaxFamilyValue1Score(p, AdjacentStarfishPrefix);

    private static double SeaPillarsScore(MapPiece p) =>
        10_000 + p.Modifiers
            .Where(m => m.Tags.HasFlag(ModifierTag.RareMonsters) ||
                        m.Tags.HasFlag(ModifierTag.Monsters) ||
                        m.Name.Contains("PackSize", StringComparison.OrdinalIgnoreCase) ||
                        m.Name.Contains("Starfish", StringComparison.OrdinalIgnoreCase))
            .Sum(m => m.Weight + Math.Abs(m.Value1) * 0.5)
        + p.LocalModifier + p.GlobalModifier;

    private static double BrineKingScore(MapPiece p) =>
        p.Modifiers.Where(m => m.Tags.HasFlag(ModifierTag.RareMonsters) ||
                               m.Tags.HasFlag(ModifierTag.Monsters))
            .Sum(m => m.Weight + Math.Abs(m.Value1) * 0.25)
        + p.LocalModifier + p.GlobalModifier;

    private static double AdjacentRareScore(MapPiece p) =>
        MaxFamilyTierScore(p, AdjacentIncreasedRarePrefix);

    private static double RareVoyageScore(MapPiece p) =>
        p.Modifiers.Where(m => m.Name.Equals(VoyageIncreasedRareMonsters, StringComparison.OrdinalIgnoreCase))
            .Sum(m => m.Weight);

    private static double SulphurChartScore(MapPiece p) =>
        p.Modifiers.Where(m => IsFamily(m.Name, VoyageResourceFoundPrefix) ||
                               m.Tags.HasFlag(ModifierTag.Sulphur))
            .Sum(m => m.Weight + Math.Abs(m.Value1));

    private static double LostMessageScore(MapPiece p) =>
        MaxFamilyTierScore(p, AdjacentLostMessagePrefix);

    private static double UniqueAmuletScore(MapPiece p) =>
        MaxFamilyTierScore(p, AdjacentUniqueAmuletPrefix);

    private static double UniqueBeltScore(MapPiece p) =>
        MaxFamilyTierScore(p, AdjacentUniqueBeltPrefix);

    private static double UniqueRingScore(MapPiece p) =>
        MaxFamilyTierScore(p, AdjacentUniqueRingPrefix);

    private static double CenterOnlyUniqueScore(MapPiece p)
    {
        if (IsUniqueAmulet2Chart(p))
            return 3_000 + UniqueAmuletScore(p);
        if (IsUniqueBeltChart(p))
            return 2_000 + UniqueBeltScore(p);
        if (IsUniqueRingChart(p))
            return 1_000 + UniqueRingScore(p);
        return 0;
    }

    private static double OrbRareComboScore(MapPiece p)
    {
        double score = 0;
        if (IsAdjacentRareChart(p))
            score = Math.Max(score, AdjacentRareScore(p) + 2_000);
        if (IsRareVoyageChart(p))
            score = Math.Max(score, RareVoyageScore(p));
        return score;
    }

    private static double MaxFamilyTierScore(MapPiece p, string prefix)
    {
        double best = 0;
        foreach (var m in p.Modifiers)
        {
            if (!IsFamily(m.Name, prefix))
                continue;
            best = Math.Max(best, TierFromFamily(m.Name, prefix) * 1_000 + m.Weight);
        }

        return best;
    }

    private static double MaxFamilyValue1Score(MapPiece p, string prefix)
    {
        double best = 0;
        foreach (var m in p.Modifiers)
        {
            if (!IsFamily(m.Name, prefix))
                continue;
            best = Math.Max(best, m.Value1 * 1_000_000.0 + m.Weight);
        }

        return best;
    }

    private static int MaxFamilyValue1(IEnumerable<(string Name, int Value1)> mods, string prefix)
    {
        var best = 0;
        foreach (var m in mods)
        {
            if (!IsFamily(m.Name, prefix))
                continue;
            if (m.Value1 > best)
                best = m.Value1;
        }

        return best;
    }

    private static int BoxPoolValue1(IEnumerable<(string Name, int Value1)> mods) =>
        Math.Max(
            MaxFamilyValue1(mods, AdjacentStrongboxesPrefix),
            Math.Max(
                MaxFamilyValue1(mods, AdjacentDivinerBoxPrefix),
                Math.Max(
                    MaxFamilyValue1(mods, AdjacentArcanistBoxPrefix),
                    MaxFamilyValue1(mods, AdjacentOperativeBoxPrefix))));

    private static bool IsFamily(string rawName, string prefix) =>
        !string.IsNullOrEmpty(rawName) &&
        rawName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase);

    private static int TierFromFamily(string rawName, string prefix)
    {
        if (!IsFamily(rawName, prefix) || rawName.Length <= prefix.Length)
            return 0;
        return int.TryParse(rawName.AsSpan(prefix.Length), out var tier) ? tier : 0;
    }

    public static bool IsStrongNoConsume(IReadOnlyList<BorderEffect> borders)
    {
        var t1 = 0;
        var t2 = 0;
        foreach (var b in borders)
        {
            if (b.Name.Equals(NotConsume1, StringComparison.OrdinalIgnoreCase))
                t1++;
            else if (b.Name.Equals(NotConsume2, StringComparison.OrdinalIgnoreCase))
                t2++;
        }

        return t2 >= 1 || t1 >= 2;
    }

    public static bool IsStrongTreasureAnchorsCounts(int t1, int t2) =>
        (t2 >= 1 && t1 >= 2) || t1 >= 3 || t2 >= 2;

    public static bool IsStrongTreasureAnchors(IEnumerable<string> borderNames)
    {
        var t1 = 0;
        var t2 = 0;
        if (borderNames == null)
            return false;

        foreach (var name in borderNames)
        {
            if (string.IsNullOrEmpty(name))
                continue;
            if (name.Equals(TreasureAnchors1, StringComparison.OrdinalIgnoreCase))
                t1++;
            else if (name.Equals(TreasureAnchors2, StringComparison.OrdinalIgnoreCase))
                t2++;
        }

        return IsStrongTreasureAnchorsCounts(t1, t2);
    }

    public static bool IsStrongTreasureAnchors(IReadOnlyList<BorderEffect> borders) =>
        IsStrongTreasureAnchors(borders?.Select(b => b.Name));

    public static int OrbPriority(IReadOnlyList<BorderEffect> borders)
    {
        var best = 0;
        foreach (var b in borders)
        {
            if (b.Name.Equals(RareDivine, StringComparison.OrdinalIgnoreCase))
                best = Math.Max(best, 4);
            else if (b.Name.Equals(RareExalted, StringComparison.OrdinalIgnoreCase))
                best = Math.Max(best, 3);
            else if (b.Name.Equals(RareAnnul, StringComparison.OrdinalIgnoreCase))
                best = Math.Max(best, 2);
            else if (b.Name.Equals(RareAncient, StringComparison.OrdinalIgnoreCase))
                best = Math.Max(best, 1);
        }

        return best;
    }

    public static bool HasRareMonsterBoostBorder(IReadOnlyList<BorderEffect> borders) =>
        borders?.Any(b => b.Name?.Contains("RareMonster", StringComparison.OrdinalIgnoreCase) == true ||
                          b.Name?.Contains("PackSize", StringComparison.OrdinalIgnoreCase) == true ||
                          b.Tags.HasFlag(ModifierTag.RareMonsters)) == true;

    private static bool HasChartEffectBorder(IReadOnlyList<BorderEffect> borders) =>
        borders?.Any(b => b.AffectsPlacedChart ||
                          b.Name?.Contains("ChartEffect", StringComparison.OrdinalIgnoreCase) == true) == true;

    public static bool HasValuableRareCurrencyBorder(IReadOnlyList<BorderEffect>[,] tileBorders) =>
        EnumerateCells().Any(c => BordersAt(tileBorders, c.Row, c.Col).Any(b =>
            b.Name.Equals(RareDivine, StringComparison.OrdinalIgnoreCase) ||
            b.Name.Equals(RareExalted, StringComparison.OrdinalIgnoreCase) ||
            b.Name.Equals(RareAnnul, StringComparison.OrdinalIgnoreCase) ||
            b.Name.Equals(RareAncient, StringComparison.OrdinalIgnoreCase)));

    public static bool HasStrongboxSpendBorder(IReadOnlyList<BorderEffect>[,] tileBorders) =>
        EnumerateCells().Any(c => BordersAt(tileBorders, c.Row, c.Col).Any(b =>
            b.Name.Contains("MoreScarabs", StringComparison.OrdinalIgnoreCase) ||
            b.Name.Contains("MoreCurrency", StringComparison.OrdinalIgnoreCase) ||
            b.Name.Contains("RareMonster", StringComparison.OrdinalIgnoreCase)));

    private static int InGridDegree(int row, int col) =>
        4 - (row == 0 || row == 2 ? 1 : 0) - (col == 0 || col == 2 ? 1 : 0);

    private static MapPiece TakeBest(
        List<MapPiece> working,
        HashSet<int> used,
        Func<MapPiece, bool> pred,
        Func<MapPiece, double> score)
    {
        return working
            .Where(p => !used.Contains(p.Id) && pred(p))
            .OrderByDescending(score)
            .ThenByDescending(p => p.LocalModifier + p.GlobalModifier)
            .FirstOrDefault();
    }

    private static bool TrySavePiece(List<MapPiece> working, int pieceId, bool force = false)
    {
        // Hard economic reservations may intentionally leave fewer than nine filler charts. In
        // that case the UI reports that the premium package is still incomplete instead of
        // silently spending it in a low-value voyage.
        if (!force && working.Count <= 9)
            return false;
        return working.RemoveAll(p => p.Id == pieceId) > 0;
    }

    private static int RemoveUnused(
        List<MapPiece> working,
        HashSet<int> used,
        Func<MapPiece, bool> pred,
        Func<MapPiece, double> score = null,
        int? maxSave = null,
        bool force = false)
    {
        IEnumerable<MapPiece> candidates = working.Where(p => !used.Contains(p.Id) && pred(p));
        if (score != null)
        {
            candidates = candidates
                .OrderByDescending(score)
                .ThenByDescending(p => p.LocalModifier + p.GlobalModifier);
        }

        var drop = candidates.Select(p => p.Id).ToList();
        if (maxSave is int cap && drop.Count > cap)
            drop = drop.Take(cap).ToList();

        var removed = 0;
        foreach (var id in drop)
        {
            if (!TrySavePiece(working, id, force))
                break;
            removed++;
        }

        return removed;
    }

    private static IEnumerable<(int Row, int Col)> FreeNeighbors(
        int row, int col, Func<int, int, bool> cellFree)
    {
        foreach (var (dr, dc) in Ortho)
        {
            var nr = row + dr;
            var nc = col + dc;
            if (nr is < 0 or > 2 || nc is < 0 or > 2) continue;
            if (cellFree(nr, nc))
                yield return (nr, nc);
        }
    }

    private static IReadOnlyList<BorderEffect> BordersAt(
        IReadOnlyList<BorderEffect>[,] tileBorders, int row, int col) =>
        tileBorders?[row, col] ?? [];

    private static IEnumerable<(int Row, int Col)> EnumerateCells()
    {
        for (var r = 0; r < 3; r++)
        for (var c = 0; c < 3; c++)
            yield return (r, c);
    }
}
