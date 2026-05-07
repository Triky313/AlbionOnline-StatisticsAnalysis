using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.GameFileData;
using StatisticsAnalysisTool.Localization;
using StatisticsAnalysisTool.Models.NetworkModel;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace StatisticsAnalysisTool.DamageMeter;

public static class DamageMeterYourStatsSnapshotFactory
{
    private const int TopCount = 3;

    public static DamageMeterYourStatsSnapshot Clone(DamageMeterYourStatsSnapshot snapshot)
    {
        snapshot ??= DamageMeterYourStatsSnapshot.Empty;

        return new DamageMeterYourStatsSnapshot
        {
            HasData = snapshot.HasData,
            TotalDamage = snapshot.TotalDamage,
            TotalDps = snapshot.TotalDps,
            PeakDpsThreeSeconds = snapshot.PeakDpsThreeSeconds,
            PeakDpsFiveSeconds = snapshot.PeakDpsFiveSeconds,
            PeakDpsTenSeconds = snapshot.PeakDpsTenSeconds,
            BiggestHit = snapshot.BiggestHit,
            PveDamage = snapshot.PveDamage,
            PvpDamage = snapshot.PvpDamage,
            TotalHealing = snapshot.TotalHealing,
            EffectiveHealing = snapshot.EffectiveHealing,
            OverhealPercent = snapshot.OverhealPercent,
            TopHealingTargets = CloneTopEntries(snapshot.TopHealingTargets),
            DamageTaken = snapshot.DamageTaken,
            Dtps = snapshot.Dtps,
            TopDamageTakenBySpell = CloneTopEntries(snapshot.TopDamageTakenBySpell),
            CombatTime = snapshot.CombatTime,
            FightCount = snapshot.FightCount,
            AverageFightDuration = snapshot.AverageFightDuration
        };
    }

    public static DamageMeterYourStatsSnapshot FromLiveData(
        PlayerGameObject localPlayer,
        IEnumerable<CombatEvent> combatEvents,
        Func<long, string> objectNameResolver
    )
    {
        if (localPlayer?.ObjectId is not { } localObjectId)
        {
            return DamageMeterYourStatsSnapshot.Empty;
        }

        var events = (combatEvents ?? [])
            .Where(x => IsLocalCombatEvent(x, localObjectId))
            .ToList();

        var damageContributions = events
            .SelectMany(x => x.Contributions.Select(y => new ContributionWithEvent(x, y)))
            .Where(x => x.Contribution.ValueType == CombatEventValueType.Damage && x.Contribution.SourceObjectId == localObjectId)
            .ToList();

        var healContributions = events
            .SelectMany(x => x.Contributions)
            .Where(x => x.ValueType == CombatEventValueType.Heal && x.SourceObjectId == localObjectId)
            .ToList();

        var takenDamageContributions = events
            .SelectMany(x => x.Contributions)
            .Where(x => x.ValueType == CombatEventValueType.TakenDamage
                        && x.TargetObjectId == localObjectId
                        && x.SourceObjectId != localObjectId)
            .ToList();

        var combatTime = localPlayer.CombatTime;
        var totalHealing = localPlayer.Heal + localPlayer.Overhealed;
        var fightDurations = events
            .Select(GetDuration)
            .Where(x => x > TimeSpan.Zero)
            .ToList();

        return new DamageMeterYourStatsSnapshot
        {
            HasData = true,
            TotalDamage = localPlayer.Damage.ToShortNumberString(),
            TotalDps = localPlayer.Dps.ToShortNumberString(),
            PeakDpsThreeSeconds = FormatPerSecond(GetPeakDamage(damageContributions.Select(x => x.Contribution), TimeSpan.FromSeconds(3)), TimeSpan.FromSeconds(3)),
            PeakDpsFiveSeconds = FormatPerSecond(GetPeakDamage(damageContributions.Select(x => x.Contribution), TimeSpan.FromSeconds(5)), TimeSpan.FromSeconds(5)),
            PeakDpsTenSeconds = FormatPerSecond(GetPeakDamage(damageContributions.Select(x => x.Contribution), TimeSpan.FromSeconds(10)), TimeSpan.FromSeconds(10)),
            BiggestHit = damageContributions.Select(x => x.Contribution.Value).DefaultIfEmpty(0).Max().ToShortNumberString(),
            PveDamage = damageContributions.Where(IsPveDamage).Sum(x => x.Contribution.Value).ToShortNumberString(),
            PvpDamage = damageContributions.Where(IsPvpDamage).Sum(x => x.Contribution.Value).ToShortNumberString(),
            TotalHealing = totalHealing.ToShortNumberString(),
            EffectiveHealing = localPlayer.Heal.ToShortNumberString(),
            OverhealPercent = FormatPercent(localPlayer.Overhealed, totalHealing),
            TopHealingTargets = CreateTopEntries(healContributions
                .GroupBy(x => x.TargetObjectId)
                .Select(x => new NamedValue(objectNameResolver?.Invoke(x.Key), x.Sum(y => y.Value)))
                .ToList()),
            DamageTaken = localPlayer.TakenDamage.ToShortNumberString(),
            Dtps = FormatPerSecond(localPlayer.TakenDamage, combatTime),
            TopDamageTakenBySpell = CreateTopEntries(takenDamageContributions
                .GroupBy(x => x.CausingSpellIndex)
                .Select(x => new NamedValue(ResolveSpellName(x.Key), x.Sum(y => y.Value)))
                .ToList()),
            CombatTime = FormatDuration(combatTime),
            FightCount = events.Count.ToString("N0", CultureInfo.CurrentCulture),
            AverageFightDuration = FormatDuration(GetAverageDuration(fightDurations))
        };
    }

    public static DamageMeterYourStatsSnapshot FromSnapshotFragments(IEnumerable<DamageMeterSnapshotFragment> fragments, Guid? localPlayerGuid, string localPlayerName)
    {
        var localPlayer = (fragments ?? [])
            .FirstOrDefault(x => IsLocalPlayer(x.CauserGuid, x.Name, localPlayerGuid, localPlayerName));

        if (localPlayer == null)
        {
            return DamageMeterYourStatsSnapshot.Empty;
        }

        var totalHealing = localPlayer.Heal;
        var totalSpellDamage = localPlayer.Spells
            .Where(x => x.HealthChangeType == HealthChangeType.Damage)
            .Sum(x => x.DamageHealValue);

        return new DamageMeterYourStatsSnapshot
        {
            HasData = true,
            TotalDamage = localPlayer.DamageShortString,
            TotalDps = localPlayer.DpsString,
            BiggestHit = localPlayer.Spells
                .Where(x => x.HealthChangeType == HealthChangeType.Damage)
                .Select(x => x.DamageHealValue)
                .DefaultIfEmpty(0)
                .Max()
                .ToShortNumberString(),
            PveDamage = totalSpellDamage.ToShortNumberString(),
            PvpDamage = 0L.ToShortNumberString(),
            TotalHealing = totalHealing.ToShortNumberString(),
            EffectiveHealing = localPlayer.HealShortString,
            OverhealPercent = FormatPercentFromDouble(localPlayer.OverhealedPercentageOfTotalHealing),
            DamageTaken = localPlayer.TakenDamage.ToShortNumberString(),
            Dtps = FormatPerSecond(localPlayer.TakenDamage, localPlayer.CombatTime),
            CombatTime = FormatDuration(localPlayer.CombatTime)
        };
    }

    private static IReadOnlyList<DamageMeterYourStatsTopEntry> CloneTopEntries(IEnumerable<DamageMeterYourStatsTopEntry> entries)
    {
        return (entries ?? [])
            .Select(x => new DamageMeterYourStatsTopEntry
            {
                Rank = x.Rank,
                Name = x.Name,
                Value = x.Value
            })
            .ToList();
    }

    private static IReadOnlyList<DamageMeterYourStatsTopEntry> CreateTopEntries(IReadOnlyCollection<NamedValue> values)
    {
        var rank = 1;
        return (values ?? [])
            .Where(x => x.Value > 0)
            .OrderByDescending(x => x.Value)
            .ThenBy(x => x.Name)
            .Take(TopCount)
            .Select(x => new DamageMeterYourStatsTopEntry
            {
                Rank = rank++,
                Name = FormatName(x.Name),
                Value = x.Value
            })
            .ToList();
    }

    private static bool IsLocalCombatEvent(CombatEvent combatEvent, long localObjectId)
    {
        return combatEvent.PlayerObjectIds.Contains(localObjectId)
               || combatEvent.Contributions.Any(x => x.SourceObjectId == localObjectId || x.TargetObjectId == localObjectId);
    }

    private static bool IsPveDamage(ContributionWithEvent contribution)
    {
        return !IsPvpDamage(contribution);
    }

    private static bool IsPvpDamage(ContributionWithEvent contribution)
    {
        return contribution.CombatEvent.PlayerObjectIds.Contains(contribution.Contribution.TargetObjectId)
               && contribution.Contribution.TargetObjectId != contribution.Contribution.SourceObjectId;
    }

    private static long GetPeakDamage(IEnumerable<CombatEventContribution> contributions, TimeSpan window)
    {
        var events = (contributions ?? [])
            .Where(x => x.Value > 0)
            .OrderBy(x => x.Timestamp)
            .ToList();

        long highestDamage = 0;
        long currentDamage = 0;
        var startIndex = 0;

        for (var endIndex = 0; endIndex < events.Count; endIndex++)
        {
            currentDamage += events[endIndex].Value;

            while (events[endIndex].Timestamp - events[startIndex].Timestamp > window)
            {
                currentDamage -= events[startIndex].Value;
                startIndex++;
            }

            highestDamage = Math.Max(highestDamage, currentDamage);
        }

        return highestDamage;
    }

    private static TimeSpan GetDuration(CombatEvent combatEvent)
    {
        var endTime = combatEvent.EndTime ?? combatEvent.LastEventTime;
        return endTime > combatEvent.StartTime
            ? endTime - combatEvent.StartTime
            : TimeSpan.Zero;
    }

    private static TimeSpan GetAverageDuration(IReadOnlyCollection<TimeSpan> durations)
    {
        if (durations == null || durations.Count <= 0)
        {
            return TimeSpan.Zero;
        }

        return TimeSpan.FromTicks((long) durations.Average(x => x.Ticks));
    }

    private static bool IsLocalPlayer(Guid causerGuid, string playerName, Guid? localPlayerGuid, string localPlayerName)
    {
        if (localPlayerGuid.HasValue && causerGuid == localPlayerGuid.Value)
        {
            return true;
        }

        return !string.IsNullOrWhiteSpace(localPlayerName)
               && string.Equals(playerName, localPlayerName, StringComparison.OrdinalIgnoreCase);
    }

    private static string ResolveSpellName(int spellIndex)
    {
        if (spellIndex <= 0)
        {
            return LocalizationController.Translation("AUTO_ATTACK");
        }

        var uniqueName = SpellData.GetUniqueName(spellIndex);
        var localizedName = SpellData.GetLocalizationName(uniqueName);
        return !string.IsNullOrWhiteSpace(localizedName)
            ? localizedName
            : string.Format(CultureInfo.CurrentCulture, "Spell {0}", spellIndex);
    }

    private static string FormatPerSecond(long value, TimeSpan timeSpan)
    {
        return timeSpan.TotalSeconds > 0
            ? (value / timeSpan.TotalSeconds).ToShortNumberString()
            : 0d.ToShortNumberString();
    }

    private static string FormatPercent(long value, long total)
    {
        return total > 0
            ? FormatPercentFromDouble(value / (double) total * 100d)
            : FormatPercentFromDouble(0d);
    }

    private static string FormatPercentFromDouble(double value)
    {
        return $"{value.ToString("N2", CultureInfo.CurrentCulture)}%";
    }

    private static string FormatDuration(TimeSpan duration)
    {
        return duration.TotalHours >= 1
            ? duration.ToString(@"h\:mm\:ss", CultureInfo.CurrentCulture)
            : duration.ToString(@"m\:ss", CultureInfo.CurrentCulture);
    }

    private static string FormatName(string name)
    {
        return string.IsNullOrWhiteSpace(name) ? Translation("UNKNOWN") : name;
    }

    private static string Translation(string key)
    {
        return LocalizationController.Translation(key);
    }

    private readonly record struct ContributionWithEvent(CombatEvent CombatEvent, CombatEventContribution Contribution);
    private readonly record struct NamedValue(string Name, long Value);
}