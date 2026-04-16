using StatisticsAnalysisTool.ViewModels;
using System.Windows;
using System.Windows.Media;
using ValueType = StatisticsAnalysisTool.Enumerations.ValueType;

namespace StatisticsAnalysisTool.Models;

public sealed class DashboardChartSeriesFilter : BaseViewModel
{
    private bool _isSelected = true;

    public ValueType ValueType
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

    public static Brush GetBrush(ValueType valueType)
    {
        try
        {
            return (Brush) Application.Current.Resources[$"SolidColorBrush.Value.{valueType}"];
        }
        catch
        {
            return Brushes.Transparent;
        }
    }
}
