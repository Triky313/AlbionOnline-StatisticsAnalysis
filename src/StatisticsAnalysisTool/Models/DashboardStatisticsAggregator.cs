using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Cluster;
using StatisticsAnalysisTool.Dungeon;
using StatisticsAnalysisTool.Enumerations;
using System;
using System.Collections.Generic;
using System.Linq;
using ValueType = StatisticsAnalysisTool.Enumerations.ValueType;

namespace StatisticsAnalysisTool.Models;

public sealed class DashboardStatisticsAggregator
{
    private readonly object _syncRoot = new();
    private readonly Dictionary<DateTime, Dictionary<(ValueType ValueType, Guid SessionId, MapType MapType, DungeonMode DungeonMode, ClusterMode ClusterMode, CityFaction CityFaction), double>> _minuteValues = [];
    private readonly Dictionary<DateTime, Dictionary<(ValueType ValueType, Guid SessionId, MapType MapType, DungeonMode DungeonMode, ClusterMode ClusterMode, CityFaction CityFaction), double>> _hourlyValues = [];
    private readonly Dictionary<DateTime, Dictionary<(ValueType ValueType, Guid SessionId, MapType MapType, DungeonMode DungeonMode, ClusterMode ClusterMode, CityFaction CityFaction), double>> _dailyValues = [];
    private readonly EntryBucketIndex _repairCostEntries = new();
    private readonly EntryBucketIndex _economyEntries = new();
    private readonly EntryBucketIndex _lootEntries = new();
    private readonly EntryBucketIndex _lootedChestEntries = new();
    private readonly EntryBucketIndex _combatEntries = new();
    private readonly EntryBucketIndex _mobKillEntries = new();

    public DashboardStatisticsAggregator(DashboardStatistics statistics)
    {
        statistics ??= new DashboardStatistics();

        foreach (var entry in statistics.Entries ?? [])
        {
            AddInternal(NormalizeHistoricalArenaCombatEntry(entry));
        }
    }

    public void Add(StatisticEntry entry)
    {
        if (entry == null || entry.OccurredAtUtc == default)
        {
            return;
        }

        lock (_syncRoot)
        {
            AddInternal(entry);
        }
    }

    public Dictionary<ValueType, Dictionary<DateTime, double>> AggregateChartValues(
        IReadOnlyCollection<DateTime> bucketStarts,
        DashboardChartRangeUnit unit,
        Guid? sessionId,
        DashboardContentType? contentType,
        CityFaction? cityFaction = null)
    {
        var result = new Dictionary<ValueType, Dictionary<DateTime, double>>();
        if (bucketStarts == null || bucketStarts.Count == 0)
        {
            return result;
        }

        lock (_syncRoot)
        {
            var indexedValues = unit switch
            {
                DashboardChartRangeUnit.Minute => _minuteValues,
                DashboardChartRangeUnit.Hour => _hourlyValues,
                DashboardChartRangeUnit.Day => _dailyValues,
                _ => _dailyValues
            };

            foreach (var bucketStart in bucketStarts.Distinct())
            {
                if (!indexedValues.TryGetValue(bucketStart, out var bucketValues))
                {
                    continue;
                }

                foreach (var (key, value) in bucketValues)
                {
                    if (sessionId.HasValue && key.SessionId != sessionId.Value
                        || !MatchesContentFilter(contentType, key.MapType, key.DungeonMode, key.ClusterMode)
                        || cityFaction.HasValue && key.CityFaction != cityFaction.Value)
                    {
                        continue;
                    }

                    AddValue(result, key.ValueType, bucketStart, value);
                }
            }
        }

        return result;
    }

    public Dictionary<(MapType MapType, DungeonMode DungeonMode, ClusterMode ClusterMode), double> AggregateContentValues(
        IReadOnlyCollection<DateTime> bucketStarts,
        DashboardChartRangeUnit unit,
        Guid? sessionId,
        ValueType valueType,
        DashboardContentType? contentType = null)
    {
        var result = new Dictionary<(MapType MapType, DungeonMode DungeonMode, ClusterMode ClusterMode), double>();
        if (bucketStarts == null || bucketStarts.Count == 0)
        {
            return result;
        }

        lock (_syncRoot)
        {
            var indexedValues = unit switch
            {
                DashboardChartRangeUnit.Minute => _minuteValues,
                DashboardChartRangeUnit.Hour => _hourlyValues,
                DashboardChartRangeUnit.Day => _dailyValues,
                _ => _dailyValues
            };

            foreach (var bucketStart in bucketStarts.Distinct())
            {
                if (!indexedValues.TryGetValue(bucketStart, out var bucketValues))
                {
                    continue;
                }

                foreach (var (key, value) in bucketValues)
                {
                    if (key.ValueType != valueType
                        || sessionId.HasValue && key.SessionId != sessionId.Value
                        || !MatchesContentFilter(contentType, key.MapType, key.DungeonMode, key.ClusterMode))
                    {
                        continue;
                    }

                    var contentKey = (
                        key.MapType,
                        key.MapType == MapType.RandomDungeon
                            ? key.DungeonMode
                            : DungeonMode.Unknown,
                        key.MapType == MapType.Unknown
                            ? key.ClusterMode
                            : ClusterMode.Unknown);
                    result[contentKey] = result.GetValueOrDefault(contentKey) + value;
                }
            }
        }

        return result;
    }

    public DashboardEconomyStatistics AggregateEconomyValues(
        IReadOnlyCollection<DateTime> bucketStarts,
        DashboardChartRangeUnit unit,
        Guid? sessionId)
    {
        var result = new DashboardEconomyStatistics();
        if (bucketStarts == null || bucketStarts.Count == 0)
        {
            return result;
        }

        lock (_syncRoot)
        {
            foreach (var bucketStart in bucketStarts.Distinct())
            {
                foreach (var entry in _economyEntries.GetEntries(bucketStart, unit))
                {
                    if (sessionId.HasValue && entry.SessionId != sessionId.Value)
                    {
                        continue;
                    }

                    AddEconomyEntry(result, entry);
                }
            }
        }

        return result;
    }

    public IReadOnlyList<StatisticEntry> GetLootEntries(
        IReadOnlyCollection<DateTime> bucketStarts,
        DashboardChartRangeUnit unit,
        Guid? sessionId,
        DashboardContentType? contentType)
    {
        if (bucketStarts == null || bucketStarts.Count == 0)
        {
            return [];
        }

        return GetEntries(_lootEntries, bucketStarts, unit, sessionId, contentType);
    }

    public IReadOnlyList<StatisticEntry> GetLootedChestEntries(
        IReadOnlyCollection<DateTime> bucketStarts,
        DashboardChartRangeUnit unit,
        Guid? sessionId,
        DashboardContentType? contentType)
    {
        if (bucketStarts == null || bucketStarts.Count == 0)
        {
            return [];
        }

        return GetEntries(_lootedChestEntries, bucketStarts, unit, sessionId, contentType);
    }

    public IReadOnlyList<StatisticEntry> GetCombatEntries(
        DateTime rangeStartUtc,
        DateTime rangeEndUtc,
        Guid? sessionId,
        DashboardContentType? contentType)
    {
        if (rangeStartUtc >= rangeEndUtc)
        {
            return [];
        }

        var firstLocalDate = rangeStartUtc.ToLocalTime().Date;
        var lastLocalDate = rangeEndUtc.ToLocalTime().Date;
        var dayBucketStarts = Enumerable.Range(0, (lastLocalDate - firstLocalDate).Days + 1)
            .Select(dayOffset => firstLocalDate.AddDays(dayOffset))
            .ToArray();

        return GetEntries(
                _combatEntries,
                dayBucketStarts,
                DashboardChartRangeUnit.Day,
                sessionId,
                contentType)
            .Where(entry => entry.OccurredAtUtc >= rangeStartUtc && entry.OccurredAtUtc < rangeEndUtc)
            .ToArray();
    }

    public IReadOnlyList<StatisticEntry> GetMobKillEntries(
        IReadOnlyCollection<DateTime> bucketStarts,
        DashboardChartRangeUnit unit,
        Guid? sessionId,
        DashboardContentType? contentType)
    {
        if (bucketStarts == null || bucketStarts.Count == 0)
        {
            return [];
        }

        return GetEntries(_mobKillEntries, bucketStarts, unit, sessionId, contentType);
    }

    private IReadOnlyList<StatisticEntry> GetEntries(
        EntryBucketIndex index,
        IReadOnlyCollection<DateTime> bucketStarts,
        DashboardChartRangeUnit unit,
        Guid? sessionId,
        DashboardContentType? contentType)
    {
        var result = new List<StatisticEntry>();

        lock (_syncRoot)
        {
            foreach (var bucketStart in bucketStarts.Distinct())
            {
                foreach (var entry in index.GetEntries(bucketStart, unit))
                {
                    if ((!sessionId.HasValue || entry.SessionId == sessionId.Value)
                        && MatchesContentFilter(contentType, entry.MapType, entry.DungeonMode, entry.ClusterMode))
                    {
                        result.Add(entry);
                    }
                }
            }
        }

        return result;
    }

    private static bool MatchesContentFilter(
        DashboardContentType? selectedContentType,
        MapType mapType,
        DungeonMode dungeonMode,
        ClusterMode clusterMode)
    {
        if (!selectedContentType.HasValue)
        {
            return true;
        }

        return DashboardContentTypeResolver.Resolve(mapType, dungeonMode, clusterMode) == selectedContentType.Value;
    }

    public double SumRepairCosts(DateTime localStartInclusive, DateTime localEndExclusive)
    {
        if (localEndExclusive <= localStartInclusive)
        {
            return 0;
        }

        var startUtc = localStartInclusive.ToUniversalTime();
        var endUtc = localEndExclusive.ToUniversalTime();
        var result = 0d;

        lock (_syncRoot)
        {
            for (var day = localStartInclusive.Date; day < localEndExclusive; day = day.AddDays(1))
            {
                result += _repairCostEntries
                    .GetEntries(day, DashboardChartRangeUnit.Day)
                    .Where(entry => entry.OccurredAtUtc >= startUtc && entry.OccurredAtUtc < endUtc)
                    .Sum(entry => entry.Value);
            }
        }

        return result;
    }

    private void AddInternal(StatisticEntry entry)
    {
        if (entry.OccurredAtUtc == default)
        {
            return;
        }

        var localDate = entry.OccurredAtUtc.ToLocalTime();
        var minuteBucket = new DateTime(localDate.Year, localDate.Month, localDate.Day, localDate.Hour, localDate.Minute, 0);
        var hourBucket = new DateTime(localDate.Year, localDate.Month, localDate.Day, localDate.Hour, 0, 0);
        var dayBucket = localDate.Date;

        AddIndexedValue(_minuteValues, entry, minuteBucket);
        AddIndexedValue(_hourlyValues, entry, hourBucket);
        AddIndexedValue(_dailyValues, entry, dayBucket);

        if (entry.ValueType is ValueType.ReSpec
            or ValueType.PaidSilverForReSpec
            or ValueType.RepairCosts
            or ValueType.ItemQualityRerollCosts
            or ValueType.ItemQualityRerollResult
            or ValueType.ItemQualityRerollAttempt
            or ValueType.AwakenedWeaponCosts
            or ValueType.AwakenedWeaponTraitUpgrade
            or ValueType.AwakenedWeaponTraitUpgradeProc)
        {
            _economyEntries.Add(entry, minuteBucket, hourBucket, dayBucket);
        }

        if (entry.ValueType is ValueType.PlayerKill
            or ValueType.PlayerDeath
            or ValueType.PlayerKnockout
            or ValueType.PlayerKnockedOut)
        {
            _combatEntries.Add(entry, minuteBucket, hourBucket, dayBucket);
        }

        if (entry.ValueType == ValueType.MobKill && !string.IsNullOrWhiteSpace(entry.MobUniqueName))
        {
            _mobKillEntries.Add(entry, minuteBucket, hourBucket, dayBucket);
        }

        if (entry.ValueType == ValueType.RepairCosts)
        {
            _repairCostEntries.Add(entry, minuteBucket, hourBucket, dayBucket);
        }

        if (entry.ValueType == ValueType.LootValue
            && entry.ItemIndex > 0
            && entry.ItemQuantity > 0)
        {
            _lootEntries.Add(entry, minuteBucket, hourBucket, dayBucket);
        }

        if (entry.ValueType == ValueType.LootedChest
            && entry.TreasureRarity != TreasureRarity.Unknown)
        {
            _lootedChestEntries.Add(entry, minuteBucket, hourBucket, dayBucket);
        }
    }

    private static StatisticEntry NormalizeHistoricalArenaCombatEntry(StatisticEntry entry)
    {
        var normalizedValueType = (entry.MapType, entry.ValueType) switch
        {
            (MapType.Arena, ValueType.PlayerKill) => ValueType.PlayerKnockout,
            (MapType.Arena, ValueType.PlayerDeath) => ValueType.PlayerKnockedOut,
            _ => entry.ValueType
        };
        if (normalizedValueType == entry.ValueType)
        {
            return entry;
        }

        return new StatisticEntry
        {
            SessionId = entry.SessionId,
            OccurredAtUtc = entry.OccurredAtUtc,
            ValueType = normalizedValueType,
            Value = entry.Value,
            MapType = entry.MapType,
            DungeonMode = entry.DungeonMode,
            ClusterMode = entry.ClusterMode,
            CityFaction = entry.CityFaction,
            CombatAreaIndex = entry.CombatAreaIndex,
            CombatAreaClusterType = entry.CombatAreaClusterType,
            CombatOpponentName = entry.CombatOpponentName,
            CombatLootValue = entry.CombatLootValue,
            CombatKiller = entry.CombatKiller,
            CombatVictim = entry.CombatVictim
        };
    }

    private static void AddEconomyEntry(DashboardEconomyStatistics result, StatisticEntry entry)
    {
        var absoluteValue = Math.Abs(entry.Value);
        switch (entry.ValueType)
        {
            case ValueType.ReSpec:
                result.ReSpec += entry.Value;
                if (entry.Value < 0)
                {
                    result.SpentReSpec += absoluteValue;
                }
                break;
            case ValueType.PaidSilverForReSpec:
                result.ReSpecSilverCost += absoluteValue;
                break;
            case ValueType.RepairCosts:
                result.RepairCosts += absoluteValue;
                result.HighestRepairCost = Math.Max(result.HighestRepairCost, absoluteValue);
                break;
            case ValueType.ItemQualityRerollCosts:
                result.ItemQualityRerollCosts += absoluteValue;
                AddItemQualityCount(result, entry.ItemQuality, entry.ItemQuantity);
                break;
            case ValueType.ItemQualityRerollResult:
                AddItemQualityCount(result, entry.ItemQuality, entry.ItemQuantity);
                AddSuccessfulItemQualityRerollCount(result, entry.ItemQuality, entry.ItemQuantity);
                break;
            case ValueType.ItemQualityRerollAttempt:
                AddEligibleItemQualityRerollCounts(result, entry.ItemQuality, entry.ItemQuantity);
                break;
            case ValueType.AwakenedWeaponCosts:
                result.AwakenedWeaponCosts += absoluteValue;
                break;
            case ValueType.AwakenedWeaponTraitUpgrade:
                result.AwakenedWeaponTraitUpgradeCount += entry.ItemQuantity;
                break;
            case ValueType.AwakenedWeaponTraitUpgradeProc:
                result.AwakenedWeaponTraitUpgradeProcCount += entry.ItemQuantity;
                break;
        }
    }

    private static void AddItemQualityCount(
        DashboardEconomyStatistics statistics,
        ItemQuality itemQuality,
        int itemQuantity)
    {
        var quantity = itemQuantity > 0 ? itemQuantity : 1;
        switch (itemQuality)
        {
            case ItemQuality.Good:
                statistics.GoodItemCount += quantity;
                break;
            case ItemQuality.Outstanding:
                statistics.OutstandingItemCount += quantity;
                break;
            case ItemQuality.Excellent:
                statistics.ExcellentItemCount += quantity;
                break;
            case ItemQuality.Masterpiece:
                statistics.MasterpieceItemCount += quantity;
                break;
        }
    }

    private static void AddSuccessfulItemQualityRerollCount(
        DashboardEconomyStatistics statistics,
        ItemQuality itemQuality,
        int itemQuantity)
    {
        if (itemQuantity <= 0)
        {
            return;
        }

        switch (itemQuality)
        {
            case ItemQuality.Good:
                statistics.GoodItemSuccessfulRerollCount += itemQuantity;
                break;
            case ItemQuality.Outstanding:
                statistics.OutstandingItemSuccessfulRerollCount += itemQuantity;
                break;
            case ItemQuality.Excellent:
                statistics.ExcellentItemSuccessfulRerollCount += itemQuantity;
                break;
            case ItemQuality.Masterpiece:
                statistics.MasterpieceItemSuccessfulRerollCount += itemQuantity;
                break;
        }
    }

    private static void AddEligibleItemQualityRerollCounts(
        DashboardEconomyStatistics statistics,
        ItemQuality sourceItemQuality,
        int itemQuantity)
    {
        if (sourceItemQuality is < ItemQuality.Normal or >= ItemQuality.Masterpiece
            || itemQuantity <= 0)
        {
            return;
        }

        if (sourceItemQuality < ItemQuality.Good)
        {
            statistics.GoodItemEligibleRerollCount += itemQuantity;
        }

        if (sourceItemQuality < ItemQuality.Outstanding)
        {
            statistics.OutstandingItemEligibleRerollCount += itemQuantity;
        }

        if (sourceItemQuality < ItemQuality.Excellent)
        {
            statistics.ExcellentItemEligibleRerollCount += itemQuantity;
        }

        if (sourceItemQuality < ItemQuality.Masterpiece)
        {
            statistics.MasterpieceItemEligibleRerollCount += itemQuantity;
        }
    }

    private static void AddIndexedValue(
        IDictionary<DateTime, Dictionary<(ValueType ValueType, Guid SessionId, MapType MapType, DungeonMode DungeonMode, ClusterMode ClusterMode, CityFaction CityFaction), double>> values,
        StatisticEntry entry,
        DateTime bucket)
    {
        if (!values.TryGetValue(bucket, out var bucketValues))
        {
            bucketValues = [];
            values[bucket] = bucketValues;
        }

        var key = (entry.ValueType, entry.SessionId, entry.MapType, entry.DungeonMode, entry.ClusterMode, entry.CityFaction);
        bucketValues[key] = bucketValues.GetValueOrDefault(key) + entry.Value;
    }

    private static void AddValue(
        IDictionary<ValueType, Dictionary<DateTime, double>> result,
        ValueType valueType,
        DateTime bucket,
        double value)
    {
        if (!result.TryGetValue(valueType, out var valuesByBucket))
        {
            valuesByBucket = new Dictionary<DateTime, double>();
            result[valueType] = valuesByBucket;
        }

        valuesByBucket[bucket] = valuesByBucket.GetValueOrDefault(bucket) + value;
    }

    private sealed class EntryBucketIndex
    {
        private readonly Dictionary<DateTime, List<StatisticEntry>> _minuteEntries = [];
        private readonly Dictionary<DateTime, List<StatisticEntry>> _hourlyEntries = [];
        private readonly Dictionary<DateTime, List<StatisticEntry>> _dailyEntries = [];

        public void Add(
            StatisticEntry entry,
            DateTime minuteBucket,
            DateTime hourBucket,
            DateTime dayBucket)
        {
            AddEntry(_minuteEntries, minuteBucket, entry);
            AddEntry(_hourlyEntries, hourBucket, entry);
            AddEntry(_dailyEntries, dayBucket, entry);
        }

        public IReadOnlyList<StatisticEntry> GetEntries(
            DateTime bucketStart,
            DashboardChartRangeUnit unit)
        {
            var entries = unit switch
            {
                DashboardChartRangeUnit.Minute => _minuteEntries,
                DashboardChartRangeUnit.Hour => _hourlyEntries,
                DashboardChartRangeUnit.Day => _dailyEntries,
                _ => _dailyEntries
            };

            return entries.GetValueOrDefault(bucketStart) ?? [];
        }

        private static void AddEntry(
            IDictionary<DateTime, List<StatisticEntry>> entries,
            DateTime bucket,
            StatisticEntry entry)
        {
            if (!entries.TryGetValue(bucket, out var bucketEntries))
            {
                bucketEntries = [];
                entries[bucket] = bucketEntries;
            }

            bucketEntries.Add(entry);
        }
    }
}