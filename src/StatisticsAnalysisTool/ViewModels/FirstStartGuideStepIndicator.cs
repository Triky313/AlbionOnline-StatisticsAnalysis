namespace StatisticsAnalysisTool.ViewModels;

public class FirstStartGuideStepIndicator : BaseViewModel
{
    private bool _isActive;
    private bool _isCompleted;

    public FirstStartGuideStepIndicator(int stepIndex)
    {
        StepIndex = stepIndex;
    }

    public int StepIndex { get; }

    public bool IsActive
    {
        get => _isActive;
        set
        {
            _isActive = value;
            OnPropertyChanged();
        }
    }

    public bool IsCompleted
    {
        get => _isCompleted;
        set
        {
            _isCompleted = value;
            OnPropertyChanged();
        }
    }
}