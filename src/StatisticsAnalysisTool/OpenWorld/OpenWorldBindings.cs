using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Common.UserSettings;
using StatisticsAnalysisTool.GameFileData;
using StatisticsAnalysisTool.Localization;
using StatisticsAnalysisTool.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace StatisticsAnalysisTool.OpenWorld;

public class OpenWorldBindings : BaseViewModel
{
    private ObservableCollection<OpenWorldMobKillStatsEntry> _mammothKillStatsEntries = [];

    public OpenWorldBindings()
    {
        IsOpenWorldTrackingActive = SettingsController.CurrentSettings.IsOpenWorldTrackingActive;
        AutoDeleteStatsSelection = SettingsController.CurrentSettings.OpenWorldAutoDeleteStats;
    }

    public ObservableCollection<OpenWorldMobKill> MobKills { get; } = [];
    public ObservableCollection<OpenWorldMobKillStatsEntry> MammothKillStatsEntries
    {
        get => _mammothKillStatsEntries;
        private set
        {
            _mammothKillStatsEntries = value;
            OnPropertyChanged();
        }
    }

    public IReadOnlyList<OpenWorldStatsTimeFilter> StatsTimeTypes { get; } =
    [
        new()
        {
            Name = LocalizationController.Translation("HOUR"),
            StatsTimeType = OpenWorldStatsTimeType.Hour
        },
        new()
        {
            Name = LocalizationController.Translation("TODAY"),
            StatsTimeType = OpenWorldStatsTimeType.Today
        },
        new()
        {
            Name = LocalizationController.Translation("THIS_WEEK"),
            StatsTimeType = OpenWorldStatsTimeType.ThisWeek
        },
        new()
        {
            Name = LocalizationController.Translation("LAST_WEEK"),
            StatsTimeType = OpenWorldStatsTimeType.LastWeek
        },
        new()
        {
            Name = LocalizationController.Translation("LAST_30_DAYS"),
            StatsTimeType = OpenWorldStatsTimeType.Month
        },
        new()
        {
            Name = LocalizationController.Translation("LAST_365_DAYS"),
            StatsTimeType = OpenWorldStatsTimeType.Year
        }
    ];

    public IReadOnlyList<OpenWorldAutoDeleteStatsFilter> AutoDeleteStats { get; } =
    [
        new()
        {
            Name = LocalizationController.Translation("NEVER_DELETE"),
            AutoDeleteStats = OpenWorldAutoDeleteStats.NeverDelete
        },
        new()
        {
            Name = LocalizationController.Translation("DELETE_AFTER_7_DAYS"),
            AutoDeleteStats = OpenWorldAutoDeleteStats.DeleteAfter7Days
        },
        new()
        {
            Name = LocalizationController.Translation("DELETE_AFTER_30_DAYS"),
            AutoDeleteStats = OpenWorldAutoDeleteStats.DeleteAfter30Days
        },
        new()
        {
            Name = LocalizationController.Translation("DELETE_AFTER_365_DAYS"),
            AutoDeleteStats = OpenWorldAutoDeleteStats.DeleteAfter365Days
        }
    ];

    public void UpdateStats()
    {
        var timeRange = GetTimeRange(StatsTimeTypeSelection);
        var filteredKills = MobKills
            .Where(x => timeRange.Contains(x.TimestampDateTimeUtc))
            .ToList();

        var entries = filteredKills
            .GroupBy(x => x.MobUniqueName, StringComparer.OrdinalIgnoreCase)
            .Select(x => new OpenWorldMobKillStatsEntry
            {
                MobUniqueName = x.Key,
                MobName = GetMobName(x),
                Avatar = GetAvatarFileName(x),
                Kills = x.Count(),
                KillsPerHour = CalculateKillsPerHour(x)
            })
            .OrderByDescending(x => x.Kills)
            .ThenBy(x => x.MobName)
            .ToList();

        MammothKillStatsEntries = new ObservableCollection<OpenWorldMobKillStatsEntry>(entries);
    }

    public static OpenWorldTimeRange GetTimeRange(OpenWorldStatsTimeType statsTimeType)
    {
        var currentUtc = DateTime.UtcNow;
        var currentDay = currentUtc.Date;
        var currentMinute = new DateTime(currentUtc.Year, currentUtc.Month, currentUtc.Day, currentUtc.Hour, currentUtc.Minute, 0, DateTimeKind.Utc);
        var startOfThisWeek = GetStartOfIsoWeek(currentDay);

        return statsTimeType switch
        {
            OpenWorldStatsTimeType.Hour => new OpenWorldTimeRange(currentMinute.AddHours(-1), currentMinute.AddMinutes(1)),
            OpenWorldStatsTimeType.Today => new OpenWorldTimeRange(currentDay, currentDay.AddDays(1)),
            OpenWorldStatsTimeType.ThisWeek => new OpenWorldTimeRange(startOfThisWeek, startOfThisWeek.AddDays(7)),
            OpenWorldStatsTimeType.LastWeek => new OpenWorldTimeRange(startOfThisWeek.AddDays(-7), startOfThisWeek),
            OpenWorldStatsTimeType.Month => new OpenWorldTimeRange(currentDay.AddDays(-29), currentDay.AddDays(1)),
            OpenWorldStatsTimeType.Year => new OpenWorldTimeRange(currentDay.AddDays(-364), currentDay.AddDays(1)),
            _ => OpenWorldTimeRange.Empty
        };
    }

    public bool IsOpenWorldTrackingActive
    {
        get;
        set
        {
            field = value;
            SettingsController.CurrentSettings.IsOpenWorldTrackingActive = field;
            OnPropertyChanged();
        }
    }

    public OpenWorldStatsTimeType StatsTimeTypeSelection
    {
        get;
        set
        {
            if (field == value)
            {
                return;
            }

            field = value;
            UpdateStats();
            OnPropertyChanged();
        }
    } = OpenWorldStatsTimeType.Today;

    public OpenWorldAutoDeleteStats AutoDeleteStatsSelection
    {
        get;
        set
        {
            field = value;
            SettingsController.CurrentSettings.OpenWorldAutoDeleteStats = field;
            OnPropertyChanged();
        }
    }

    public string TranslationKilledMammoths => LocalizationController.Translation("KILLED_MAMMOTHS");
    public string TranslationOpenWorldActive => LocalizationController.Translation("OPEN_WORLD_ACTIVE");
    public string TranslationMob => LocalizationController.Translation("MOB");
    public string TranslationKills => LocalizationController.Translation("KILLS");
    public string TranslationPerHour => LocalizationController.Translation("PER_HOUR");

    private static double CalculateKillsPerHour(IEnumerable<OpenWorldMobKill> kills)
    {
        var killList = kills
            .OrderBy(x => x.TimestampDateTimeUtc)
            .ToList();

        if (killList.Count == 0)
        {
            return 0;
        }

        var durationInSeconds = Math.Max(3600d, (killList[^1].TimestampDateTimeUtc - killList[0].TimestampDateTimeUtc).TotalSeconds);
        return ((double) killList.Count).GetValuePerHour(durationInSeconds);
    }

    private static string GetMobName(IGrouping<string, OpenWorldMobKill> kills)
    {
        var mobData = MobsData.GetMobByUniqueNameOrDefault(kills.Key);
        var localizedName = MobsData.GetLocalizedMobName(mobData);
        if (!string.IsNullOrWhiteSpace(localizedName)
            && !string.Equals(localizedName, kills.Key, StringComparison.OrdinalIgnoreCase))
        {
            return localizedName;
        }

        var savedName = kills.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x.MobName)
                                                  && !string.Equals(x.MobName, x.MobUniqueName, StringComparison.OrdinalIgnoreCase))?.MobName;
        return string.IsNullOrWhiteSpace(savedName) ? kills.Key : savedName;
    }

    private static string GetAvatarFileName(IGrouping<string, OpenWorldMobKill> kills)
    {
        var savedAvatar = kills.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x.Avatar))?.Avatar;
        if (!string.IsNullOrWhiteSpace(savedAvatar))
        {
            return savedAvatar;
        }

        return MobsData.GetAvatarFileName(MobsData.GetMobByUniqueNameOrDefault(kills.Key));
    }

    private static DateTime GetStartOfIsoWeek(DateTime dateTime)
    {
        var diff = (7 + (dateTime.DayOfWeek - DayOfWeek.Monday)) % 7;
        return dateTime.AddDays(-diff).Date;
    }

    public readonly record struct OpenWorldTimeRange(DateTime Start, DateTime End)
    {
        public static OpenWorldTimeRange Empty => new(DateTime.MinValue, DateTime.MinValue);

        public bool Contains(DateTime timestamp)
        {
            return timestamp >= Start && timestamp < End;
        }
    }
}
