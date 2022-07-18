using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Properties;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using StatisticsAnalysisTool.Network.Notification;

namespace StatisticsAnalysisTool.Models.BindingModel;

public class DamageMeterBindings : INotifyPropertyChanged
{
    private ObservableCollection<DamageMeterFragment> _damageMeter = new();
    private List<DamageMeterSnapshot> _damageMeterSnapshots = new();
    private DamageMeterSnapshot _damageMeterSnapshotSelection;

    public ObservableCollection<DamageMeterFragment> DamageMeter
    {
        get => _damageMeter;
        set
        {
            _damageMeter = value;
            OnPropertyChanged();
        }
    }

    #region Damage Meter Snapshot

    public List<DamageMeterSnapshot> DamageMeterSnapshots
    {
        get => _damageMeterSnapshots;
        set
        {
            _damageMeterSnapshots = value;
            OnPropertyChanged();
        }
    }

    public DamageMeterSnapshot DamageMeterSnapshotSelection
    {
        get => _damageMeterSnapshotSelection;
        set
        {
            _damageMeterSnapshotSelection = value;
            OnPropertyChanged();
        }
    }

    public void GetSnapshot()
    {
        var snapshots = DamageMeterSnapshots;

        var damageMeterSnapshot = new DamageMeterSnapshot();
        foreach (var damageMeterFragment in DamageMeter)
        {
            damageMeterSnapshot.DamageMeter.Add(new DamageMeterSnapshotFragment(damageMeterFragment));
        }

        DamageMeterSnapshots?.Add(damageMeterSnapshot);

        Application.Current.Dispatcher.Invoke(() =>
        {
            DamageMeterSnapshots = snapshots.OrderByDescending(x => x.Timestamp).ToList();
        });
    }

    public void DeleteSnapshot()
    {
        var damageMeterSnapshotSelection = DamageMeterSnapshotSelection;
        if (damageMeterSnapshotSelection != null)
        {
            DamageMeterSnapshots?.Remove(damageMeterSnapshotSelection);
        }

        DamageMeterSnapshots = DamageMeterSnapshots?.ToList();
    }

    #endregion

    public event PropertyChangedEventHandler PropertyChanged;

    [NotifyPropertyChangedInvocator]
    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}