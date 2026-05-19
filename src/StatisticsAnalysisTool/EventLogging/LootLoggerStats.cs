using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.EventLogging.Notification;
using StatisticsAnalysisTool.Localization;
using StatisticsAnalysisTool.Models;
using StatisticsAnalysisTool.Models.NetworkModel;
using StatisticsAnalysisTool.ViewModels;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace StatisticsAnalysisTool.EventLogging;

public sealed class LootLoggerStats : BaseViewModel
{
    private const int TopListLimit = 5;
    private static readonly TimeSpan UiRefreshDelay = TimeSpan.FromMilliseconds(250);

    private readonly Lock _sync = new();
    private readonly Dictionary<string, int> _killCounts = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, int> _deathCounts = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, double> _lostLootValueByPlayer = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, double> _lootedValueByPlayer = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, int> _lootedItemCountsByPlayer = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, string> _killerByDiedPlayer = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<LootLoggerSingleItemStats> _bestSingleLootItems = [];
    private int _isRefreshScheduled;
    private DateTime? _sessionStartedUtc;
    private DateTime _lastEventUtc;
    private double _totalLootValue;
    private double _totalFame;
    private double _totalSilver;
    private double _totalFactionPoints;
    private double _bestSingleLootValue;
    private string _bestSingleLootName = "-";
    private string _bestSingleLootDetail = "-";

    public void RecordLoot(Loot loot, Item item)
    {
        if (loot == null || item == null)
        {
            return;
        }

        var quantity = Math.Max(loot.Quantity, 0);
        var unitValue = Math.Max(item.AverageEstMarketValue, 0L);
        var value = (double) unitValue * quantity;
        var eventTimeUtc = ToUtc(loot.UtcPickupTime);

        lock (_sync)
        {
            RegisterEventTime(eventTimeUtc);
            _totalLootValue += value;

            var lootedByName = CleanName(loot.LootedByName);
            var lootedFromName = CleanName(loot.LootedFromName);
            if (!string.IsNullOrWhiteSpace(lootedByName))
            {
                _lootedValueByPlayer[lootedByName] = _lootedValueByPlayer.GetValueOrDefault(lootedByName) + value;
                if (quantity > 0)
                {
                    _lootedItemCountsByPlayer[lootedByName] = _lootedItemCountsByPlayer.GetValueOrDefault(lootedByName) + 1;
                }
            }

            if (!string.IsNullOrWhiteSpace(lootedFromName))
            {
                _lostLootValueByPlayer[lootedFromName] = _lostLootValueByPlayer.GetValueOrDefault(lootedFromName) + value;
            }

            var singleItemStats = new LootLoggerSingleItemStats(item, lootedByName, unitValue, eventTimeUtc);
            _bestSingleLootItems.Add(singleItemStats);

            if (unitValue > _bestSingleLootValue)
            {
                _bestSingleLootValue = unitValue;
                _bestSingleLootName = singleItemStats.DisplayName;
                _bestSingleLootDetail = CreateBestLootDetail(lootedByName, eventTimeUtc);
            }
        }

        RequestSnapshotUpdate();
    }

    public void RecordNotification(TrackingNotification notification)
    {
        if (notification == null)
        {
            return;
        }

        var eventTimeUtc = ToUtc(notification.DateTime);

        lock (_sync)
        {
            RegisterEventTime(eventTimeUtc);

            switch (notification.Fragment)
            {
                case FameNotificationFragment fame:
                    _totalFame += Math.Max(fame.TotalGainedFame, 0d);
                    break;
                case SilverNotificationFragment silver:
                    _totalSilver += Math.Max(silver.TotalGainedSilver, 0d);
                    break;
                case FactionPointsNotificationFragment factionPoints:
                    _totalFactionPoints += Math.Max(factionPoints.GainedFractionPoints, 0d);
                    break;
                case FactionFlagPointsNotificationFragment factionFlagPoints:
                    _totalFactionPoints += Math.Max(factionFlagPoints.GainedFractionPoints, 0d);
                    break;
                default:
                    return;
            }
        }

        RequestSnapshotUpdate();
    }

    public void RecordKillDeath(string died, string killedBy)
    {
        var diedName = CleanName(died);
        var killedByName = CleanName(killedBy);

        if (string.IsNullOrWhiteSpace(diedName) && string.IsNullOrWhiteSpace(killedByName))
        {
            return;
        }

        lock (_sync)
        {
            RegisterEventTime(DateTime.UtcNow);

            if (!string.IsNullOrWhiteSpace(diedName))
            {
                _deathCounts[diedName] = _deathCounts.GetValueOrDefault(diedName) + 1;
            }

            if (!string.IsNullOrWhiteSpace(killedByName))
            {
                _killCounts[killedByName] = _killCounts.GetValueOrDefault(killedByName) + 1;
            }

            if (!string.IsNullOrWhiteSpace(diedName) && !string.IsNullOrWhiteSpace(killedByName))
            {
                _killerByDiedPlayer[diedName] = killedByName;
            }
        }

        RequestSnapshotUpdate();
    }

    public void Reset()
    {
        lock (_sync)
        {
            _killCounts.Clear();
            _deathCounts.Clear();
            _lostLootValueByPlayer.Clear();
            _lootedValueByPlayer.Clear();
            _lootedItemCountsByPlayer.Clear();
            _killerByDiedPlayer.Clear();
            _bestSingleLootItems.Clear();
            _sessionStartedUtc = null;
            _lastEventUtc = default;
            _totalLootValue = 0d;
            _totalFame = 0d;
            _totalSilver = 0d;
            _totalFactionPoints = 0d;
            _bestSingleLootValue = 0d;
            _bestSingleLootName = "-";
            _bestSingleLootDetail = "-";
        }

        ApplySnapshot(CreateSnapshot());
    }

    public void Refresh()
    {
        RequestSnapshotUpdate();
    }

    public bool HasData
    {
        get;
        private set => SetProperty(ref field, value);
    }

    public string TotalLootValue
    {
        get;
        private set => SetProperty(ref field, value);
    } = "0";

    public string LootValuePerHour
    {
        get;
        private set => SetProperty(ref field, value);
    } = "0";

    public string TotalFame
    {
        get;
        private set => SetProperty(ref field, value);
    } = "0";

    public string FamePerHour
    {
        get;
        private set => SetProperty(ref field, value);
    } = "0";

    public string TotalSilver
    {
        get;
        private set => SetProperty(ref field, value);
    } = "0";

    public string SilverPerHour
    {
        get;
        private set => SetProperty(ref field, value);
    } = "0";

    public string TotalFactionPoints
    {
        get;
        private set => SetProperty(ref field, value);
    } = "0";

    public string FactionPointsPerHour
    {
        get;
        private set => SetProperty(ref field, value);
    } = "0";

    public string BestSingleLootValue
    {
        get;
        private set => SetProperty(ref field, value);
    } = "0";

    public string BestSingleLootName
    {
        get;
        private set => SetProperty(ref field, value);
    } = "-";

    public string BestSingleLootDetail
    {
        get;
        private set => SetProperty(ref field, value);
    } = "-";

    public IReadOnlyList<LootLoggerStatsEntry> BestSingleLoots
    {
        get;
        private set => SetProperty(ref field, value);
    } = [];

    public IReadOnlyList<LootLoggerStatsEntry> HighestLootedValues
    {
        get;
        private set => SetProperty(ref field, value);
    } = [];

    public IReadOnlyList<LootLoggerStatsEntry> MostLootedItems
    {
        get;
        private set => SetProperty(ref field, value);
    } = [];

    public IReadOnlyList<LootLoggerStatsEntry> MostKills
    {
        get;
        private set => SetProperty(ref field, value);
    } = [];

    public IReadOnlyList<LootLoggerStatsEntry> MostDeaths
    {
        get;
        private set => SetProperty(ref field, value);
    } = [];

    public IReadOnlyList<LootLoggerStatsEntry> HighestKillValues
    {
        get;
        private set => SetProperty(ref field, value);
    } = [];

    private void RegisterEventTime(DateTime eventTimeUtc)
    {
        if (_sessionStartedUtc == null || eventTimeUtc < _sessionStartedUtc.Value)
        {
            _sessionStartedUtc = eventTimeUtc;
        }

        if (eventTimeUtc > _lastEventUtc)
        {
            _lastEventUtc = eventTimeUtc;
        }
    }

    private void RequestSnapshotUpdate()
    {
        if (Interlocked.Exchange(ref _isRefreshScheduled, 1) == 1)
        {
            return;
        }

        _ = Task.Delay(UiRefreshDelay).ContinueWith(_ =>
        {
            var snapshot = CreateSnapshot();
            Interlocked.Exchange(ref _isRefreshScheduled, 0);
            RunOnUiThreadAsync(() => ApplySnapshot(snapshot));
        }, TaskScheduler.Default);
    }

    private LootLoggerStatsSnapshot CreateSnapshot()
    {
        lock (_sync)
        {
            var hours = GetSessionHours();
            var hasData = _totalLootValue > 0d
                          || _totalFame > 0d
                          || _totalSilver > 0d
                          || _totalFactionPoints > 0d
                          || _killCounts.Count > 0
                          || _deathCounts.Count > 0
                          || _lootedItemCountsByPlayer.Count > 0;

            return new LootLoggerStatsSnapshot
            {
                HasData = hasData,
                TotalLootValue = FormatValue(_totalLootValue),
                LootValuePerHour = FormatPerHourValue(GetPerHourValue(_totalLootValue, hours)),
                TotalFame = FormatValue(_totalFame),
                FamePerHour = FormatPerHourValue(GetPerHourValue(_totalFame, hours)),
                TotalSilver = FormatValue(_totalSilver),
                SilverPerHour = FormatPerHourValue(GetPerHourValue(_totalSilver, hours)),
                TotalFactionPoints = FormatValue(_totalFactionPoints),
                FactionPointsPerHour = FormatPerHourValue(GetPerHourValue(_totalFactionPoints, hours)),
                BestSingleLootValue = FormatValue(_bestSingleLootValue),
                BestSingleLootName = string.IsNullOrWhiteSpace(_bestSingleLootName) ? "-" : _bestSingleLootName,
                BestSingleLootDetail = string.IsNullOrWhiteSpace(_bestSingleLootDetail) ? "-" : _bestSingleLootDetail,
                BestSingleLoots = CreateBestSingleLootEntries(),
                HighestLootedValues = CreateValueEntries(_lootedValueByPlayer),
                MostLootedItems = CreateTopEntries(_lootedItemCountsByPlayer),
                MostKills = CreateTopEntries(_killCounts),
                MostDeaths = CreateTopEntries(_deathCounts),
                HighestKillValues = CreateHighestKillValueEntries()
            };
        }
    }

    private void ApplySnapshot(LootLoggerStatsSnapshot snapshot)
    {
        HasData = snapshot.HasData;
        TotalLootValue = snapshot.TotalLootValue;
        LootValuePerHour = snapshot.LootValuePerHour;
        TotalFame = snapshot.TotalFame;
        FamePerHour = snapshot.FamePerHour;
        TotalSilver = snapshot.TotalSilver;
        SilverPerHour = snapshot.SilverPerHour;
        TotalFactionPoints = snapshot.TotalFactionPoints;
        FactionPointsPerHour = snapshot.FactionPointsPerHour;
        BestSingleLootValue = snapshot.BestSingleLootValue;
        BestSingleLootName = snapshot.BestSingleLootName;
        BestSingleLootDetail = snapshot.BestSingleLootDetail;
        BestSingleLoots = snapshot.BestSingleLoots;
        HighestLootedValues = snapshot.HighestLootedValues;
        MostLootedItems = snapshot.MostLootedItems;
        MostKills = snapshot.MostKills;
        MostDeaths = snapshot.MostDeaths;
        HighestKillValues = snapshot.HighestKillValues;
    }

    private double GetSessionHours()
    {
        if (_sessionStartedUtc == null)
        {
            return 0d;
        }

        var endUtc = DateTime.UtcNow > _lastEventUtc ? DateTime.UtcNow : _lastEventUtc;
        return Math.Max((endUtc - _sessionStartedUtc.Value).TotalHours, 1d / 3600d);
    }

    private IReadOnlyList<LootLoggerStatsEntry> CreateHighestKillValueEntries()
    {
        return _lostLootValueByPlayer
            .Where(x => x.Value > 0d && _deathCounts.ContainsKey(x.Key))
            .OrderByDescending(x => x.Value)
            .ThenBy(x => x.Key, StringComparer.OrdinalIgnoreCase)
            .Take(TopListLimit)
            .Select((x, index) => new LootLoggerStatsEntry
            {
                Rank = index + 1,
                Name = x.Key,
                Value = x.Value,
                Detail = _killerByDiedPlayer.GetValueOrDefault(x.Key) ?? string.Empty
            })
            .ToArray();
    }

    private IReadOnlyList<LootLoggerStatsEntry> CreateBestSingleLootEntries()
    {
        return _bestSingleLootItems
            .Where(x => x.UnitValue > 0d)
            .OrderByDescending(x => x.UnitValue)
            .ThenBy(x => x.DisplayName, StringComparer.OrdinalIgnoreCase)
            .ThenBy(x => x.CreatedUtc)
            .Take(TopListLimit)
            .Select((x, index) => new LootLoggerStatsEntry
            {
                Rank = index + 1,
                Name = x.DisplayName,
                Value = x.UnitValue,
                Detail = x.PlayerName,
                Icon = x.Icon
            })
            .ToArray();
    }

    private static IReadOnlyList<LootLoggerStatsEntry> CreateValueEntries(IReadOnlyDictionary<string, double> values)
    {
        return values
            .Where(x => x.Value > 0d)
            .OrderByDescending(x => x.Value)
            .ThenBy(x => x.Key, StringComparer.OrdinalIgnoreCase)
            .Take(TopListLimit)
            .Select((x, index) => new LootLoggerStatsEntry
            {
                Rank = index + 1,
                Name = x.Key,
                Value = x.Value
            })
            .ToArray();
    }

    private static IReadOnlyList<LootLoggerStatsEntry> CreateTopEntries(IReadOnlyDictionary<string, int> values)
    {
        return values
            .Where(x => x.Value > 0)
            .OrderByDescending(x => x.Value)
            .ThenBy(x => x.Key, StringComparer.OrdinalIgnoreCase)
            .Take(TopListLimit)
            .Select((x, index) => new LootLoggerStatsEntry
            {
                Rank = index + 1,
                Name = x.Key,
                Value = x.Value
            })
            .ToArray();
    }

    private static double GetPerHourValue(double value, double hours)
    {
        return hours > 0d ? value / hours : 0d;
    }

    private static string FormatValue(double value)
    {
        return Math.Max(value, 0d).ToShortNumberString();
    }

    private static string FormatPerHourValue(double value)
    {
        return LocalizationController.Translation("LOOT_LOGGER_PER_HOUR_VALUE_FORMAT", ["VALUE"], [FormatValue(value)]);
    }

    private static DateTime ToUtc(DateTime dateTime)
    {
        return dateTime.Kind switch
        {
            DateTimeKind.Utc => dateTime,
            DateTimeKind.Local => dateTime.ToUniversalTime(),
            _ => DateTime.SpecifyKind(dateTime, DateTimeKind.Utc)
        };
    }

    private static string CleanName(string name)
    {
        return name?.Trim() ?? string.Empty;
    }

    private static string CreateBestLootDetail(string lootedByName, DateTime eventTimeUtc)
    {
        var timeText = eventTimeUtc.ToLocalTime().ToString("HH:mm:ss", CultureInfo.CurrentCulture);
        return string.IsNullOrWhiteSpace(lootedByName)
            ? timeText
            : $"{lootedByName} - {timeText}";
    }

    private bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return false;
        }

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    private sealed class LootLoggerStatsSnapshot
    {
        public bool HasData { get; init; }
        public string TotalLootValue { get; init; } = "0";
        public string LootValuePerHour { get; init; } = "0";
        public string TotalFame { get; init; } = "0";
        public string FamePerHour { get; init; } = "0";
        public string TotalSilver { get; init; } = "0";
        public string SilverPerHour { get; init; } = "0";
        public string TotalFactionPoints { get; init; } = "0";
        public string FactionPointsPerHour { get; init; } = "0";
        public string BestSingleLootValue { get; init; } = "0";
        public string BestSingleLootName { get; init; } = "-";
        public string BestSingleLootDetail { get; init; } = "-";
        public IReadOnlyList<LootLoggerStatsEntry> BestSingleLoots { get; init; } = [];
        public IReadOnlyList<LootLoggerStatsEntry> HighestLootedValues { get; init; } = [];
        public IReadOnlyList<LootLoggerStatsEntry> MostLootedItems { get; init; } = [];
        public IReadOnlyList<LootLoggerStatsEntry> MostKills { get; init; } = [];
        public IReadOnlyList<LootLoggerStatsEntry> MostDeaths { get; init; } = [];
        public IReadOnlyList<LootLoggerStatsEntry> HighestKillValues { get; init; } = [];
    }

    private sealed class LootLoggerSingleItemStats(Item item, string lootedByName, long unitValue, DateTime createdUtc)
    {
        public DateTime CreatedUtc { get; } = createdUtc;
        public string DisplayName { get; } = string.IsNullOrWhiteSpace(item.LocalizedName) ? item.UniqueName : item.LocalizedName;
        public string PlayerName { get; } = string.IsNullOrWhiteSpace(lootedByName) ? "-" : lootedByName;
        public double UnitValue { get; } = Math.Max(unitValue, 0L);
        public BitmapImage Icon { get; } = item.Icon;
    }
}