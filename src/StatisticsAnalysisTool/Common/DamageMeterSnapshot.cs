using StatisticsAnalysisTool.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using StatisticsAnalysisTool.Properties;

namespace StatisticsAnalysisTool.Common;

public class DamageMeterSnapshot : INotifyPropertyChanged
{
    private DateTime _timestamp;
    private List<DamageMeterSnapshotFragment> _damageMeter = new ();

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

    public string TimestampString => Timestamp.ToString(LanguageController.CurrentCultureInfo);

    public List<DamageMeterSnapshotFragment> DamageMeter
    {
        get => _damageMeter;
        set
        {
            _damageMeter = value;
            OnPropertyChanged();
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    [NotifyPropertyChangedInvocator]
    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}