using StatisticsAnalysisTool.ViewModels;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json.Serialization;

namespace StatisticsAnalysisTool.DamageMeter;

public sealed class DamageMeterSnapshot : BaseViewModel
{
    private DateTime _timestamp;
    private List<DamageMeterSnapshotFragment> _damageMeter = new();

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
    public string TimestampString => Timestamp.ToString(CultureInfo.DefaultThreadCurrentCulture);

    public List<DamageMeterSnapshotFragment> DamageMeter
    {
        get => _damageMeter;
        set
        {
            _damageMeter = value;
            OnPropertyChanged();
        }
    }
}