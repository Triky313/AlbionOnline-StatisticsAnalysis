using StatisticsAnalysisTool.Localization;
using StatisticsAnalysisTool.ViewModels;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace StatisticsAnalysisTool.Gathering;

public sealed class GatheringChartSeriesFilter : BaseViewModel
{
    private bool _isSelected = true;

    public GatheringResourceType ResourceType
    {
        get;
        init;
    }

    public string Name
    {
        get;
        init;
    } = string.Empty;

    public Brush Brush
    {
        get;
        init;
    } = Brushes.Transparent;

    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            _isSelected = value;
            OnPropertyChanged();
        }
    }

    public static IReadOnlyList<GatheringChartSeriesFilter> CreateDefault()
    {
        return
        [
            new GatheringChartSeriesFilter
            {
                ResourceType = GatheringResourceType.Wood,
                Name = LocalizationController.Translation("WOOD"),
                Brush = GetBrush(GatheringResourceType.Wood)
            },
            new GatheringChartSeriesFilter
            {
                ResourceType = GatheringResourceType.Fiber,
                Name = LocalizationController.Translation("FIBER"),
                Brush = GetBrush(GatheringResourceType.Fiber)
            },
            new GatheringChartSeriesFilter
            {
                ResourceType = GatheringResourceType.Hide,
                Name = LocalizationController.Translation("HIDE"),
                Brush = GetBrush(GatheringResourceType.Hide)
            },
            new GatheringChartSeriesFilter
            {
                ResourceType = GatheringResourceType.Ore,
                Name = LocalizationController.Translation("ORE"),
                Brush = GetBrush(GatheringResourceType.Ore)
            },
            new GatheringChartSeriesFilter
            {
                ResourceType = GatheringResourceType.Rock,
                Name = LocalizationController.Translation("ROCK"),
                Brush = GetBrush(GatheringResourceType.Rock)
            },
            new GatheringChartSeriesFilter
            {
                ResourceType = GatheringResourceType.Fishing,
                Name = LocalizationController.Translation("FISHING"),
                Brush = GetBrush(GatheringResourceType.Fishing)
            }
        ];
    }

    public static Brush GetBrush(GatheringResourceType resourceType)
    {
        try
        {
            return (Brush) Application.Current.Resources[$"SolidColorBrush.Resource.{resourceType}"];
        }
        catch
        {
            return Brushes.Transparent;
        }
    }
}