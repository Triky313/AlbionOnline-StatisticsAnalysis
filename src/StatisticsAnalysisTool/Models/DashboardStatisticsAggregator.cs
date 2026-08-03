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
    private readonly Dictionary<(ValueType ValueType, DateTime Bucket, Guid SessionId, MapType MapType, DungeonMode DungeonMode, ClusterMode ClusterMode), double> _minuteValues = new();
    private readonly Dictionary<(ValueType ValueType, DateTime Bucket, Guid SessionId, MapType MapType, DungeonMode DungeonMode, ClusterMode ClusterMode), double> _hourlyValues = new();
    private readonly Dictionary<(ValueType ValueType, DateTime Bucket, Guid SessionId, MapType MapType, DungeonMode DungeonMode, ClusterMode ClusterMode), double> _dailyValues = new();
    private readonly List<(DateTime OccurredAtUtc, double Value)> _repairCostEntries = [];

    public DashboardStatisticsAggregator(DashboardStatistics statistics)
    {
        statistics ??= new DashboardStatistics();

        foreach (var entry in statistics.Entries ?? [])
        {
            AddInternal(entry);
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
        DashboardContentType? contentType)
    {
        var result = new Dictionary<ValueType, Dictionary<DateTime, double>>();
        if (bucketStarts == null || bucketStarts.Count == 0)
        {
            return result;
        }

        var validBuckets = bucketStarts.ToHashSet();

        lock (_syncRoot)
        {
            var indexedValues = unit switch
            {
                DashboardChartRangeUnit.Minute => _minuteValues,
                DashboardChartRangeUnit.Hour => _hourlyValues,
                DashboardChartRangeUnit.Day => _dailyValues,
                _ => _dailyValues
            };

            foreach (var (key, value) in indexedValues)
            {
                if (!validBuckets.Contains(key.Bucket)
                    || sessionId.HasValue && key.SessionId != sessionId.Value
                    || !MatchesContentFilter(contentType, key.MapType, key.DungeonMode, key.ClusterMode))
                {
                    continue;
                }

                AddValue(result, key.ValueType, key.Bucket, value);
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

        var validBuckets = bucketStarts.ToHashSet();

        lock (_syncRoot)
        {
            var indexedValues = unit switch
            {
                DashboardChartRangeUnit.Minute => _minuteValues,
                DashboardChartRangeUnit.Hour => _hourlyValues,
                DashboardChartRangeUnit.Day => _dailyValues,
                _ => _dailyValues
            };

            foreach (var (key, value) in indexedValues)
            {
                if (key.ValueType != valueType
                    || !validBuckets.Contains(key.Bucket)
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
        var startUtc = localStartInclusive.ToUniversalTime();
        var endUtc = localEndExclusive.ToUniversalTime();

        lock (_syncRoot)
        {
            return _repairCostEntries
                .Where(x => x.OccurredAtUtc >= startUtc && x.OccurredAtUtc < endUtc)
                .Sum(x => x.Value);
        }
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

        if (entry.ValueType == ValueType.RepairCosts)
        {
            _repairCostEntries.Add((entry.OccurredAtUtc, entry.Value));
        }
    }

    private static void AddIndexedValue(
        IDictionary<(ValueType ValueType, DateTime Bucket, Guid SessionId, MapType MapType, DungeonMode DungeonMode, ClusterMode ClusterMode), double> values,
        StatisticEntry entry,
        DateTime bucket)
    {
        var key = (entry.ValueType, bucket, entry.SessionId, entry.MapType, entry.DungeonMode, entry.ClusterMode);
        var currentValue = values.TryGetValue(key, out var existingValue) ? existingValue : 0;
        values[key] = currentValue + entry.Value;
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
}
