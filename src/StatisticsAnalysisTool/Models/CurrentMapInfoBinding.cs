using StatisticsAnalysisTool.Cluster;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.ViewModels;
using System.Windows;

namespace StatisticsAnalysisTool.Models;

public class CurrentMapInfoBinding : BaseViewModel
{
    private string _displayedClusterName = string.Empty;
    private ClusterMode _clusterMode;
    private string _tier = string.Empty;
    private Visibility _currentMapInformationVisibility;

    public ClusterInfo ClusterInfo { get; set; }

    public string DisplayedClusterName
    {
        get => _displayedClusterName;
        set
        {
            _displayedClusterName = value;
            OnPropertyChanged();
        }
    }

    public ClusterMode ClusterMode
    {
        get => _clusterMode;
        set
        {
            _clusterMode = value;
            OnPropertyChanged();
        }
    }

    public string Tier
    {
        get => _tier;
        set
        {
            _tier = value;
            OnPropertyChanged();
        }
    }

    public Visibility CurrentMapInformationVisibility
    {
        get => _currentMapInformationVisibility;
        set
        {
            _currentMapInformationVisibility = value;
            OnPropertyChanged();
        }
    }

    public void ComposingMapInfoString(ClusterInfo clusterInfo)
    {
        var currentMapName = ClusterController.GetClusterDisplayName(clusterInfo);

        CurrentMapInformationVisibility = !string.IsNullOrEmpty(currentMapName) ? Visibility.Visible : Visibility.Collapsed;
        DisplayedClusterName = currentMapName;
    }
}