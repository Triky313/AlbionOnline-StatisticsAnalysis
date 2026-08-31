using StatisticsAnalysisTool.Cluster;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.GameFileData;
using StatisticsAnalysisTool.Localization;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StatisticsAnalysisTool.Gathering;

public sealed class GatheringStatisticsCache
{
    private readonly Dictionary<Gathered, EntrySnapshot> _entries = new(ReferenceEqualityComparer.Instance);
    private readonly TimestampIndex _entriesByTimestamp = new();
    private readonly Dictionary<ResourceKey, MetricAccumulator> _resourcesByType = [];
    private readonly Dictionary<string, ResourceAccumulator> _resources = [];
    private readonly Dictionary<MapKey, MapAccumulator> _maps = [];
    private readonly Dictionary<string, ClusterAccumulator> _clusters = [];
    private readonly Dictionary<GatheringResourceType, MetricAccumulator> _resourceTypes = [];
    private readonly Dictionary<ChartKey, ChartAccumulator> _chartValues = [];
    private readonly SortedDictionary<long, int> _marketValues = [];
    private readonly MetricAccumulator _totals = new(true);
    private Guid? _sessionId;
    private int _bucketCount;
    private GatheringTimeRangeUnit _timeRangeUnit;
    private DateTime _timeRangeStart;
    private DateTime _timeRangeEnd;

    public bool IsInitialized { get; private set; }

    public bool MatchesFilter(Guid? sessionId, int bucketCount, GatheringTimeRangeUnit timeRangeUnit)
    {
        return IsInitialized
               && _sessionId == sessionId
               && _bucketCount == bucketCount
               && _timeRangeUnit == timeRangeUnit;
    }

    public void Rebuild(
        IEnumerable<Gathered> gatheredEntries,
        Guid? sessionId,
        DateTime timeRangeStart,
        DateTime timeRangeEnd,
        int bucketCount,
        GatheringTimeRangeUnit timeRangeUnit)
    {
        Clear();
        _sessionId = sessionId;
        _timeRangeStart = timeRangeStart;
        _timeRangeEnd = timeRangeEnd;
        _bucketCount = bucketCount;
        _timeRangeUnit = timeRangeUnit;

        foreach (var gathered in gatheredEntries)
        {
            AddIfIncluded(gathered);
        }

        IsInitialized = true;
    }

    public IReadOnlySet<GatheringResourceType> Update(Gathered gathered)
    {
        var affectedResourceTypes = new HashSet<GatheringResourceType>();
        if (_entries.TryGetValue(gathered, out var previousSnapshot))
        {
            affectedResourceTypes.Add(previousSnapshot.ResourceType);
            Remove(previousSnapshot);
        }

        if (AddIfIncluded(gathered, out var currentSnapshot))
        {
            affectedResourceTypes.Add(currentSnapshot.ResourceType);
        }

        return affectedResourceTypes;
    }

    public IReadOnlySet<GatheringResourceType> Remove(Gathered gathered)
    {
        if (!_entries.TryGetValue(gathered, out var snapshot))
        {
            return new HashSet<GatheringResourceType>();
        }

        Remove(snapshot);
        return new HashSet<GatheringResourceType> { snapshot.ResourceType };
    }

    public IReadOnlySet<GatheringResourceType> AdvanceTimeRange(DateTime timeRangeStart, DateTime timeRangeEnd)
    {
        _timeRangeStart = timeRangeStart;
        _timeRangeEnd = timeRangeEnd;
        var affectedResourceTypes = new HashSet<GatheringResourceType>();

        while (_entriesByTimestamp.TryGetFirst(out var timestampUtc, out var gatheredEntries)
               && new DateTime(timestampUtc, DateTimeKind.Utc).ToLocalTime() < _timeRangeStart)
        {
            foreach (var gathered in gatheredEntries.ToList())
            {
                if (_entries.TryGetValue(gathered, out var snapshot))
                {
                    affectedResourceTypes.Add(snapshot.ResourceType);
                    Remove(snapshot);
                }
            }
        }

        return affectedResourceTypes;
    }

    public IReadOnlyList<Gathered> CreateGroupedResources(GatheringResourceType resourceType)
    {
        return _resourcesByType
            .Where(x => x.Key.ResourceType == resourceType)
            .Select(x => CreateGathered(x.Key.UniqueName, x.Value, resourceType == GatheringResourceType.Fishing))
            .ToList();
    }

    public long GetResourceValue(GatheringResourceType resourceType)
    {
        return _resourceTypes.GetValueOrDefault(resourceType)?.TotalValue ?? 0;
    }

    public double GetResourceValuePerHour(GatheringResourceType resourceType)
    {
        return CalculateValuePerHour(_resourceTypes.GetValueOrDefault(resourceType));
    }

    public Gathered CreateMostGatheredResource()
    {
        var resource = _resources.Values.MaxBy(x => x.Metrics.TotalAmount);
        return resource == null
            ? null
            : CreateGathered(resource.UniqueName, resource.Metrics, resource.HasBeenFished);
    }

    public Gathered CreateMostGatheredCluster()
    {
        var cluster = _clusters.Values.MaxBy(x => x.Metrics.MiningProcesses);
        return cluster == null
            ? null
            : CreateGathered(string.Empty, cluster.Metrics, false, cluster.ClusterIndex);
    }

    public long TotalResources => _totals.TotalAmount;

    public long TotalGatheringProcesses => _totals.GatheringProcesses;

    public long TotalValue => _totals.TotalValue;

    public double TotalValuePerHour => CalculateValuePerHour(_totals);

    public long BestSingleGatheringValue => _marketValues.Count > 0 ? _marketValues.Last().Key : 0;

    public double GatheringDurationSeconds => _totals.GetDurationSeconds();

    public IReadOnlyList<GatheringResourceSummary> CreateResourceSummaries()
    {
        return _resources.Values
            .Where(x => !string.IsNullOrWhiteSpace(x.UniqueName))
            .Select(x => new GatheringResourceSummary
            {
                UniqueName = x.UniqueName,
                Item = ItemController.GetItemByUniqueName(x.UniqueName),
                TimesGathered = x.Metrics.GatheringProcesses,
                TotalAmount = x.Metrics.TotalAmount,
                TotalValue = x.Metrics.TotalValue,
                AverageValuePerGather = x.Metrics.GatheringProcesses > 0
                    ? (double) x.Metrics.TotalValue / x.Metrics.GatheringProcesses
                    : 0,
                GatheringDurationSeconds = x.Metrics.GetDurationSeconds(),
                TopLocation = x.LocationValues.Count > 0
                    ? x.LocationValues.MaxBy(location => location.Value.TotalValue).Key
                    : string.Empty
            })
            .ToList();
    }

    public IReadOnlyList<GatheringMapSummary> CreateMapSummaries()
    {
        return _maps.Values
            .Select(CreateMapSummary)
            .OrderByDescending(x => x.TotalValue)
            .ThenByDescending(x => x.TimesGathered)
            .ToList();
    }

    public IReadOnlyList<ResourceTypeTotals> CreateResourceTypeTotals()
    {
        return _resourceTypes
            .Where(x => x.Key != GatheringResourceType.Unknown)
            .Select(x => new ResourceTypeTotals(x.Key, x.Value.TotalAmount, x.Value.TotalValue))
            .ToList();
    }

    public IReadOnlyList<Gathered> GetRecentGatherings(int count)
    {
        return _entriesByTimestamp.GetLatest(count);
    }

    public double GetChartValue(
        GatheringResourceType resourceType,
        DateTime bucketStart,
        GatheringChartValueType chartValueType)
    {
        if (!_chartValues.TryGetValue(new ChartKey(resourceType, bucketStart), out var value))
        {
            return 0;
        }

        return chartValueType == GatheringChartValueType.ResourceSilverValue
            ? value.TotalValue
            : value.TotalAmount;
    }

    private bool AddIfIncluded(Gathered gathered)
    {
        return AddIfIncluded(gathered, out _);
    }

    private bool AddIfIncluded(Gathered gathered, out EntrySnapshot snapshot)
    {
        snapshot = EntrySnapshot.Create(gathered, _timeRangeUnit);
        if ((_sessionId.HasValue && gathered.SessionId != _sessionId.Value)
            || snapshot.TimestampLocal < _timeRangeStart
            || snapshot.TimestampLocal >= _timeRangeEnd)
        {
            return false;
        }

        _entries.Add(gathered, snapshot);
        _entriesByTimestamp.Add(snapshot.TimestampUtc, gathered);
        _totals.Add(snapshot);
        AddMarketValue(snapshot.TotalValue);

        var resourceKey = new ResourceKey(snapshot.ResourceType, snapshot.UniqueName);
        var uniqueName = snapshot.UniqueName;
        var mapKey = snapshot.MapKey;
        var clusterIndex = snapshot.ClusterIndex;
        GetOrAdd(_resourcesByType, resourceKey, static () => new MetricAccumulator(false)).Add(snapshot);
        GetOrAdd(_resources, uniqueName, () => new ResourceAccumulator(uniqueName)).Add(snapshot);
        GetOrAdd(_maps, mapKey, () => new MapAccumulator(mapKey)).Add(snapshot);
        GetOrAdd(_clusters, clusterIndex, () => new ClusterAccumulator(clusterIndex)).Metrics.Add(snapshot);
        GetOrAdd(_resourceTypes, snapshot.ResourceType, static () => new MetricAccumulator(true)).Add(snapshot);
        GetOrAdd(_chartValues, new ChartKey(snapshot.ResourceType, snapshot.ChartBucketStart)).Add(snapshot);
        return true;
    }

    private void Remove(EntrySnapshot snapshot)
    {
        _entries.Remove(snapshot.Source);
        _entriesByTimestamp.Remove(snapshot.TimestampUtc, snapshot.Source);
        _totals.Remove(snapshot);
        RemoveMarketValue(snapshot.TotalValue);

        RemoveFromDictionary(_resourcesByType, new ResourceKey(snapshot.ResourceType, snapshot.UniqueName), snapshot);
        RemoveFromDictionary(_resources, snapshot.UniqueName, snapshot);
        RemoveFromDictionary(_maps, snapshot.MapKey, snapshot);
        RemoveFromDictionary(_clusters, snapshot.ClusterIndex, snapshot);
        RemoveFromDictionary(_resourceTypes, snapshot.ResourceType, snapshot);
        RemoveFromDictionary(_chartValues, new ChartKey(snapshot.ResourceType, snapshot.ChartBucketStart), snapshot);
    }

    private GatheringMapSummary CreateMapSummary(MapAccumulator map)
    {
        var mapName = ClusterController.ComposingMapInfoString(
                map.Key.ClusterIndex,
                map.Key.MapType,
                map.Key.InstanceName)
            .Trim();
        var mostGatheredResourceName = map.Resources
            .Where(x => !string.IsNullOrWhiteSpace(x.Key))
            .OrderByDescending(x => x.Value.TotalAmount)
            .ThenByDescending(x => x.Value.TotalValue)
            .Select(x => x.Key)
            .FirstOrDefault() ?? string.Empty;

        return new GatheringMapSummary
        {
            Name = string.IsNullOrWhiteSpace(mapName)
                ? LocalizationController.Translation("NO_DATA")
                : mapName,
            ClusterIndex = map.Key.ClusterIndex,
            TimesGathered = map.Metrics.GatheringProcesses,
            TotalValue = map.Metrics.TotalValue,
            ResourceTypeCount = map.ResourceTypes.Count,
            GatheringDurationSeconds = map.Metrics.GetDurationSeconds(),
            ClusterType = WorldData.GetClusterTypeByIndex(map.Key.ClusterIndex),
            MostGatheredResource = string.IsNullOrWhiteSpace(mostGatheredResourceName)
                ? null
                : ItemController.GetItemByUniqueName(mostGatheredResourceName)
        };
    }

    private Gathered CreateGathered(
        string uniqueName,
        MetricAccumulator metrics,
        bool hasBeenFished,
        string clusterIndex = "")
    {
        var estimatedMarketValueInternal = metrics.TotalAmount > 0
            ? metrics.WeightedMarketValueInternal / metrics.TotalAmount
            : 0;

        return new Gathered
        {
            UniqueName = uniqueName,
            ClusterIndex = clusterIndex,
            EstimatedMarketValue = FixPoint.FromInternalValue(estimatedMarketValueInternal),
            GainedStandardAmount = ToInt32(metrics.StandardAmount),
            GainedBonusAmount = ToInt32(metrics.BonusAmount),
            GainedPremiumBonusAmount = ToInt32(metrics.PremiumBonusAmount),
            GainedTotalAmount = ToInt32(metrics.TotalAmount),
            GainedFame = ToInt32(metrics.GainedFame),
            MiningProcesses = ToInt32(metrics.MiningProcesses),
            HasBeenFished = hasBeenFished
        };
    }

    private static int ToInt32(long value)
    {
        return checked((int) value);
    }

    private static double CalculateValuePerHour(MetricAccumulator metrics)
    {
        if (metrics == null || metrics.EntryCount == 0)
        {
            return 0;
        }

        var durationInSeconds = Math.Max(3600d, metrics.GetDurationSeconds());
        return (double) metrics.TotalValue / durationInSeconds * 3600d;
    }

    private void AddMarketValue(long value)
    {
        _marketValues[value] = _marketValues.GetValueOrDefault(value) + 1;
    }

    private void RemoveMarketValue(long value)
    {
        var count = _marketValues[value] - 1;
        if (count == 0)
        {
            _marketValues.Remove(value);
        }
        else
        {
            _marketValues[value] = count;
        }
    }

    private static TValue GetOrAdd<TKey, TValue>(
        IDictionary<TKey, TValue> dictionary,
        TKey key,
        Func<TValue> valueFactory)
    {
        if (!dictionary.TryGetValue(key, out var value))
        {
            value = valueFactory();
            dictionary.Add(key, value);
        }

        return value;
    }

    private static ChartAccumulator GetOrAdd(
        IDictionary<ChartKey, ChartAccumulator> dictionary,
        ChartKey key)
    {
        return GetOrAdd(dictionary, key, static () => new ChartAccumulator());
    }

    private static void RemoveFromDictionary<TKey, TValue>(
        IDictionary<TKey, TValue> dictionary,
        TKey key,
        EntrySnapshot snapshot)
        where TValue : IEntryAccumulator
    {
        var accumulator = dictionary[key];
        accumulator.Remove(snapshot);
        if (accumulator.EntryCount == 0)
        {
            dictionary.Remove(key);
        }
    }

    private void Clear()
    {
        _entries.Clear();
        _entriesByTimestamp.Clear();
        _resourcesByType.Clear();
        _resources.Clear();
        _maps.Clear();
        _clusters.Clear();
        _resourceTypes.Clear();
        _chartValues.Clear();
        _marketValues.Clear();
        _totals.Clear();
        IsInitialized = false;
    }

    public readonly record struct ResourceTypeTotals(
        GatheringResourceType ResourceType,
        long TotalAmount,
        long TotalValue);

    private interface IEntryAccumulator
    {
        int EntryCount { get; }
        void Remove(EntrySnapshot snapshot);
    }

    private sealed class MetricAccumulator : IEntryAccumulator
    {
        private readonly bool _trackTimestamps;
        private readonly TimestampCounter _timestamps = new();

        public MetricAccumulator(bool trackTimestamps)
        {
            _trackTimestamps = trackTimestamps;
        }

        public long StandardAmount { get; private set; }
        public long BonusAmount { get; private set; }
        public long PremiumBonusAmount { get; private set; }
        public long TotalAmount { get; private set; }
        public long GainedFame { get; private set; }
        public long MiningProcesses { get; private set; }
        public long GatheringProcesses { get; private set; }
        public long TotalValue { get; private set; }
        public long WeightedMarketValueInternal { get; private set; }
        public int EntryCount { get; private set; }

        public void Add(EntrySnapshot snapshot)
        {
            StandardAmount += snapshot.StandardAmount;
            BonusAmount += snapshot.BonusAmount;
            PremiumBonusAmount += snapshot.PremiumBonusAmount;
            TotalAmount += snapshot.TotalAmount;
            GainedFame += snapshot.GainedFame;
            MiningProcesses += snapshot.MiningProcesses;
            GatheringProcesses += snapshot.GatheringProcesses;
            TotalValue += snapshot.TotalValue;
            WeightedMarketValueInternal += snapshot.EstimatedMarketValueInternal * snapshot.TotalAmount;
            EntryCount++;
            if (_trackTimestamps)
            {
                _timestamps.Add(snapshot.TimestampUtc);
            }
        }

        public void Remove(EntrySnapshot snapshot)
        {
            StandardAmount -= snapshot.StandardAmount;
            BonusAmount -= snapshot.BonusAmount;
            PremiumBonusAmount -= snapshot.PremiumBonusAmount;
            TotalAmount -= snapshot.TotalAmount;
            GainedFame -= snapshot.GainedFame;
            MiningProcesses -= snapshot.MiningProcesses;
            GatheringProcesses -= snapshot.GatheringProcesses;
            TotalValue -= snapshot.TotalValue;
            WeightedMarketValueInternal -= snapshot.EstimatedMarketValueInternal * snapshot.TotalAmount;
            EntryCount--;
            if (_trackTimestamps)
            {
                _timestamps.Remove(snapshot.TimestampUtc);
            }
        }

        public double GetDurationSeconds()
        {
            return _trackTimestamps ? _timestamps.GetDurationSeconds() : 0;
        }

        public void Clear()
        {
            StandardAmount = 0;
            BonusAmount = 0;
            PremiumBonusAmount = 0;
            TotalAmount = 0;
            GainedFame = 0;
            MiningProcesses = 0;
            GatheringProcesses = 0;
            TotalValue = 0;
            WeightedMarketValueInternal = 0;
            EntryCount = 0;
            _timestamps.Clear();
        }
    }

    private sealed class ResourceAccumulator(string uniqueName) : IEntryAccumulator
    {
        private int _fishingEntryCount;

        public string UniqueName { get; } = uniqueName;
        public MetricAccumulator Metrics { get; } = new(true);
        public Dictionary<string, AmountValueAccumulator> LocationValues { get; } = [];
        public bool HasBeenFished => _fishingEntryCount > 0;
        public int EntryCount => Metrics.EntryCount;

        public void Add(EntrySnapshot snapshot)
        {
            Metrics.Add(snapshot);
            GetOrAdd(
                    LocationValues,
                    snapshot.ClusterUniqueName,
                    static () => new AmountValueAccumulator())
                .Add(snapshot);
            if (snapshot.ResourceType == GatheringResourceType.Fishing)
            {
                _fishingEntryCount++;
            }
        }

        public void Remove(EntrySnapshot snapshot)
        {
            Metrics.Remove(snapshot);
            var location = LocationValues[snapshot.ClusterUniqueName];
            location.Remove(snapshot);
            if (location.EntryCount == 0)
            {
                LocationValues.Remove(snapshot.ClusterUniqueName);
            }

            if (snapshot.ResourceType == GatheringResourceType.Fishing)
            {
                _fishingEntryCount--;
            }
        }
    }

    private sealed class MapAccumulator(MapKey key) : IEntryAccumulator
    {
        public MapKey Key { get; } = key;
        public MetricAccumulator Metrics { get; } = new(true);
        public Dictionary<string, AmountValueAccumulator> Resources { get; } = [];
        public Dictionary<GatheringResourceType, int> ResourceTypes { get; } = [];
        public int EntryCount => Metrics.EntryCount;

        public void Add(EntrySnapshot snapshot)
        {
            Metrics.Add(snapshot);
            GetOrAdd(Resources, snapshot.UniqueName, static () => new AmountValueAccumulator()).Add(snapshot);
            if (snapshot.ResourceType != GatheringResourceType.Unknown)
            {
                ResourceTypes[snapshot.ResourceType] = ResourceTypes.GetValueOrDefault(snapshot.ResourceType) + 1;
            }
        }

        public void Remove(EntrySnapshot snapshot)
        {
            Metrics.Remove(snapshot);
            var resource = Resources[snapshot.UniqueName];
            resource.Remove(snapshot);
            if (resource.EntryCount == 0)
            {
                Resources.Remove(snapshot.UniqueName);
            }

            if (snapshot.ResourceType == GatheringResourceType.Unknown)
            {
                return;
            }

            var resourceTypeCount = ResourceTypes[snapshot.ResourceType] - 1;
            if (resourceTypeCount == 0)
            {
                ResourceTypes.Remove(snapshot.ResourceType);
            }
            else
            {
                ResourceTypes[snapshot.ResourceType] = resourceTypeCount;
            }
        }
    }

    private sealed class ClusterAccumulator(string clusterIndex) : IEntryAccumulator
    {
        public string ClusterIndex { get; } = clusterIndex;
        public MetricAccumulator Metrics { get; } = new(false);
        public int EntryCount => Metrics.EntryCount;

        public void Remove(EntrySnapshot snapshot)
        {
            Metrics.Remove(snapshot);
        }
    }

    private sealed class ChartAccumulator : IEntryAccumulator
    {
        public long TotalAmount { get; private set; }
        public long TotalValue { get; private set; }
        public int EntryCount { get; private set; }

        public void Add(EntrySnapshot snapshot)
        {
            TotalAmount += snapshot.TotalAmount;
            TotalValue += snapshot.TotalValue;
            EntryCount++;
        }

        public void Remove(EntrySnapshot snapshot)
        {
            TotalAmount -= snapshot.TotalAmount;
            TotalValue -= snapshot.TotalValue;
            EntryCount--;
        }
    }

    private sealed class AmountValueAccumulator
    {
        public long TotalAmount { get; private set; }
        public long TotalValue { get; private set; }
        public int EntryCount { get; private set; }

        public void Add(EntrySnapshot snapshot)
        {
            TotalAmount += snapshot.TotalAmount;
            TotalValue += snapshot.TotalValue;
            EntryCount++;
        }

        public void Remove(EntrySnapshot snapshot)
        {
            TotalAmount -= snapshot.TotalAmount;
            TotalValue -= snapshot.TotalValue;
            EntryCount--;
        }
    }

    private sealed class TimestampCounter
    {
        private readonly SortedDictionary<long, int> _timestamps = [];

        public void Add(long timestampUtc)
        {
            _timestamps[timestampUtc] = _timestamps.GetValueOrDefault(timestampUtc) + 1;
        }

        public void Remove(long timestampUtc)
        {
            var count = _timestamps[timestampUtc] - 1;
            if (count == 0)
            {
                _timestamps.Remove(timestampUtc);
            }
            else
            {
                _timestamps[timestampUtc] = count;
            }
        }

        public double GetDurationSeconds()
        {
            return _timestamps.Count > 1
                ? TimeSpan.FromTicks(_timestamps.Last().Key - _timestamps.First().Key).TotalSeconds
                : 0;
        }

        public void Clear()
        {
            _timestamps.Clear();
        }
    }

    private sealed class TimestampIndex
    {
        private readonly SortedDictionary<long, HashSet<Gathered>> _entries = [];

        public void Add(long timestampUtc, Gathered gathered)
        {
            GetOrAdd(
                    _entries,
                    timestampUtc,
                    static () => new HashSet<Gathered>(ReferenceEqualityComparer.Instance))
                .Add(gathered);
        }

        public void Remove(long timestampUtc, Gathered gathered)
        {
            var entries = _entries[timestampUtc];
            entries.Remove(gathered);
            if (entries.Count == 0)
            {
                _entries.Remove(timestampUtc);
            }
        }

        public bool TryGetFirst(out long timestampUtc, out IReadOnlyCollection<Gathered> gatheredEntries)
        {
            if (_entries.Count == 0)
            {
                timestampUtc = 0;
                gatheredEntries = [];
                return false;
            }

            var first = _entries.First();
            timestampUtc = first.Key;
            gatheredEntries = first.Value;
            return true;
        }

        public IReadOnlyList<Gathered> GetLatest(int count)
        {
            var result = new List<Gathered>(count);
            foreach (var entry in _entries.Reverse())
            {
                foreach (var gathered in entry.Value)
                {
                    result.Add(gathered);
                    if (result.Count == count)
                    {
                        return result;
                    }
                }
            }

            return result;
        }

        public void Clear()
        {
            _entries.Clear();
        }
    }

    private sealed record EntrySnapshot(
        Gathered Source,
        string UniqueName,
        GatheringResourceType ResourceType,
        long TimestampUtc,
        DateTime TimestampLocal,
        DateTime ChartBucketStart,
        int StandardAmount,
        int BonusAmount,
        int PremiumBonusAmount,
        int TotalAmount,
        int GainedFame,
        int MiningProcesses,
        int GatheringProcesses,
        long TotalValue,
        long EstimatedMarketValueInternal,
        string ClusterIndex,
        string ClusterUniqueName,
        MapKey MapKey)
    {
        public static EntrySnapshot Create(Gathered gathered, GatheringTimeRangeUnit timeRangeUnit)
        {
            var resourceType = GetResourceType(gathered);
            var timestampLocal = gathered.TimestampDateTimeUtc.ToLocalTime();
            var clusterIndex = gathered.ClusterIndex ?? string.Empty;
            var instanceName = gathered.InstanceName ?? string.Empty;

            return new EntrySnapshot(
                gathered,
                gathered.UniqueName ?? string.Empty,
                resourceType,
                gathered.TimestampUtc,
                timestampLocal,
                AlignTimestampToBucketStart(timestampLocal, timeRangeUnit),
                gathered.GainedStandardAmount,
                gathered.GainedBonusAmount,
                gathered.GainedPremiumBonusAmount,
                gathered.GainedTotalAmount,
                gathered.GainedFame,
                gathered.MiningProcesses,
                gathered.HasBeenFished ? 1 : gathered.MiningProcesses,
                gathered.TotalMarketValue.IntegerValue,
                gathered.EstimatedMarketValue.InternalValue,
                clusterIndex,
                gathered.ClusterUniqueName,
                new MapKey(clusterIndex, gathered.MapType, instanceName));
        }
    }

    private readonly record struct ResourceKey(GatheringResourceType ResourceType, string UniqueName);

    private readonly record struct MapKey(string ClusterIndex, MapType MapType, string InstanceName);

    private readonly record struct ChartKey(GatheringResourceType ResourceType, DateTime BucketStart);

    private static GatheringResourceType GetResourceType(Gathered gathered)
    {
        if (gathered.HasBeenFished)
        {
            return GatheringResourceType.Fishing;
        }

        return gathered.Item?.FullItemInformation?.ShopSubCategory2?.ToLowerInvariant() switch
        {
            "wood" => GatheringResourceType.Wood,
            "fiber" => GatheringResourceType.Fiber,
            "hide" => GatheringResourceType.Hide,
            "ore" => GatheringResourceType.Ore,
            "rock" => GatheringResourceType.Rock,
            _ => GatheringResourceType.Unknown
        };
    }

    private static DateTime AlignTimestampToBucketStart(DateTime timestamp, GatheringTimeRangeUnit unit)
    {
        return unit switch
        {
            GatheringTimeRangeUnit.Minute => new DateTime(timestamp.Year, timestamp.Month, timestamp.Day, timestamp.Hour, timestamp.Minute, 0),
            GatheringTimeRangeUnit.Hour => new DateTime(timestamp.Year, timestamp.Month, timestamp.Day, timestamp.Hour, 0, 0),
            GatheringTimeRangeUnit.Day => timestamp.Date,
            _ => timestamp.Date
        };
    }
}