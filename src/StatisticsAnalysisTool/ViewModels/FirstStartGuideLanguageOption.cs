using StatisticsAnalysisTool.Models;

namespace StatisticsAnalysisTool.ViewModels;

public class FirstStartGuideLanguageOption(FileInformation language, string displayText, int colorIndex) : BaseViewModel
{
    public FileInformation Language { get; } = language;
    public string DisplayText { get; } = displayText;
    public int ColorIndex { get; } = colorIndex;

    public bool IsSelected
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }
}