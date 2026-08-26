using StatisticsAnalysisTool.Cluster;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.ViewModels;
using System.Windows;

namespace StatisticsAnalysisTool.Models;

public class CurrentMapInfoBinding : BaseViewModel
{
    public ClusterInfo ClusterInfo { get; set; }

    public string DisplayedClusterName
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = string.Empty;

    public ClusterMode ClusterMode
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public string Tier
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = string.Empty;

    public Visibility CurrentMapInformationVisibility
    {
        get;
        set
        {
            field = value;
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