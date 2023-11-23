using Avalonia;
using Avalonia.Controls;

namespace StatisticsAnalysisTool.Avalonia.ToolSettings;

public interface ISettingsController
{
    void SetWindowSettings(WindowState windowState, double height, double width, PixelPoint position);
    void SaveSettings();
    void LoadSettings();
}