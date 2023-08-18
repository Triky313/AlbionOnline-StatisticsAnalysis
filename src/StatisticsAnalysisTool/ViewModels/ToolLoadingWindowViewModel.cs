using System.ComponentModel;
using System.Runtime.CompilerServices;
using StatisticsAnalysisTool.Common;

namespace StatisticsAnalysisTool.ViewModels;

public class ToolLoadingWindowViewModel : INotifyPropertyChanged
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

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}