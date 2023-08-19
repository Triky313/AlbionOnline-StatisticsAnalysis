using StatisticsAnalysisTool.Common.UserSettings;
using StatisticsAnalysisTool.Models.TranslationModel;
using StatisticsAnalysisTool.Properties;

namespace StatisticsAnalysisTool.ViewModels;

internal class InfoWindowViewModel : BaseViewModel
{
    private bool _showNotAgainChecked;
    private InfoWindowTranslation _translation;

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
        get => _translation;
        set
        {
            _translation = value;
            OnPropertyChanged();
        }
    }

    public bool ShowNotAgainChecked
    {
        get => _showNotAgainChecked;
        set
        {
            _showNotAgainChecked = value;
            SettingsController.CurrentSettings.IsInfoWindowShownOnStart = !_showNotAgainChecked;
            OnPropertyChanged();
        }
    }

    public string DonateUrl => Settings.Default.DonateUrl;

    #endregion Bindings
}