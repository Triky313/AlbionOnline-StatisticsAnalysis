using StatisticsAnalysisTool.Common.UserSettings;
using StatisticsAnalysisTool.ViewModels;

namespace StatisticsAnalysisTool.Views.Overlay;

public partial class OverlayGatheringWindow
{
    public OverlayGatheringWindow(MainWindowViewModel mainWindowViewModel)
    {
        InitializeComponent();
        DataContext = mainWindowViewModel;
    }

    protected override (double Left, double Top) GetSavedPosition(SettingsObject settings)
        => (settings.OverlayGatheringLeft, settings.OverlayGatheringTop);

    protected override void SavePosition(SettingsObject settings, double left, double top)
    {
        settings.OverlayGatheringLeft = left;
        settings.OverlayGatheringTop = top;
    }
}
