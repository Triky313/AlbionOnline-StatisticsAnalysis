using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Models.NetworkModel;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StatisticsAnalysisTool.DamageMeter;

public static class DamageMeterContentSnapshotFactory
{
    public static DamageMeterContentSnapshot Create(
        List<KeyValuePair<Guid, PlayerGameObject>> entities,
        DamageStatsSnapshot trackerSnapshot,
        IReadOnlyCollection<CombatEvent> combatEvents,
        DamageMeterYourStatsSnapshot yourStats,
        IReadOnlyCollection<MobDamageMeterFragment> mobDamageMeter)
    {
        var fragments = entities.Select(x => CreateFragment(x.Value, entities)).ToList();
        return new DamageMeterContentSnapshot
        {
            DamageMeter = fragments,
            MobDamageMeter = mobDamageMeter?.ToList() ?? [],
            DamageStats = DamageStatsSnapshotFactory.FromLiveData(
                trackerSnapshot,
                entities.Select(x => x.Value),
                combatEvents),
            YourStats = yourStats
        };
    }

    private static DamageMeterSnapshotFragment CreateFragment(
        PlayerGameObject player,
        List<KeyValuePair<Guid, PlayerGameObject>> entities)
    {
        var mainHand = DamageMeterWeaponResolver.GetWeaponByIndex(player.LastContributionWeaponItemIndex);
        return new DamageMeterSnapshotFragment
        {
            CauserGuid = player.UserGuid,
            Name = player.Name,
            CombatTime = player.CombatTime,
            Damage = player.Damage,
            Dps = player.Dps,
            DamageInPercent = GetBarPercentage(player.Damage, entities.Max(x => x.Value.Damage)),
            DamagePercentage = GetTotalPercentage(player.Damage, entities.Sum(x => x.Value.Damage)),
            Heal = player.Heal,
            Hps = player.Hps,
            HealInPercent = GetBarPercentage(player.Heal, entities.Max(x => x.Value.Heal)),
            HealPercentage = GetTotalPercentage(player.Heal, entities.Sum(x => x.Value.Heal)),
            OverhealedPercentageOfTotalHealing = GetTotalPercentage(player.Overhealed, player.Heal + player.Overhealed),
            TakenDamage = player.TakenDamage,
            TakenDamageInPercent = GetBarPercentage(player.TakenDamage, entities.Max(x => x.Value.TakenDamage)),
            TakenDamagePercentage = GetTotalPercentage(player.TakenDamage, entities.Sum(x => x.Value.TakenDamage)),
            CauserMainHandItemUniqueName = mainHand?.UniqueName ?? string.Empty,
            Spells = CreateSpells(player.Spells)
        };
    }

    private static List<SpellsSnapshotFragment> CreateSpells(IReadOnlyCollection<UsedSpell> spells)
    {
        var fragments = new List<SpellsSnapshotFragment>();
        foreach (var spell in spells)
        {
            var matchingSpells = spells.Where(x => x.HealthChangeType == spell.HealthChangeType).ToList();
            var maximum = matchingSpells.Max(x => x.DamageHealValue);
            var total = matchingSpells.Sum(x => x.DamageHealValue);
            fragments.Add(new SpellsSnapshotFragment
            {
                SpellIndex = spell.SpellIndex,
                ItemIndex = spell.ItemIndex,
                UniqueName = spell.UniqueName,
                Target = spell.Target,
                Category = spell.Category,
                DamageHealValue = spell.DamageHealValue,
                Ticks = spell.Ticks,
                HealthChangeType = spell.HealthChangeType,
                DamageInPercent = GetBarPercentage(spell.DamageHealValue, maximum),
                DamagePercentage = GetTotalPercentage(spell.DamageHealValue, total)
            });
        }

        return fragments.OrderByDescending(x => x.DamageHealValue).ToList();
    }

    private static double GetBarPercentage(long value, long maximum)
    {
        return maximum > 0 ? Math.Min(100, (double) value / maximum * 100) : 0;
    }

    private static double GetTotalPercentage(long value, long total)
    {
        return total > 0 ? Math.Min(100, (double) value / total * 100) : 0;
    }
}
