using StatisticsAnalysisTool.Dungeon.Models;
using StatisticsAnalysisTool.Enumerations;
using System.Collections.Generic;
using System.Linq;

namespace StatisticsAnalysisTool.Dungeon;

internal static class DungeonRunPresentationService
{
    public static IReadOnlyList<DungeonRunMetric> BuildMetrics(DungeonBaseFragment dungeon)
    {
        List<DungeonRunMetric> metrics =
        [
            CreateMetric(DungeonBaseFragment.TranslationFame, "/Resources/fame.png", dungeon.Fame, dungeon.FamePerHour),
            CreateMetric(DungeonBaseFragment.TranslationReSpec, "/Resources/respec.png", dungeon.ReSpec, dungeon.ReSpecPerHour),
            CreateMetric(DungeonBaseFragment.TranslationSilver, "/Resources/silver.png", dungeon.Silver, dungeon.SilverPerHour),
            CreateMetric(DungeonBaseFragment.TranslationTotalValue, "/Assets/static_chest.png", dungeon.TotalValue, GetValuePerHour(dungeon.TotalValue, dungeon.EffectiveRunTimeInSeconds))
        ];

        switch (dungeon)
        {
            case RandomDungeonFragment randomDungeon:
                AddMetric(metrics, DungeonBaseFragment.TranslationMight, "/Resources/might.png", randomDungeon.Might, randomDungeon.MightPerHour);
                AddMetric(metrics, DungeonBaseFragment.TranslationFavor, "/Resources/favor.png", randomDungeon.Favor, randomDungeon.FavorPerHour);
                AddMetric(
                    metrics,
                    DungeonBaseFragment.TranslationFactionCoins,
                    GetFactionCoinIconPath(randomDungeon.CityFaction),
                    randomDungeon.FactionCoins,
                    randomDungeon.FactionCoinsPerHour);
                AddMetric(
                    metrics,
                    DungeonBaseFragment.TranslationFactionStanding,
                    GetFactionStandingIconPath(randomDungeon.CityFaction),
                    randomDungeon.FactionStanding,
                    randomDungeon.FactionStandingPerHour);
                break;
            case MistsFragment mists:
                AddMetric(metrics, DungeonBaseFragment.TranslationMight, "/Resources/might.png", mists.Might, mists.MightPerHour);
                AddMetric(metrics, DungeonBaseFragment.TranslationFavor, "/Resources/favor.png", mists.Favor, mists.FavorPerHour);
                AddMetric(metrics, DungeonBaseFragment.TranslationBrecilianStanding, "/Assets/brecilien_standing_coin.png", mists.BrecilianStanding, mists.BrecilianStandingPerHour);
                break;
            case MistsDungeonFragment mistsDungeon:
                AddMetric(metrics, DungeonBaseFragment.TranslationMight, "/Resources/might.png", mistsDungeon.Might, mistsDungeon.MightPerHour);
                AddMetric(metrics, DungeonBaseFragment.TranslationFavor, "/Resources/favor.png", mistsDungeon.Favor, mistsDungeon.FavorPerHour);
                break;
            case HellGateFragment hellGate:
                AddMetric(metrics, DungeonBaseFragment.TranslationMight, "/Resources/might.png", hellGate.Might, hellGate.MightPerHour);
                AddMetric(metrics, DungeonBaseFragment.TranslationFavor, "/Resources/favor.png", hellGate.Favor, hellGate.FavorPerHour);
                break;
            case CorruptedFragment corrupted:
                AddMetric(metrics, DungeonBaseFragment.TranslationMight, "/Resources/might.png", corrupted.Might, corrupted.MightPerHour);
                AddMetric(metrics, DungeonBaseFragment.TranslationFavor, "/Resources/favor.png", corrupted.Favor, corrupted.FavorPerHour);
                break;
            case AbyssalDepthsFragment abyssalDepths:
                AddMetric(metrics, DungeonBaseFragment.TranslationMight, "/Resources/might.png", abyssalDepths.Might, abyssalDepths.MightPerHour);
                AddMetric(metrics, DungeonBaseFragment.TranslationFavor, "/Resources/favor.png", abyssalDepths.Favor, abyssalDepths.FavorPerHour);
                break;
            case DragonAreaFragment dragonArea:
                AddMetric(metrics, DungeonBaseFragment.TranslationMight, "/Resources/might.png", dragonArea.Might, dragonArea.MightPerHour);
                AddMetric(metrics, DungeonBaseFragment.TranslationFavor, "/Resources/favor.png", dragonArea.Favor, dragonArea.FavorPerHour);
                break;
        }

        return metrics;
    }

    public static (IReadOnlyList<DungeonLootGroup> ChestGroups, DungeonLootGroup OtherLootGroup) BuildLootPresentation(
        IEnumerable<PointOfInterest> events,
        IEnumerable<Loot> loot,
        IReadOnlySet<long> expandedChestIds,
        bool isOtherLootExpanded,
        bool hideClosedChests)
    {
        var visibleLoot = loot.ToList();
        var lootedChestIds = visibleLoot
            .Where(x => x.SourceType == DungeonLootSourceType.Chest)
            .Select(x => x.SourceObjectId)
            .ToHashSet();
        var chestEvents = events
            .Where(x => x.Type is EventType.Chest or EventType.BookChest)
            .Where(x => !hideClosedChests
                        || x.Status == ChestStatus.Open
                        || lootedChestIds.Contains(x.Id))
            .ToList();
        var chestGroups = chestEvents
            .Select(x => CreateChestLootGroup(x, visibleLoot
                .Where(y => y.SourceType == DungeonLootSourceType.Chest && y.SourceObjectId == x.Id)
                .ToList(), expandedChestIds))
            .ToList();
        var knownChestIds = chestEvents.Select(x => (long) x.Id).ToHashSet();

        chestGroups.AddRange(visibleLoot
            .Where(x => x.SourceType == DungeonLootSourceType.Chest && !knownChestIds.Contains(x.SourceObjectId))
            .GroupBy(x => x.SourceObjectId)
            .Select(x => CreateChestLootGroup(null, x.ToList(), expandedChestIds)));

        var otherLoot = visibleLoot.Where(x => x.SourceType != DungeonLootSourceType.Chest).ToList();
        return (chestGroups, DungeonLootGroup.CreateOtherLoot(otherLoot, isOtherLootExpanded));
    }

    private static DungeonRunMetric CreateMetric(string label, string iconPath, double value, double valuePerHour)
    {
        return new DungeonRunMetric
        {
            Label = label,
            IconPath = iconPath,
            Value = value,
            ValuePerHour = valuePerHour
        };
    }

    private static void AddMetric(ICollection<DungeonRunMetric> metrics, string label, string iconPath, double value, double valuePerHour)
    {
        if (value <= 0)
        {
            return;
        }

        metrics.Add(CreateMetric(label, iconPath, value, valuePerHour));
    }

    private static double GetValuePerHour(double value, int durationInSeconds)
    {
        return durationInSeconds <= 0 ? 0 : value / durationInSeconds * 3600;
    }

    private static string GetFactionCoinIconPath(CityFaction cityFaction)
    {
        return GetFactionIconPath(cityFaction, "factioncoin");
    }

    private static string GetFactionStandingIconPath(CityFaction cityFaction)
    {
        return GetFactionIconPath(cityFaction, "factionflag");
    }

    private static string GetFactionIconPath(CityFaction cityFaction, string iconPrefix)
    {
        var factionResourceName = cityFaction switch
        {
            CityFaction.Martlock => "martlock",
            CityFaction.Lymhurst => "lymhurst",
            CityFaction.FortSterling => "fortsterling",
            CityFaction.Bridgewatch => "bridgewatch",
            CityFaction.Thetford => "thetford",
            CityFaction.Caerleon => "caerleon",
            _ => string.Empty
        };

        return string.IsNullOrEmpty(factionResourceName)
            ? string.Empty
            : $"/Resources/{iconPrefix}_{factionResourceName}.png";
    }

    private static DungeonLootGroup CreateChestLootGroup(
        PointOfInterest pointOfInterest,
        IReadOnlyList<Loot> loot,
        IReadOnlySet<long> expandedChestIds)
    {
        var sourceObjectId = pointOfInterest?.Id ?? loot.FirstOrDefault()?.SourceObjectId ?? 0;
        var isOpened = pointOfInterest is null
                       || pointOfInterest.Status == ChestStatus.Open
                       || loot.Count > 0;
        return new DungeonLootGroup(
            sourceObjectId,
            isOpened ? pointOfInterest?.Rarity ?? TreasureRarity.Unknown : TreasureRarity.Unknown,
            pointOfInterest?.Type ?? EventType.Chest,
            pointOfInterest?.IsBossChest ?? false,
            loot,
            expandedChestIds.Contains(sourceObjectId),
            isOpened: isOpened);
    }
}