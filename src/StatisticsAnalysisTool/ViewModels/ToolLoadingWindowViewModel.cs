using StatisticsAnalysisTool.Common;

namespace StatisticsAnalysisTool.ViewModels;

public class ToolLoadingWindowViewModel : BaseViewModel
{
    private double _progressBarValue;

    public double ProgressBarValue
    {
        get => _progressBarValue;
        set
        {
            _progressBarValue = value;
            OnPropertyChanged();
        }
    }

    public static string TranslationLoading => LanguageController.Translation("LOADING");
}