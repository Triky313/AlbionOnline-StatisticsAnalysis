using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Dungeon.Models;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Localization;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StatisticsAnalysisTool.Dungeon;

public static class DungeonAnalyticsService
{
    public static void Populate(
        DungeonAnalytics analytics,
        IReadOnlyList<DungeonBaseFragment> currentDungeons,
        IReadOnlyList<DungeonBaseFragment> previousDungeons,
        DungeonMode selectedMode)
    {
        var current = CalculateTotals(currentDungeons);
        var previous = CalculateTotals(previousDungeons);

        analytics.TotalRuns.Update(current.RunCount, previous.RunCount);
        analytics.TotalFame.Update(current.Fame, previous.Fame, GetValuePerHour(current.Fame, current.DurationInSeconds));
        analytics.TotalReSpec.Update(current.ReSpec, previous.ReSpec, GetValuePerHour(current.ReSpec, current.DurationInSeconds));
        analytics.TotalMight.Update(current.Might, previous.Might, GetValuePerHour(current.Might, current.DurationInSeconds));
        analytics.TotalFavor.Update(current.Favor, previous.Favor, GetValuePerHour(current.Favor, current.DurationInSeconds));
        analytics.TotalSilver.Update(current.Silver, previous.Silver, GetValuePerHour(current.Silver, current.DurationInSeconds));
        analytics.TotalLootValue.Update(current.LootValue, previous.LootValue, GetValuePerHour(current.LootValue, current.DurationInSeconds));
        analytics.Deaths.Update(current.Deaths, previous.Deaths);
        analytics.AverageRunTime.Update(current.AverageRunTimeInSeconds, previous.AverageRunTimeInSeconds);

        UpdateOverview(analytics, currentDungeons, selectedMode);
        analytics.EfficiencyEntries = CreateEfficiencyEntries(currentDungeons);
    }

    public static string GetTierEnchantment(DungeonBaseFragment dungeon)
    {
        if (dungeon.Tier == Tier.Unknown)
        {
            return GetModeName(dungeon.Mode);
        }

        var tier = $"T{(int) dungeon.Tier}";
        return dungeon is RandomDungeonFragment { Level: >= 0 } randomDungeon ? $"{tier}.{randomDungeon.Level}" : tier;
    }

    private static PeriodTotals CalculateTotals(IReadOnlyList<DungeonBaseFragment> dungeons)
    {
        var durationInSeconds = dungeons.Sum(x => x.EffectiveRunTimeInSeconds);
        var fame = dungeons.Sum(x => x.Fame);
        var reSpec = dungeons.Sum(x => x.ReSpec);
        var silver = dungeons.Sum(x => x.Silver);
        var might = dungeons.Sum(GetMight);
        var favor = dungeons.Sum(GetFavor);
        var lootValue = dungeons.Sum(GetLootValue);
        var deaths = dungeons.Count(x => x.KillStatus == KillStatus.LocalPlayerDead);
        var averageRunTime = dungeons.Count == 0 ? 0 : dungeons.Average(x => x.EffectiveRunTimeInSeconds);

        return new PeriodTotals(dungeons.Count, durationInSeconds, fame, reSpec, silver, might, favor, lootValue, deaths, averageRunTime);
    }

    private static void UpdateOverview(DungeonAnalytics analytics, IReadOnlyList<DungeonBaseFragment> dungeons, DungeonMode selectedMode)
    {
        var tierGroups = dungeons
            .Where(x => x.Tier != Tier.Unknown)
            .GroupBy(GetTierEnchantment)
            .Select(group => new TierStatistics(
                group.Key,
                group.Count(),
                group.Sum(GetLootValue),
                group.Sum(x => x.EffectiveRunTimeInSeconds)))
            .ToList();

        var mostProfitable = tierGroups
            .Where(x => x.DurationInSeconds > 0)
            .OrderByDescending(x => GetValuePerHour(x.LootValue, x.DurationInSeconds))
            .FirstOrDefault();
        analytics.MostProfitableTierEnchantment = mostProfitable?.Name ?? "—";
        analytics.MostProfitableLootPerHour = mostProfitable is null ? 0 : GetValuePerHour(mostProfitable.LootValue, mostProfitable.DurationInSeconds);

        var mostPlayed = tierGroups
            .OrderByDescending(x => x.RunCount)
            .ThenByDescending(x => x.LootValue)
            .FirstOrDefault();
        analytics.MostPlayedTierEnchantment = mostPlayed?.Name ?? "—";
        analytics.MostPlayedTierRunCount = mostPlayed?.RunCount ?? 0;

        analytics.MostValuableLoot = dungeons
            .SelectMany(x => x.VisibleLoot)
            .OrderByDescending(x => x.EstimatedMarketValueInternal)
            .FirstOrDefault() ?? new Loot();

        var bestMap = dungeons
            .Where(x => !string.IsNullOrWhiteSpace(x.MainMapName))
            .GroupBy(x => x.MainMapName)
            .Select(group => new MapStatistics(group.Key, group.Average(GetLootValue)))
            .OrderByDescending(x => x.AverageLoot)
            .FirstOrDefault();
        analytics.BestDungeonMap = bestMap?.Name ?? "—";
        analytics.BestDungeonMapAverageLoot = bestMap?.AverageLoot ?? 0;

        DungeonBaseFragment fastestRun = null;
        if (selectedMode != DungeonMode.Unknown)
        {
            fastestRun = dungeons
                .Where(x => x.EffectiveRunTimeInSeconds > 0)
                .OrderBy(x => x.EffectiveRunTimeInSeconds)
                .FirstOrDefault()
                ?? dungeons.FirstOrDefault();
        }

        analytics.HasFastestRun = fastestRun is not null;
        analytics.FastestRunTimeInSeconds = fastestRun?.EffectiveRunTimeInSeconds ?? 0;
        analytics.FastestRunTierEnchantment = fastestRun is null ? "—" : GetTierEnchantment(fastestRun);
        analytics.FastestRunPartySize = fastestRun?.PartySize ?? 0;

        var openedChests = dungeons.Sum(x => x.Events.Count(point =>
            point.Status == ChestStatus.Open && point.Type is EventType.Chest or EventType.BookChest));
        analytics.AverageChestsPerRun = dungeons.Count == 0 ? 0 : (double) openedChests / dungeons.Count;
    }

    private static IReadOnlyList<DungeonEfficiencyEntry> CreateEfficiencyEntries(IReadOnlyList<DungeonBaseFragment> dungeons)
    {
        var entries = dungeons
            .GroupBy(GetEfficiencyGroupName)
            .Select(group => new EfficiencyValues(
                group.Key,
                group.Average(GetLootValue),
                group.Average(x => x.Fame),
                group.Average(x => x.EffectiveRunTimeInSeconds)))
            .OrderByDescending(x => x.AverageLootPerRun)
            .ThenByDescending(x => x.AverageFamePerRun)
            .Take(5)
            .ToList();

        var maximumLoot = entries.Select(x => x.AverageLootPerRun).DefaultIfEmpty(0).Max();
        var maximumFame = entries.Select(x => x.AverageFamePerRun).DefaultIfEmpty(0).Max();

        return entries.Select(x => new DungeonEfficiencyEntry()
        {
            TierEnchantment = x.Name,
            AverageLootPerRun = x.AverageLootPerRun,
            AverageFamePerRun = x.AverageFamePerRun,
            AverageDurationInSeconds = x.AverageDurationInSeconds,
            LootScore = GetScore(x.AverageLootPerRun, maximumLoot),
            FameScore = GetScore(x.AverageFamePerRun, maximumFame)
        }).ToList();
    }

    private static string GetEfficiencyGroupName(DungeonBaseFragment dungeon)
    {
        if (dungeon.Tier != Tier.Unknown)
        {
            return GetTierEnchantment(dungeon);
        }

        return dungeon.Mode switch
        {
            DungeonMode.Solo or DungeonMode.Standard or DungeonMode.Avalon
                or DungeonMode.Mists or DungeonMode.MistsDungeon => "T?",
            _ => GetModeName(dungeon.Mode)
        };
    }

    private static double GetLootValue(DungeonBaseFragment dungeon)
    {
        return dungeon.VisibleLoot.Sum(x => x.Quantity * FixPoint.FromInternalValue(x.EstimatedMarketValueInternal).DoubleValue);
    }

    private static double GetMight(DungeonBaseFragment dungeon)
    {
        return dungeon switch
        {
            RandomDungeonFragment randomDungeon => randomDungeon.Might,
            CorruptedFragment corrupted => corrupted.Might,
            HellGateFragment hellGate => hellGate.Might,
            MistsFragment mists => mists.Might,
            MistsDungeonFragment mistsDungeon => mistsDungeon.Might,
            AbyssalDepthsFragment abyssalDepths => abyssalDepths.Might,
            DragonAreaFragment dragonArea => dragonArea.Might,
            _ => 0
        };
    }

    private static double GetFavor(DungeonBaseFragment dungeon)
    {
        return dungeon switch
        {
            RandomDungeonFragment randomDungeon => randomDungeon.Favor,
            CorruptedFragment corrupted => corrupted.Favor,
            HellGateFragment hellGate => hellGate.Favor,
            MistsFragment mists => mists.Favor,
            MistsDungeonFragment mistsDungeon => mistsDungeon.Favor,
            AbyssalDepthsFragment abyssalDepths => abyssalDepths.Favor,
            DragonAreaFragment dragonArea => dragonArea.Favor,
            _ => 0
        };
    }

    private static double GetValuePerHour(double value, double durationInSeconds)
    {
        return durationInSeconds <= 0 ? 0 : value / durationInSeconds * 3600;
    }

    private static double GetScore(double value, double maximum)
    {
        return maximum <= 0 ? 0 : value / maximum * 100;
    }

    private static string GetModeName(DungeonMode mode)
    {
        var translationKey = mode switch
        {
            DungeonMode.Solo => "SOLO_DUNGEON",
            DungeonMode.Standard => "STANDARD_DUNGEON",
            DungeonMode.Avalon => "AVALONIAN_DUNGEON",
            DungeonMode.HellGate => "HELLGATE",
            DungeonMode.Corrupted => "CORRUPTED",
            DungeonMode.Expedition => "HCE_EXPEDITION",
            DungeonMode.Mists => "MISTS",
            DungeonMode.MistsDungeon => "MISTS_DUNGEON",
            DungeonMode.AbyssalDepths => "ABYSSALDEPTHS",
            DungeonMode.DragonArea => "DRAGONAREA",
            _ => "UNKNOWN"
        };

        return LocalizationController.Translation(translationKey);
    }

    private sealed record PeriodTotals(
        int RunCount,
        double DurationInSeconds,
        double Fame,
        double ReSpec,
        double Silver,
        double Might,
        double Favor,
        double LootValue,
        int Deaths,
        double AverageRunTimeInSeconds);

    private sealed record TierStatistics(string Name, int RunCount, double LootValue, double DurationInSeconds);
    private sealed record MapStatistics(string Name, double AverageLoot);
    private sealed record EfficiencyValues(string Name, double AverageLootPerRun, double AverageFamePerRun, double AverageDurationInSeconds);
}
