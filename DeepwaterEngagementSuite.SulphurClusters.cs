using System;
using System.Collections.Generic;
using System.Linq;
using ExileCore.Shared.Enums;
using ExileCore.Shared.Helpers;
using SharpDX;
using Vector2 = System.Numerics.Vector2;

namespace DeepwaterEngagementSuite;

public partial class DeepwaterEngagementSuite
{
    private sealed class SulphurVisualCluster
    {
        public readonly List<(EntityCacheItem Entity, IconPickerIndex Type)> Members = [];
        public Vector2 Center;
        public float VisualWeight;

        public void Add(EntityCacheItem entity, IconPickerIndex type)
        {
            Members.Add((entity, type));
            Recalculate();
        }

        private void Recalculate()
        {
            VisualWeight = Members.Sum(x => SulphurVisualWeight(x.Type));
            if (VisualWeight <= 0)
                return;

            Center = Members.Aggregate(Vector2.Zero, (sum, x) =>
                sum + x.Entity.GridPos * SulphurVisualWeight(x.Type)) / VisualWeight;
        }
    }

    private static bool IsSulphurCrystalType(IconPickerIndex type) => type is
        IconPickerIndex.DeadMansSulphurSmall or
        IconPickerIndex.DeadMansSulphurBase or
        IconPickerIndex.DeadMansSulphurLarge or
        IconPickerIndex.DeadMansSulphurHuge;

    private static float SulphurVisualWeight(IconPickerIndex type) => type switch
    {
        IconPickerIndex.DeadMansSulphurSmall => 1f,
        IconPickerIndex.DeadMansSulphurBase => 1.5f,
        IconPickerIndex.DeadMansSulphurLarge => 2.5f,
        IconPickerIndex.DeadMansSulphurHuge => 4f,
        _ => 0f,
    };

    private void DrawCompactSulphurClusters()
    {
        var settings = Settings.IconSettings;
        if (!settings.CompactSulphurClusters.Value ||
            !settings.IsIconEnabled(IconPickerIndex.DeadMansSulphurBase))
        {
            return;
        }

        var members = _cachedEntities.Values
            .Where(x => !x.IsOpened)
            .Select(x => (Entity: x, Type: GetChestType(x.Path)))
            .Where(x => IsSulphurCrystalType(x.Type))
            .Where(x => !settings.HideCoveredSulphurClusters.Value || !IsEntityInBubble(x.Entity.GridPos))
            .ToList();

        if (members.Count == 0)
            return;

        var clusters = BuildSulphurVisualClusters(members, settings.SulphurClusterRadius.Value);
        var mapSettings = settings.IconMapping.GetValueOrDefault(
            IconPickerIndex.DeadMansSulphurBase, new IconDisplaySettings());
        var icon = mapSettings.Icon ?? DeepwaterEngagementSuiteSettings.DefaultDeadmansSulphurBaseIcon;
        var opacity = (byte)Math.Clamp(
            (int)MathF.Round(settings.SulphurClusterOpacityPercent.Value * 2.55f), 1, 255);
        var tint = mapSettings.Tint ?? new Color(80, 255, 80);
        var transparentTint = new Color(tint.R, tint.G, tint.B, opacity);
        var maximumScale = settings.SulphurClusterMaxSizePercent.Value / 100f;

        foreach (var cluster in clusters)
        {
            var scale = Math.Clamp(
                0.55f + MathF.Sqrt(cluster.VisualWeight) * 0.22f,
                0.65f,
                maximumScale);

            if (_largeMapOpen && mapSettings.ShowOnMap)
            {
                DrawUnframedIcon(
                    icon,
                    transparentTint,
                    Graphics.GridToMap(cluster.Center, cluster.Center),
                    settings.MapIconSize.Value * scale);
            }

            if (!_largeMapOpen && mapSettings.ShowInWorld)
            {
                DrawUnframedIcon(
                    icon,
                    transparentTint,
                    Camera.WorldToScreen(ExpandWithTerrainHeight(cluster.Center)),
                    settings.WorldIconSize.Value * scale);
            }
        }
    }

    private static List<SulphurVisualCluster> BuildSulphurVisualClusters(
        List<(EntityCacheItem Entity, IconPickerIndex Type)> members,
        float clusterRadius)
    {
        var remaining = new HashSet<int>(Enumerable.Range(0, members.Count));
        var clusters = new List<SulphurVisualCluster>();

        while (remaining.Count > 0)
        {
            var seed = remaining.First();
            remaining.Remove(seed);
            var queue = new Queue<int>();
            queue.Enqueue(seed);
            var cluster = new SulphurVisualCluster();

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                var member = members[current];
                cluster.Add(member.Entity, member.Type);

                foreach (var candidate in remaining
                             .Where(index => Vector2.Distance(
                                 members[index].Entity.GridPos,
                                 member.Entity.GridPos) <= clusterRadius)
                             .ToList())
                {
                    remaining.Remove(candidate);
                    queue.Enqueue(candidate);
                }
            }

            clusters.Add(cluster);
        }

        return clusters;
    }

    private void DrawUnframedIcon(
        MapIconsIndex icon,
        Color color,
        Vector2 displayPosition,
        float iconSize)
    {
        var rect = new RectangleF(displayPosition.X, displayPosition.Y, 0, 0);
        var halfSize = iconSize / 2f;
        rect.Inflate(halfSize, halfSize);
        Graphics.DrawImage(TextureName, rect, SpriteHelper.GetUV(icon), color);
    }
}
