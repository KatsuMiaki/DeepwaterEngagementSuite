using ExileCore;
using ExileCore.PoEMemory.MemoryObjects;
using ExileCore.Shared.Enums;
using System.Collections.Generic;

namespace DeepwaterEngagementSuite;

public partial class DeepwaterEngagementSuite
{
    private bool _voyageAreaLatched;
    private bool _sleepingCollectionChangedByPlugin;
    private bool _sleepingCollectionValueBeforeVoyage;

    private bool SleepingPlannerActive
    {
        get
        {
            // Voyages no longer run the Bubble Planner. Avoid walking the much larger sleeping
            // entity collection every Tick; awake entities still feed icons and Trail normally.
            if (IsVoyageArea ||
                !Settings.PlannerSettings.IncludeSleepingEntities.Value ||
                GameController?.SleepingEntityListWrapper == null)
                return false;

            try
            {
                return GameController.Settings.CoreSettings.DebugSettings.CollectSleepingEntities.Value;
            }
            catch
            {
                return false;
            }
        }
    }

    /// <summary>
    /// MaxLanternCount can be reset to zero when a Voyage finishes, before the
    /// player leaves the area. Keep Voyage mode latched so the Bubble Planner and
    /// sleeping-entity traversal cannot restart during that end-of-area refresh.
    /// </summary>
    private void UpdateVoyageEntitySafetyState()
    {
        var maxLanterns = Handler?.MaxLanternCount ?? 0;
        var voyageThreshold = Settings.PlannerSettings.VoyageLanternThreshold.Value;

        if (maxLanterns >= voyageThreshold)
        {
            _voyageAreaLatched = true;
        }
        else if (maxLanterns > 0 && _voyageAreaLatched)
        {
            // A positive, below-threshold count means a normal Deepwater chart has
            // loaded. Zero is deliberately ignored because Voyages use it while
            // finishing and refreshing their entity tree.
            _voyageAreaLatched = false;
            RestoreSleepingEntityCollection();
        }

        if (_voyageAreaLatched &&
            Settings.PlannerSettings.DisableSleepingEntityCollectionInVoyages.Value)
        {
            DisableSleepingEntityCollectionForVoyage();
        }
        else if (!Settings.PlannerSettings.DisableSleepingEntityCollectionInVoyages.Value)
        {
            RestoreSleepingEntityCollection();
        }
    }

    private void DisableSleepingEntityCollectionForVoyage()
    {
        try
        {
            var collectSleepingEntities =
                GameController.Settings.CoreSettings.DebugSettings.CollectSleepingEntities;

            if (!collectSleepingEntities.Value)
                return;

            if (!_sleepingCollectionChangedByPlugin)
            {
                _sleepingCollectionValueBeforeVoyage = true;
                _sleepingCollectionChangedByPlugin = true;
                DebugWindow.LogMsg(
                    "Deepwater: Collect Sleeping Entities paused for this Voyage (10,000 entity guard).",
                    5);
            }

            collectSleepingEntities.Value = false;
        }
        catch
        {
            // Core settings can be unavailable briefly during process/area reload.
        }
    }

    private void RestoreSleepingEntityCollection()
    {
        if (!_sleepingCollectionChangedByPlugin)
            return;

        try
        {
            GameController.Settings.CoreSettings.DebugSettings
                .CollectSleepingEntities.Value = _sleepingCollectionValueBeforeVoyage;
            _sleepingCollectionChangedByPlugin = false;
            _sleepingCollectionValueBeforeVoyage = false;
        }
        catch
        {
            // Keep the ownership flag so OnUnload or the next normal chart can retry.
        }
    }

    private IEnumerable<Entity> GetPlannerSourceEntities(params EntityType[] types)
    {
        var seen = new HashSet<uint>();
        var awakeByType = GameController?.EntityListWrapper?.ValidEntitiesByType;
        if (awakeByType != null)
        {
            foreach (var type in types)
            {
                if (!awakeByType.TryGetValue(type, out var entities) || entities == null)
                    continue;

                foreach (var entity in entities)
                {
                    if (entity == null || string.IsNullOrEmpty(entity.Path))
                        continue;

                    seen.Add(entity.Id);
                    yield return entity;
                }
            }
        }

        if (!SleepingPlannerActive)
            yield break;

        var sleepingByType = GameController.SleepingEntityListWrapper.ValidEntitiesByType;
        foreach (var type in types)
        {
            if (!sleepingByType.TryGetValue(type, out var entities) || entities == null)
                continue;

            foreach (var entity in entities)
            {
                if (entity == null || string.IsNullOrEmpty(entity.Path) || !seen.Add(entity.Id))
                    continue;

                yield return entity;
            }
        }
    }
}
