using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Localization;
using StatisticsAnalysisTool.ViewModels;

namespace StatisticsAnalysisTool.Dungeon.Models;

public class CheckPoint : BaseViewModel
{
    private CheckPointStatus _status;
    public int Id { get; set; }

    public CheckPointStatus Status
    {
        get => _status;
        set
        {
            _status = value;
            OnPropertyChanged();
        }
    }

    public static string TranslationCheckPoint => LocalizationController.Translation("CHECKPOINT");
}