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
    private const int MaxDisplayedMobStatsEntries = 10;
    private const string AllFactionsValue = "";
    private ObservableCollection<OpenWorldMobKillStatsEntry> _mobKillStatsEntries = [];
    private IReadOnlyList<OpenWorldFactionFilter> _factionFilters = [];
    private string _selectedFaction = AllFactionsValue;
    private string _mobSearchText = string.Empty;

    public OpenWorldBindings()
    {
        IsOpenWorldTrackingActive = SettingsController.CurrentSettings.IsOpenWorldTrackingActive;
        AutoDeleteStatsSelection = SettingsController.CurrentSettings.OpenWorldAutoDeleteStats;
        FactionFilters = CreateFactionFilters();
    }

    public ObservableCollection<OpenWorldMobKill> MobKills { get; } = [];
    public ObservableCollection<OpenWorldMobKillStatsEntry> MobKillStatsEntries
    {
        get => _mobKillStatsEntries;
        private set
        {
            _mobKillStatsEntries = value;
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

    public IReadOnlyList<OpenWorldFactionFilter> FactionFilters
    {
        get => _factionFilters;
        private set
        {
            _factionFilters = value;
            OnPropertyChanged();
        }
    }

    public void UpdateStats()
    {
        RefreshFactionFilters();

        var timeRange = GetTimeRange(StatsTimeTypeSelection);
        var filteredKills = MobKills
            .Where(x => !string.IsNullOrWhiteSpace(x.MobUniqueName))
            .Where(x => timeRange.Contains(x.TimestampDateTimeUtc))
            .ToList();

        var entries = filteredKills
            .GroupBy(x => x.MobUniqueName, StringComparer.OrdinalIgnoreCase)
            .Select(x => new OpenWorldMobKillStatsEntry
            {
                MobUniqueName = x.Key,
                MobName = GetMobName(x),
                Avatar = GetAvatarFileName(x),
                Faction = GetFaction(x),
                LastKillTimestampUtc = x.Max(y => y.TimestampUtc),
                Kills = x.Count(),
                KillsPerHour = CalculateKillsPerHour(x)
            })
            .Where(MatchesFactionFilter)
            .Where(MatchesSearchText)
            .OrderByDescending(x => x.LastKillTimestampUtc)
            .ThenBy(x => x.MobName)
            .Take(MaxDisplayedMobStatsEntries)
            .ToList();

        MobKillStatsEntries = new ObservableCollection<OpenWorldMobKillStatsEntry>(entries);
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

    public string SelectedFaction
    {
        get => _selectedFaction;
        set
        {
            var faction = value ?? AllFactionsValue;
            if (string.Equals(_selectedFaction, faction, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            _selectedFaction = faction;
            UpdateStats();
            OnPropertyChanged();
        }
    }

    public string MobSearchText
    {
        get => _mobSearchText;
        set
        {
            var searchText = value ?? string.Empty;
            if (string.Equals(_mobSearchText, searchText, StringComparison.Ordinal))
            {
                return;
            }

            _mobSearchText = searchText;
            UpdateStats();
            OnPropertyChanged();
        }
    }

    public string TranslationKilledMobs => LocalizationController.Translation("KILLED_MOBS");
    public string TranslationOpenWorldActive => LocalizationController.Translation("OPEN_WORLD_ACTIVE");
    public string TranslationMob => LocalizationController.Translation("MOB");
    public string TranslationKills => LocalizationController.Translation("KILLS");
    public string TranslationPerHour => LocalizationController.Translation("PER_HOUR");
    public string TranslationSearchMobs => LocalizationController.Translation("SEARCH_MOBS");

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

    private static string GetFaction(IGrouping<string, OpenWorldMobKill> kills)
    {
        var savedFaction = kills.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x.Faction))?.Faction;
        if (!string.IsNullOrWhiteSpace(savedFaction))
        {
            return savedFaction;
        }

        return MobsData.GetFaction(MobsData.GetMobByUniqueNameOrDefault(kills.Key));
    }

    private bool MatchesFactionFilter(OpenWorldMobKillStatsEntry entry)
    {
        return string.IsNullOrWhiteSpace(SelectedFaction)
               || string.Equals(entry.Faction, SelectedFaction, StringComparison.OrdinalIgnoreCase);
    }

    private bool MatchesSearchText(OpenWorldMobKillStatsEntry entry)
    {
        if (string.IsNullOrWhiteSpace(MobSearchText))
        {
            return true;
        }

        return entry.MobName.Contains(MobSearchText, StringComparison.OrdinalIgnoreCase)
               || entry.MobUniqueName.Contains(MobSearchText, StringComparison.OrdinalIgnoreCase);
    }

    private void RefreshFactionFilters()
    {
        var selectedFaction = SelectedFaction;
        var filters = CreateFactionFilters();
        if (!AreFactionFiltersEqual(FactionFilters, filters))
        {
            FactionFilters = filters;
        }

        if (string.IsNullOrWhiteSpace(selectedFaction)
            || filters.Any(x => string.Equals(x.Faction, selectedFaction, StringComparison.OrdinalIgnoreCase)))
        {
            return;
        }

        _selectedFaction = AllFactionsValue;
        OnPropertyChanged(nameof(SelectedFaction));
    }

    private static IReadOnlyList<OpenWorldFactionFilter> CreateFactionFilters()
    {
        var filters = new List<OpenWorldFactionFilter>
        {
            new()
            {
                Name = LocalizationController.Translation("ALL_FACTIONS"),
                Faction = AllFactionsValue
            }
        };

        filters.AddRange(MobsData.GetFactions().Select(x => new OpenWorldFactionFilter
        {
            Name = x,
            Faction = x
        }));

        return filters;
    }

    private static bool AreFactionFiltersEqual(IReadOnlyList<OpenWorldFactionFilter> currentFilters, IReadOnlyList<OpenWorldFactionFilter> nextFilters)
    {
        if (currentFilters.Count != nextFilters.Count)
        {
            return false;
        }

        for (var i = 0; i < currentFilters.Count; i++)
        {
            if (!string.Equals(currentFilters[i].Faction, nextFilters[i].Faction, StringComparison.OrdinalIgnoreCase)
                || !string.Equals(currentFilters[i].Name, nextFilters[i].Name, StringComparison.Ordinal))
            {
                return false;
            }
        }

        return true;
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
