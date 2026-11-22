using StatisticsAnalysisTool.Common.UserSettings;
using StatisticsAnalysisTool.ViewModels;

namespace StatisticsAnalysisTool.Views.Overlay;

public partial class OverlayDamageMeterWindow
{
    public OverlayDamageMeterWindow(MainWindowViewModel mainWindowViewModel)
    {
        InitializeComponent();
        DataContext = mainWindowViewModel;
    }

    // The program's damage meter rows are ~650px wide; shrink them so the overlay fits over the game.
    protected override double BaseScale => 0.8;

    protected override (double Left, double Top) GetSavedPosition(SettingsObject settings)
        => (settings.OverlayDamageMeterLeft, settings.OverlayDamageMeterTop);

    protected override void SavePosition(SettingsObject settings, double left, double top)
    {
        settings.OverlayDamageMeterLeft = left;
        settings.OverlayDamageMeterTop = top;
    }
}
