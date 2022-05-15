using System.Collections.ObjectModel;
using StatisticsAnalysisTool.Properties;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using StatisticsAnalysisTool.Common.UserSettings;

namespace StatisticsAnalysisTool.Models.BindingModel;

public class DungeonBindings : INotifyPropertyChanged
{
    private DungeonStatsFilter _dungeonStatsFilter;
    private ObservableCollection<ClusterInfo> _enteredCluster = new();
    private DungeonStats _dungeonStatsDay = new();
    private DungeonStats _dungeonStatsTotal = new();
    private GridLength _gridSplitterPosition;

    public ObservableCollection<ClusterInfo> EnteredCluster
    {
        get => _enteredCluster;
        set
        {
            _enteredCluster = value;
            OnPropertyChanged();
        }
    }

    public DungeonStats DungeonStatsDay
    {
        get => _dungeonStatsDay;
        set
        {
            _dungeonStatsDay = value;
            OnPropertyChanged();
        }
    }

    public DungeonStats DungeonStatsTotal
    {
        get => _dungeonStatsTotal;
        set
        {
            _dungeonStatsTotal = value;
            OnPropertyChanged();
        }
    }

    public DungeonStatsFilter DungeonStatsFilter
    {
        get => _dungeonStatsFilter;
        set
        {
            _dungeonStatsFilter = value;
            OnPropertyChanged();
        }
    }

    public GridLength GridSplitterPosition
    {
        get => _gridSplitterPosition;
        set
        {
            _gridSplitterPosition = value;
            SettingsController.CurrentSettings.DungeonsGridSplitterPosition = _gridSplitterPosition.Value;
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