using System;
using System.Collections.Generic;

namespace StatisticsAnalysisTool.DamageMeter;

internal sealed class RollingDamageWindow
{
    private readonly TimeSpan _duration;
    private readonly Queue<DamageStatsEvent> _events = [];
    private long _currentDamage;

    public RollingDamageWindow(TimeSpan duration)
    {
        if (duration <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(duration));
        }

        _duration = duration;
    }

    public long HighestDamage { get; private set; }

    public void AddDamage(DamageStatsEvent damageEvent)
    {
        _events.Enqueue(damageEvent);
        _currentDamage += damageEvent.Value;

        while (_events.TryPeek(out var oldestEvent) && damageEvent.Timestamp - oldestEvent.Timestamp > _duration)
        {
            _events.Dequeue();
            _currentDamage -= oldestEvent.Value;
        }

        HighestDamage = Math.Max(HighestDamage, _currentDamage);
    }
}