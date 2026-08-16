using System;
using System.Collections.Generic;

namespace StatisticsAnalysisTool.DamageMeter;

public sealed class CombatMobPlayerDamageStats
{
    private readonly Dictionary<int, long> _damageBySpell = [];

    public Guid PlayerGuid { get; init; }
    public string PlayerName { get; private set; } = string.Empty;
    public long Damage { get; private set; }
    public IReadOnlyDictionary<int, long> DamageBySpell => _damageBySpell;

    internal void RecordDamage(string playerName, int causingSpellIndex, long value)
    {
        if (value <= 0)
        {
            return;
        }

        if (!string.IsNullOrWhiteSpace(playerName))
        {
            PlayerName = playerName;
        }

        Damage += value;
        _damageBySpell[causingSpellIndex] = _damageBySpell.GetValueOrDefault(causingSpellIndex) + value;
    }

    internal CombatMobPlayerDamageStats Clone()
    {
        var clone = new CombatMobPlayerDamageStats
        {
            PlayerGuid = PlayerGuid,
            PlayerName = PlayerName,
            Damage = Damage
        };

        foreach (var damageBySpell in _damageBySpell)
        {
            clone._damageBySpell[damageBySpell.Key] = damageBySpell.Value;
        }

        return clone;
    }
}
