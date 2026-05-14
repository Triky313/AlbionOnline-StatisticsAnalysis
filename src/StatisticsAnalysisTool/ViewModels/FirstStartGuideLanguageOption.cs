using StatisticsAnalysisTool.Models;

namespace StatisticsAnalysisTool.ViewModels;

public class FirstStartGuideLanguageOption : BaseViewModel
{
    private bool _isSelected;

    public FirstStartGuideLanguageOption(FileInformation language, string displayText, int colorIndex)
    {
        Language = language;
        DisplayText = displayText;
        ColorIndex = colorIndex;
    }

    public FileInformation Language { get; }
    public string DisplayText { get; }
    public int ColorIndex { get; }

    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            _isSelected = value;
            OnPropertyChanged();
        }
    }
}