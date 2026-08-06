using System;
using System.Collections.Generic;
using System.Linq;

namespace StatisticsAnalysisTool.Common;

public sealed class ItemRefreshCooldownTracker
{
    private static readonly TimeSpan RefreshCooldown = TimeSpan.FromSeconds(30);
    private readonly Dictionary<string, DateTimeOffset> _cooldownEndTimes = new(StringComparer.Ordinal);
    private readonly object _syncRoot = new();

    public bool TryStart(string itemUniqueName, out TimeSpan remainingCooldown)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(itemUniqueName);

        lock (_syncRoot)
        {
            var now = DateTimeOffset.UtcNow;
            RemoveExpiredCooldowns(now);

            if (_cooldownEndTimes.TryGetValue(itemUniqueName, out var cooldownEndTime))
            {
                remainingCooldown = cooldownEndTime - now;
                return false;
            }

            _cooldownEndTimes[itemUniqueName] = now.Add(RefreshCooldown);
            remainingCooldown = RefreshCooldown;
            return true;
        }
    }

    public TimeSpan GetRemainingCooldown(string itemUniqueName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(itemUniqueName);

        lock (_syncRoot)
        {
            var now = DateTimeOffset.UtcNow;

            if (!_cooldownEndTimes.TryGetValue(itemUniqueName, out var cooldownEndTime))
            {
                return TimeSpan.Zero;
            }

            if (cooldownEndTime <= now)
            {
                _cooldownEndTimes.Remove(itemUniqueName);
                return TimeSpan.Zero;
            }

            return cooldownEndTime - now;
        }
    }

    private void RemoveExpiredCooldowns(DateTimeOffset now)
    {
        foreach (var itemUniqueName in _cooldownEndTimes.Where(x => x.Value <= now).Select(x => x.Key).ToList())
        {
            _cooldownEndTimes.Remove(itemUniqueName);
        }
    }
}