using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Common.UserSettings;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Localization;
using StatisticsAnalysisTool.ViewModels;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace StatisticsAnalysisTool.Streaming;

/// <summary>Copies the current values of the view models into a plain snapshot. Must run on the UI thread.</summary>
public static class StreamingSnapshotBuilder
{
    public static StreamingSnapshot Build(MainWindowViewModel vm, SettingsObject settings)
    {
        var snapshot = new StreamingSnapshot
        {
            Config = new StreamingConfig
            {
                ShowSession = settings.IsStreamingSessionShown,
                ShowDamageMeter = settings.IsStreamingDamageMeterShown,
                ShowGathering = settings.IsStreamingGatheringShown,
                MaxPlayers = Math.Clamp(settings.StreamingDamageMeterMaxPlayers, 1, 20),
                FontSizePercentage = Math.Clamp(settings.StreamingFontSizePercentage, 50, 300),
                BackgroundOpacityPercentage = Math.Clamp(settings.StreamingBackgroundOpacityPercentage, 0, 100)
            },
            Labels = new Dictionary<string, string>
            {
                ["session"] = LocalizationController.Translation("SESSION"),
                ["sessionTime"] = LocalizationController.Translation("SESSION_TIME"),
                ["damageMeter"] = LocalizationController.Translation("DAMAGE_METER"),
                ["gathering"] = LocalizationController.Translation("GATHERING"),
                ["resources"] = LocalizationController.Translation("RESOURCES"),
                ["total"] = LocalizationController.Translation("TOTAL"),
                ["perHour"] = LocalizationController.Translation("STREAMING_PER_HOUR"),
                ["noData"] = LocalizationController.Translation("STREAMING_NO_DATA")
            }
        };

        BuildSession(vm, snapshot.Session);
        BuildDamageMeter(vm, snapshot.DamageMeter, snapshot.Config.MaxPlayers);
        BuildGathering(vm, snapshot.Gathering);
        return snapshot;
    }

    private static void BuildSession(MainWindowViewModel vm, StreamingSession session)
    {
        var dashboard = vm.DashboardBindings;
        if (dashboard == null)
        {
            return;
        }

        session.SessionSeconds = Finite(dashboard.SessionTimeSummary.Value);

        // Same order as the session bar at the top of the tool
        session.Stats.Add(Stat("fame", "fame", "FAME", dashboard.FamePerHour, dashboard.TotalGainedFameInSession));
        session.Stats.Add(Stat("silver", "silver", "SILVER", dashboard.SilverPerHour, dashboard.TotalGainedSilverInSession));
        session.Stats.Add(Stat("respec", "respec", "RESPEC", dashboard.ReSpecPointsPerHour, dashboard.TotalGainedReSpecPointsInSession));
        session.Stats.Add(Stat("might", "might", "MIGHT", dashboard.MightPerHour, dashboard.TotalGainedMightInSession));
        session.Stats.Add(Stat("favor", "favor", "FAVOR", dashboard.FavorPerHour, dashboard.TotalGainedFavorInSession));

        var faction = vm.FactionPointStats?.FirstOrDefault();
        if (faction != null)
        {
            var icon = faction.CityFaction == CityFaction.Unknown ? null : $"faction-{faction.CityFaction.ToString().ToLowerInvariant()}";
            session.Stats.Add(Stat("faction", icon, "FACTION_POINTS", faction.ValuePerHour, faction.Value));
        }

        session.Stats.Add(Stat("loot", "loot", "LOOT", dashboard.LootStatistics.LootPerHour, dashboard.LootStatistics.TotalValueSummary.Value));
    }

    private static void BuildDamageMeter(MainWindowViewModel vm, List<StreamingDamageEntry> entries, int maxPlayers)
    {
        var fragments = vm.DamageMeterBindings?.DamageMeter;
        if (fragments == null)
        {
            return;
        }

        // Same order as the damage meter in the tool (follows the sort selected there)
        foreach (var fragment in fragments.Take(maxPlayers))
        {
            var dps = Finite(fragment.Dps);
            entries.Add(new StreamingDamageEntry
            {
                Name = fragment.Name,
                WeaponUniqueName = fragment.CauserMainHand?.UniqueName,
                Damage = fragment.Damage,
                DamageText = ShortNumber(fragment.Damage),
                Dps = dps,
                DpsText = dps.ToString("N0", CultureInfo.CurrentCulture),
                DamagePercentage = Finite(fragment.DamagePercentage),
                DamageBarPercentage = Finite(fragment.DamageInPercent),
                Heal = fragment.Heal,
                Hps = Finite(fragment.Hps)
            });
        }
    }

    private static void BuildGathering(MainWindowViewModel vm, StreamingGathering gathering)
    {
        var bindings = vm.GatheringBindings;
        var stats = bindings?.GatheringStats;
        if (stats == null)
        {
            return;
        }

        gathering.IsActive = bindings.IsGatheringActive;
        gathering.SilverPerHour = Finite(stats.TotalGainedSilverPerHour);
        gathering.TotalSilver = stats.TotalGainedSilverString;
        gathering.TotalResources = stats.TotalResources;
        gathering.Summary = Stat("gathering", "gathering", "GATHERING", stats.TotalGainedSilverPerHour, stats.TotalGainedSilverString);

        gathering.Resources.Add(Stat("wood", "wood", "WOOD", stats.GainedSilverPerHourByWood, stats.GainedSilverByWood));
        gathering.Resources.Add(Stat("ore", "ore", "ORE", stats.GainedSilverPerHourByOre, stats.GainedSilverByOre));
        gathering.Resources.Add(Stat("fiber", "fiber", "FIBER", stats.GainedSilverPerHourByFiber, stats.GainedSilverByFiber));
        gathering.Resources.Add(Stat("hide", "hide", "HIDE", stats.GainedSilverPerHourByHide, stats.GainedSilverByHide));
        gathering.Resources.Add(Stat("rock", "rock", "ROCK", stats.GainedSilverPerHourByRock, stats.GainedSilverByRock));
        gathering.Resources.Add(Stat("fish", "fish", "FISHING", stats.GainedSilverPerHourByFish, stats.GainedSilverByFish));
    }

    private static StreamingStat Stat(string key, string icon, string translationKey, double perHour, double total)
    {
        perHour = Finite(perHour);
        total = Finite(total);
        return new StreamingStat
        {
            Key = key,
            Icon = icon,
            Label = LocalizationController.Translation(translationKey),
            PerHour = perHour,
            Total = total,
            TotalText = ShortNumber(total),
            // Same format as ValuePerHourConverter in the tool
            PerHourText = $"{perHour.ToString("N0", CultureInfo.CurrentCulture)}/h"
        };
    }

    private static string ShortNumber(double value) => ((decimal) value).GetShortNumber(CultureInfo.CurrentCulture);

    // JSON cannot hold NaN or infinity (e.g. DPS right after a fight starts)
    private static double Finite(double value) => double.IsFinite(value) ? Math.Clamp(value, -1e15, 1e15) : 0;
}
