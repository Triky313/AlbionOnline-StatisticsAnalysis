using StatisticsAnalysisTool.Common.UserSettings;
using StatisticsAnalysisTool.Models.TranslationModel;
using StatisticsAnalysisTool.Properties;

namespace StatisticsAnalysisTool.ViewModels;

internal class InfoWindowViewModel : BaseViewModel
{
    public InfoWindowViewModel()
    {
        Init();
    }

    private void Init()
    {
        Translation = new InfoWindowTranslation();
    }

    #region Bindings

    public InfoWindowTranslation Translation
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public bool ShowNotAgainChecked
    {
        get;
        set
        {
            field = value;
            SettingsController.CurrentSettings.IsInfoWindowShownOnStart = !field;
            OnPropertyChanged();
        }
    }

    public string DonateUrl => Settings.Default.DonateUrl;

    #endregion Bindings
}