using CommunityToolkit.Mvvm.ComponentModel;

namespace StatisticsAnalysisTool.Avalonia.ViewModels;

public partial class NetworkAdapterItemViewModel : ObservableObject
{
    public int Index { get; init; }

    public string Identifier { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;

    [ObservableProperty]
    private bool _isSelected;
}
