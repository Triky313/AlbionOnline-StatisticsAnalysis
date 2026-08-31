using Serilog;
using StatisticsAnalysisTool.Enumerations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Crafting;

internal sealed class LossExplorerEventLoader
{
    private const int EventPageSize = 51;
    private const int MaximumEventOffset = 1000;
    private const int ConsecutiveKnownPageLimit = 2;
    private readonly LossExplorerApiClient _apiClient = new();

    public async Task<IReadOnlyList<LossExplorerEvent>> LoadNewEventsAsync(
        LossExplorerCache cache,
        ServerLocation serverLocation,
        Action<int> eventPageLoaded,
        CancellationToken cancellationToken)
    {
        var knownEventIds = cache.RecentProcessedEvents.Select(x => x.EventId).ToHashSet();
        var newEventIds = new HashSet<long>();
        var newEvents = new List<LossExplorerEvent>();
        var earliestRetainedDay = LossExplorerHistoryController.GetEarliestRetainedDay(DateTime.UtcNow);
        var consecutiveKnownPages = 0;
        var loadedPageCount = 0;
        var offset = 0;

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var requestOffset = Math.Min(offset, MaximumEventOffset);
            var isLastAvailableApiPage = offset >= MaximumEventOffset;
            var events = await _apiClient.GetEventsPageAsync(
                serverLocation,
                EventPageSize,
                requestOffset,
                cancellationToken).ConfigureAwait(false);

            if (events == null)
            {
                throw new InvalidOperationException("Albion Online kill events could not be loaded.");
            }

            loadedPageCount++;
            eventPageLoaded?.Invoke(loadedPageCount);
            if (events.Count == 0)
            {
                break;
            }

            var pageContainsNewEvents = false;
            var reachedRetentionBoundary = false;
            foreach (var lossEvent in events)
            {
                var eventDay = DateOnly.FromDateTime(lossEvent.TimeStampUtc.ToLocalTime());
                if (eventDay < earliestRetainedDay)
                {
                    reachedRetentionBoundary = true;
                    continue;
                }

                if (knownEventIds.Contains(lossEvent.EventId) || !newEventIds.Add(lossEvent.EventId))
                {
                    continue;
                }

                pageContainsNewEvents = true;
                newEvents.Add(lossEvent);
            }

            consecutiveKnownPages = pageContainsNewEvents ? 0 : consecutiveKnownPages + 1;
            offset = requestOffset + events.Count;

            if (reachedRetentionBoundary
                || events.Count < EventPageSize
                || consecutiveKnownPages >= ConsecutiveKnownPageLimit
                || isLastAvailableApiPage)
            {
                if (isLastAvailableApiPage && !reachedRetentionBoundary && consecutiveKnownPages < ConsecutiveKnownPageLimit)
                {
                    Log.Information(
                        "Loss Explorer reached the Albion Online events API pagination limit. MaximumOffset={MaximumOffset}",
                        MaximumEventOffset);
                }

                break;
            }
        }

        return newEvents;
    }
}