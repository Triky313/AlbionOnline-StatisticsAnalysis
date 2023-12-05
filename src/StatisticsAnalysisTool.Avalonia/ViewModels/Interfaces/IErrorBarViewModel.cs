namespace StatisticsAnalysisTool.Avalonia.ViewModels.Interfaces;

public interface IErrorBarViewModel
{
    void Set(bool isVisible, string errorMessage);
    void Close();
}