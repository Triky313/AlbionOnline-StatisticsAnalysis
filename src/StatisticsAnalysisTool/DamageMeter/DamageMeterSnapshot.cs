using StatisticsAnalysisTool.ViewModels;
using StatisticsAnalysisTool.Localization;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json.Serialization;

namespace StatisticsAnalysisTool.DamageMeter;

public sealed class DamageMeterSnapshot : BaseViewModel
{
    private DateTime _timestamp;
    private List<DamageMeterSnapshotFragment> _damageMeter = new();
    private DamageStatsSnapshot _damageStats = DamageStatsSnapshot.Empty;
    private DamageMeterYourStatsSnapshot _yourStats = DamageMeterYourStatsSnapshot.Empty;
    private string _location = string.Empty;
    private bool _isAutoSave;

    public DamageMeterSnapshot()
    {
        Timestamp = DateTime.UtcNow;
    }

    public DateTime Timestamp
    {
        get => _timestamp;
        set
        {
            _timestamp = value;
            OnPropertyChanged();
        }
    }

    [JsonIgnore]
    public string TimestampString
    {
        get
        {
            var displayName = Timestamp.ToString(CultureInfo.DefaultThreadCurrentCulture);
            if (!string.IsNullOrWhiteSpace(Location))
            {
                displayName = $"{displayName} - {Location}";
            }

            if (IsAutoSave)
            {
                displayName = $"{displayName} - {LocalizationController.Translation("AUTOSAVE")}";
            }

            return displayName;
        }
    }

    public string Location
    {
        get => _location;
        set
        {
            _location = value ?? string.Empty;
            OnPropertyChanged();
            OnPropertyChanged(nameof(TimestampString));
        }
    }

    public bool IsAutoSave
    {
        get => _isAutoSave;
        set
        {
            _isAutoSave = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(TimestampString));
        }
    }

    public List<DamageMeterSnapshotFragment> DamageMeter
    {
        get => _damageMeter;
        set
        {
            _damageMeter = value;
            OnPropertyChanged();
        }
    }

    public DamageStatsSnapshot DamageStats
    {
        get => _damageStats;
        set
        {
            _damageStats = value ?? DamageStatsSnapshot.Empty;
            OnPropertyChanged();
        }
    }

    public DamageMeterYourStatsSnapshot YourStats
    {
        get => _yourStats;
        set
        {
            _yourStats = value ?? DamageMeterYourStatsSnapshot.Empty;
            OnPropertyChanged();
        }
    }
}
