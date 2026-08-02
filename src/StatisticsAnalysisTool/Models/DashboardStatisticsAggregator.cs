using StatisticsAnalysisTool.Cluster;
using StatisticsAnalysisTool.Dungeon;
using System;
using System.Collections.Generic;
using System.Linq;
using ValueType = StatisticsAnalysisTool.Enumerations.ValueType;

namespace StatisticsAnalysisTool.Models;

public sealed class DashboardStatisticsAggregator
{
    private readonly object _syncRoot = new();
    private readonly Dictionary<(ValueType ValueType, DateTime Bucket, Guid SessionId, MapType MapType, DungeonMode DungeonMode), double> _hourlyValues = new();
    private readonly Dictionary<(ValueType ValueType, DateTime Bucket, Guid SessionId, MapType MapType, DungeonMode DungeonMode), double> _dailyValues = new();
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
        bool useHourlyValues,
        Guid? sessionId,
        MapType? mapType,
        DungeonMode? dungeonMode)
    {
        var result = new Dictionary<ValueType, Dictionary<DateTime, double>>();
        if (bucketStarts == null || bucketStarts.Count == 0)
        {
            return result;
        }

        var validBuckets = bucketStarts.ToHashSet();

        lock (_syncRoot)
        {
            var indexedValues = useHourlyValues ? _hourlyValues : _dailyValues;
            foreach (var (key, value) in indexedValues)
            {
                if (!validBuckets.Contains(key.Bucket)
                    || sessionId.HasValue && key.SessionId != sessionId.Value
                    || mapType.HasValue && key.MapType != mapType.Value
                    || dungeonMode.HasValue && key.DungeonMode != dungeonMode.Value)
                {
                    continue;
                }

                AddValue(result, key.ValueType, key.Bucket, value);
            }
        }

        return result;
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
        var hourBucket = new DateTime(localDate.Year, localDate.Month, localDate.Day, localDate.Hour, 0, 0);
        var dayBucket = localDate.Date;

        AddIndexedValue(_hourlyValues, entry, hourBucket);
        AddIndexedValue(_dailyValues, entry, dayBucket);

        if (entry.ValueType == ValueType.RepairCosts)
        {
            _repairCostEntries.Add((entry.OccurredAtUtc, entry.Value));
        }
    }

    private static void AddIndexedValue(
        IDictionary<(ValueType ValueType, DateTime Bucket, Guid SessionId, MapType MapType, DungeonMode DungeonMode), double> values,
        StatisticEntry entry,
        DateTime bucket)
    {
        var key = (entry.ValueType, bucket, entry.SessionId, entry.MapType, entry.DungeonMode);
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
