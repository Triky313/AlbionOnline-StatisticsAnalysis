using StatisticsAnalysisTool.Localization;

namespace StatisticsAnalysisTool.ViewModels;

public class ToolLoadingWindowViewModel : BaseViewModel
{
    private string _currentTaskName = string.Empty;
    private double _progressBarValue;

    public string CurrentTaskName
    {
        get => _currentTaskName;
        private set
        {
            _currentTaskName = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(LoadingText));
        }
    }

    public double ProgressBarValue
    {
        get => _progressBarValue;
        set
        {
            _progressBarValue = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(LoadingText));
        }
    }

    public static string TranslationLoading => LocalizationController.Translation("LOADING");
    public string LoadingText => string.IsNullOrWhiteSpace(CurrentTaskName)
        ? $"{TranslationLoading} ({ProgressBarValue:N0}%)"
        : $"{TranslationLoading}: {CurrentTaskName} ({ProgressBarValue:N0}%)";

    public void UpdateProgress(double progressBarValue, string currentTaskName)
    {
        CurrentTaskName = currentTaskName ?? string.Empty;
        ProgressBarValue = System.Math.Clamp(progressBarValue, 0, 100);
    }
}
