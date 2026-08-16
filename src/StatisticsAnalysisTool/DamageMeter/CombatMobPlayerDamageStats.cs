using System;
using System.Collections.Generic;

namespace StatisticsAnalysisTool.DamageMeter;

public sealed class CombatMobPlayerDamageStats
{
    private readonly Dictionary<int, long> _damageBySpell = [];
    private readonly Dictionary<int, int> _hitCountBySpell = [];

    public Guid PlayerGuid { get; init; }
    public string PlayerName { get; private set; } = string.Empty;
    public long Damage { get; private set; }
    public DateTime FirstDamageTime { get; private set; }
    public DateTime LastDamageTime { get; private set; }
    public IReadOnlyDictionary<int, long> DamageBySpell => _damageBySpell;
    public IReadOnlyDictionary<int, int> HitCountBySpell => _hitCountBySpell;

    internal void RecordDamage(string playerName, int causingSpellIndex, long value, DateTime timestamp)
    {
        if (value <= 0)
        {
            return;
        }

        if (!string.IsNullOrWhiteSpace(playerName))
        {
            PlayerName = playerName;
        }

        if (FirstDamageTime == default)
        {
            FirstDamageTime = timestamp;
        }

        LastDamageTime = timestamp;
        Damage += value;
        _damageBySpell[causingSpellIndex] = _damageBySpell.GetValueOrDefault(causingSpellIndex) + value;
        _hitCountBySpell[causingSpellIndex] = _hitCountBySpell.GetValueOrDefault(causingSpellIndex) + 1;
    }

    internal CombatMobPlayerDamageStats Clone()
    {
        var clone = new CombatMobPlayerDamageStats
        {
            PlayerGuid = PlayerGuid,
            PlayerName = PlayerName,
            Damage = Damage,
            FirstDamageTime = FirstDamageTime,
            LastDamageTime = LastDamageTime
        };

        foreach (var damageBySpell in _damageBySpell)
        {
            clone._hitCountBySpell[damageBySpell.Key] = _hitCountBySpell.GetValueOrDefault(damageBySpell.Key);
            clone._damageBySpell[damageBySpell.Key] = damageBySpell.Value;
        }

        return clone;
    }
}
