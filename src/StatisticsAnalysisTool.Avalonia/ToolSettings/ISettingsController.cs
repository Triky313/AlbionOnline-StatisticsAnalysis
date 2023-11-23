using Avalonia.Controls;

namespace StatisticsAnalysisTool.Avalonia.ToolSettings;

public interface ISettingsController
{
    void SetWindowSettings(WindowState windowState, double height, double width, double left, double top);
    void SaveSettings();
    void LoadSettings();
}