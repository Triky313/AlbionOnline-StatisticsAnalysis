namespace StatisticsAnalysisTool.ViewModels;

public class FirstStartGuideStepIndicator(int stepIndex) : BaseViewModel
{
    public int StepIndex { get; } = stepIndex;

    public bool IsActive
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public bool IsCompleted
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }
}