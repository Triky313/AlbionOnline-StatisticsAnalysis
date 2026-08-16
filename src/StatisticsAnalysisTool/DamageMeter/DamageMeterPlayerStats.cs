using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Models.NetworkModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StatisticsAnalysisTool.DamageMeter;

public sealed class DamageMeterPlayerStats
{
    private readonly List<ActionInterval> _combatTimes = [];

    internal object SyncRoot { get; } = new();

    public DateTime? CombatStart { get; set; }
    public TimeSpan CombatTime { get; private set; } = new(1);
    public long Damage { get; set; }
    public long Heal { get; set; }
    public long TakenDamage { get; set; }
    public long Overhealed { get; set; }
    public List<UsedSpell> Spells { get; } = [];
    public double Dps => Utilities.GetValuePerSecondToDouble(Damage, CombatStart, CombatTime, 9999);
    public double Hps => Utilities.GetValuePerSecondToDouble(Heal, CombatStart, CombatTime, 9999);

    public void StartCombatInterval(DateTime startTime)
    {
        lock (SyncRoot)
        {
            if (_combatTimes.Any(x => x.EndTime == null))
            {
                return;
            }

            _combatTimes.Add(new ActionInterval(startTime));
        }
    }

    public void EndCombatInterval(DateTime endTime)
    {
        lock (SyncRoot)
        {
            var combatTime = _combatTimes.FirstOrDefault(x => x.EndTime == null);
            if (combatTime == null)
            {
                return;
            }

            combatTime.EndTime = endTime;
            SetCombatTimeSpan();
        }
    }

    public void ApplyTo(PlayerGameObject player)
    {
        lock (SyncRoot)
        {
            player.CombatStart = CombatStart;
            player.CombatTime = CombatTime;
            player.Damage = Damage;
            player.Heal = Heal;
            player.TakenDamage = TakenDamage;
            player.Overhealed = Overhealed;
            player.Spells = Spells.Select(CloneSpell).ToList();
        }
    }

    private void SetCombatTimeSpan()
    {
        foreach (var combatTime in _combatTimes.Where(x => x.EndTime != null).ToList())
        {
            CombatTime += combatTime.TimeSpan;
            _combatTimes.Remove(combatTime);
        }
    }

    private static UsedSpell CloneSpell(UsedSpell spell)
    {
        return new UsedSpell(spell.SpellIndex, spell.ItemIndex)
        {
            ItemIndex = spell.ItemIndex,
            UniqueName = spell.UniqueName,
            Target = spell.Target,
            Category = spell.Category,
            HealthChangeType = spell.HealthChangeType,
            DamageHealValue = spell.DamageHealValue,
            Ticks = spell.Ticks
        };
    }
}