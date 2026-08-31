using System;
using System.Collections.Generic;
using System.Linq;

namespace StatisticsAnalysisTool.Crafting;

internal static class LossExplorerHistoryController
{
    public const int RetainedDayCount = 7;

    private const int MaximumRememberedEventCount = 2500;

    public static LossExplorerCache PrepareCache(LossExplorerCache cache, DateTime utcNow)
    {
        if (cache == null)
        {
            return CreateCache(utcNow);
        }

        cache.CreatedUtc = cache.CreatedUtc > DateTime.MinValue ? cache.CreatedUtc : utcNow;
        cache.ObservedDays ??= [];
        cache.DailyEventCounts ??= [];
        cache.DailyItems ??= [];
        cache.RecentProcessedEvents ??= [];
        cache.Items ??= [];

        if (cache.ObservedDays.Count == 0)
        {
            cache.ObservedDays = cache.DailyItems
                .Select(x => x.Day)
                .Concat(cache.RecentProcessedEvents
                    .Where(x => x.TimeStampUtc > DateTime.MinValue)
                    .Select(x => DateOnly.FromDateTime(x.TimeStampUtc.ToLocalTime())))
                .Distinct()
                .ToList();
        }

        if (cache.DailyEventCounts.Count == 0)
        {
            cache.DailyEventCounts = CreateDailyEventCounts(cache.RecentProcessedEvents);
        }

        NormalizeHistory(cache, utcNow);
        RebuildAverageItems(cache);
        return cache;
    }

    public static void ApplyEvents(
        LossExplorerCache cache,
        IReadOnlyCollection<LossExplorerEvent> events,
        DateTime utcNow)
    {
        var itemsByKey = cache.DailyItems.ToDictionary(
            x => CreateDailyItemKey(x.Day, x.ItemUniqueName, x.QualityLevel),
            StringComparer.Ordinal);
        var observedDays = cache.ObservedDays.ToHashSet();
        var eventCountsByDay = cache.DailyEventCounts
            .ToDictionary(x => x.Day, x => x.EventCount);
        var processedEvents = cache.RecentProcessedEvents.ToDictionary(x => x.EventId);
        var earliestRetainedDay = GetEarliestRetainedDay(utcNow);

        foreach (var lossEvent in events)
        {
            var eventDay = DateOnly.FromDateTime(lossEvent.TimeStampUtc.ToLocalTime());
            if (eventDay < earliestRetainedDay)
            {
                continue;
            }

            if (processedEvents.ContainsKey(lossEvent.EventId))
            {
                continue;
            }

            observedDays.Add(eventDay);
            eventCountsByDay[eventDay] = eventCountsByDay.GetValueOrDefault(eventDay) + 1;
            processedEvents[lossEvent.EventId] = new LossExplorerProcessedEvent
            {
                EventId = lossEvent.EventId,
                TimeStampUtc = lossEvent.TimeStampUtc
            };
            AggregateItems(itemsByKey, eventDay, lossEvent.EquipmentItems, true);
            AggregateItems(itemsByKey, eventDay, lossEvent.InventoryItems, false);
        }

        cache.ObservedDays = observedDays.ToList();
        cache.DailyEventCounts = eventCountsByDay
            .Select(x => new LossExplorerDailyEventCount
            {
                Day = x.Key,
                EventCount = x.Value
            })
            .ToList();
        cache.DailyItems = itemsByKey.Values.ToList();
        cache.RecentProcessedEvents = processedEvents.Values.ToList();
        NormalizeHistory(cache, utcNow);
        RebuildAverageItems(cache);
    }

    public static DateOnly GetEarliestRetainedDay(DateTime utcNow)
    {
        return DateOnly.FromDateTime(utcNow.ToLocalTime()).AddDays(-(RetainedDayCount - 1));
    }

    public static bool IsValidCache(LossExplorerCache cache)
    {
        return cache != null
               && cache.CreatedUtc > DateTime.MinValue
               && cache.ObservedDays != null
               && cache.ObservedDays.Count <= RetainedDayCount
               && cache.DailyEventCounts != null
               && cache.DailyItems != null
               && cache.RecentProcessedEvents != null
               && cache.Items != null;
    }

    private static List<LossExplorerDailyEventCount> CreateDailyEventCounts(
        IEnumerable<LossExplorerProcessedEvent> processedEvents)
    {
        return (processedEvents ?? [])
            .Where(x => x.EventId > 0 && x.TimeStampUtc > DateTime.MinValue)
            .GroupBy(x => DateOnly.FromDateTime(x.TimeStampUtc.ToLocalTime()))
            .Select(x => new LossExplorerDailyEventCount
            {
                Day = x.Key,
                EventCount = x.Select(y => y.EventId).Distinct().LongCount()
            })
            .OrderBy(x => x.Day)
            .ToList();
    }

    private static void NormalizeHistory(LossExplorerCache cache, DateTime utcNow)
    {
        var earliestRetainedDay = GetEarliestRetainedDay(utcNow);
        var retainedDays = cache.ObservedDays
            .Where(x => x >= earliestRetainedDay)
            .Distinct()
            .OrderByDescending(x => x)
            .Take(RetainedDayCount)
            .OrderBy(x => x)
            .ToList();
        var retainedDaySet = retainedDays.ToHashSet();

        cache.ObservedDays = retainedDays;
        cache.DailyEventCounts = cache.DailyEventCounts
            .Where(x => retainedDaySet.Contains(x.Day) && x.EventCount > 0)
            .GroupBy(x => x.Day)
            .Select(x => new LossExplorerDailyEventCount
            {
                Day = x.Key,
                EventCount = x.Sum(y => y.EventCount)
            })
            .OrderBy(x => x.Day)
            .ToList();
        cache.DailyItems = cache.DailyItems
            .Where(x => retainedDaySet.Contains(x.Day)
                        && !string.IsNullOrWhiteSpace(x.ItemUniqueName)
                        && (x.EquipmentQuantity > 0 || x.InventoryQuantity > 0))
            .GroupBy(x => CreateDailyItemKey(x.Day, x.ItemUniqueName, x.QualityLevel), StringComparer.Ordinal)
            .Select(x => new LossExplorerDailyItem
            {
                Day = x.First().Day,
                ItemUniqueName = x.First().ItemUniqueName,
                QualityLevel = x.First().QualityLevel,
                EquipmentQuantity = x.Sum(y => y.EquipmentQuantity),
                InventoryQuantity = x.Sum(y => y.InventoryQuantity)
            })
            .OrderBy(x => x.Day)
            .ThenBy(x => x.ItemUniqueName, StringComparer.Ordinal)
            .ThenBy(x => x.QualityLevel)
            .ToList();
        cache.RecentProcessedEvents = cache.RecentProcessedEvents
            .Where(x => x.EventId > 0
                        && x.TimeStampUtc > DateTime.MinValue
                        && DateOnly.FromDateTime(x.TimeStampUtc.ToLocalTime()) >= earliestRetainedDay)
            .GroupBy(x => x.EventId)
            .Select(x => x.OrderByDescending(y => y.TimeStampUtc).First())
            .OrderByDescending(x => x.TimeStampUtc)
            .ThenByDescending(x => x.EventId)
            .Take(MaximumRememberedEventCount)
            .ToList();
    }

    private static void RebuildAverageItems(LossExplorerCache cache)
    {
        var pricesByKey = cache.Items
            .GroupBy(x => CreateItemKey(x.ItemUniqueName, x.QualityLevel), StringComparer.Ordinal)
            .ToDictionary(x => x.Key, x => x.First(), StringComparer.Ordinal);
        var observedDayCount = cache.ObservedDays.Count;

        if (observedDayCount == 0)
        {
            cache.Items = [];
            return;
        }

        cache.Items = cache.DailyItems
            .GroupBy(x => CreateItemKey(x.ItemUniqueName, x.QualityLevel), StringComparer.Ordinal)
            .Select(x =>
            {
                var first = x.First();
                pricesByKey.TryGetValue(x.Key, out var previousItem);
                return new LossExplorerCachedItem
                {
                    ItemUniqueName = first.ItemUniqueName,
                    QualityLevel = first.QualityLevel,
                    EquipmentQuantity = x.Sum(y => y.EquipmentQuantity) / (decimal) observedDayCount,
                    InventoryQuantity = x.Sum(y => y.InventoryQuantity) / (decimal) observedDayCount,
                    UnitValue = previousItem?.UnitValue ?? 0,
                    HasPrice = previousItem?.HasPrice ?? false
                };
            })
            .OrderBy(x => x.ItemUniqueName, StringComparer.Ordinal)
            .ThenBy(x => x.QualityLevel)
            .ToList();
    }

    private static void AggregateItems(
        IDictionary<string, LossExplorerDailyItem> itemsByKey,
        DateOnly day,
        IEnumerable<LossExplorerEventItem> eventItems,
        bool isEquipment)
    {
        foreach (var eventItem in eventItems ?? [])
        {
            if (string.IsNullOrWhiteSpace(eventItem.ItemUniqueName) || eventItem.Count <= 0)
            {
                continue;
            }

            var key = CreateDailyItemKey(day, eventItem.ItemUniqueName, eventItem.QualityLevel);
            if (!itemsByKey.TryGetValue(key, out var dailyItem))
            {
                dailyItem = new LossExplorerDailyItem
                {
                    Day = day,
                    ItemUniqueName = eventItem.ItemUniqueName,
                    QualityLevel = eventItem.QualityLevel
                };
                itemsByKey[key] = dailyItem;
            }

            if (isEquipment)
            {
                dailyItem.EquipmentQuantity += eventItem.Count;
            }
            else
            {
                dailyItem.InventoryQuantity += eventItem.Count;
            }
        }
    }

    private static string CreateDailyItemKey(DateOnly day, string itemUniqueName, int qualityLevel)
    {
        return $"{day:yyyy-MM-dd}\u001f{CreateItemKey(itemUniqueName, qualityLevel)}";
    }

    private static string CreateItemKey(string itemUniqueName, int qualityLevel)
    {
        return $"{itemUniqueName}\u001f{qualityLevel}";
    }

    private static LossExplorerCache CreateCache(DateTime utcNow)
    {
        return new LossExplorerCache
        {
            CreatedUtc = utcNow
        };
    }
}
