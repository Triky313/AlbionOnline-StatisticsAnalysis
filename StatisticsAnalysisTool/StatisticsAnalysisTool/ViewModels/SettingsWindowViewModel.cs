using log4net;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Common.UserSettings;
using StatisticsAnalysisTool.Models;
using StatisticsAnalysisTool.Properties;
using StatisticsAnalysisTool.Views;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace StatisticsAnalysisTool.ViewModels
{
    public class SettingsWindowViewModel : INotifyPropertyChanged
    {
        private static SettingsWindow _settingsWindow;
        private static string _itemListSourceUrl;
        private static ObservableCollection<FileInformation> _languages = new();
        private static FileInformation _languagesSelection;
        private static ObservableCollection<FileSettingInformation> _refreshRates = new();
        private static FileSettingInformation _refreshRatesSelection;
        private static ObservableCollection<FileSettingInformation> _updateItemListByDays = new();
        private static FileSettingInformation _updateItemListByDaysSelection;
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);
        private readonly MainWindowViewModel _mainWindowViewModel;
        private ObservableCollection<FileInformation> _alertSounds = new();
        private FileInformation _alertSoundSelection;
        private int _fullItemInformationUpdateCycleDays;
        private bool _isOpenItemWindowInNewWindowChecked;
        private bool _showInfoWindowOnStartChecked;
        private SettingsWindowTranslation _translation;
        private string _cityPricesApiUrl;
        private string _cityPricesHistoryApiUrl;
        private string _goldStatsApiUrl;
        private bool _isLootLoggerSaveReminderActive;
        private string _automaticLootLoggerSavePath;

        public SettingsWindowViewModel(SettingsWindow settingsWindow, MainWindowViewModel mainWindowViewModel)
        {
            _settingsWindow = settingsWindow;
            _mainWindowViewModel = mainWindowViewModel;
            Translation = new SettingsWindowTranslation();
            InitializeSettings();
        }

        private void InitializeSettings()
        {
            #region Refrash rate

            RefreshRates.Clear();
            RefreshRates.Add(new FileSettingInformation {Name = LanguageController.Translation("5_SECONDS"), Value = 5000});
            RefreshRates.Add(new FileSettingInformation {Name = LanguageController.Translation("10_SECONDS"), Value = 10000});
            RefreshRates.Add(new FileSettingInformation {Name = LanguageController.Translation("30_SECONDS"), Value = 30000});
            RefreshRates.Add(new FileSettingInformation {Name = LanguageController.Translation("60_SECONDS"), Value = 60000});
            RefreshRates.Add(new FileSettingInformation {Name = LanguageController.Translation("5_MINUTES"), Value = 300000});
            RefreshRatesSelection = RefreshRates.FirstOrDefault(x => x.Value == SettingsController.CurrentSettings.RefreshRate);

            #endregion

            #region Language

            Languages.Clear();
            foreach (var langInfo in LanguageController.LanguageFiles)
                try
                {
                    var cultureInfo = CultureInfo.CreateSpecificCulture(langInfo.FileName);
                    Languages.Add(new FileInformation(langInfo.FileName, string.Empty)
                    {
                        EnglishName = cultureInfo.EnglishName,
                        NativeName = cultureInfo.NativeName
                    });
                }
                catch (CultureNotFoundException e)
                {
                    ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
                    Log.Error(MethodBase.GetCurrentMethod()?.DeclaringType, e);
                }

            LanguagesSelection = Languages.FirstOrDefault(x => x.FileName == LanguageController.CurrentCultureInfo.TextInfo.CultureName);

            #endregion

            #region Update item list by days

            UpdateItemListByDays.Clear();
            UpdateItemListByDays.Add(new FileSettingInformation {Name = LanguageController.Translation("EVERY_DAY"), Value = 1});
            UpdateItemListByDays.Add(new FileSettingInformation {Name = LanguageController.Translation("EVERY_3_DAYS"), Value = 3});
            UpdateItemListByDays.Add(new FileSettingInformation {Name = LanguageController.Translation("EVERY_7_DAYS"), Value = 7});
            UpdateItemListByDays.Add(new FileSettingInformation {Name = LanguageController.Translation("EVERY_14_DAYS"), Value = 14});
            UpdateItemListByDays.Add(new FileSettingInformation {Name = LanguageController.Translation("EVERY_28_DAYS"), Value = 28});
            UpdateItemListByDaysSelection = UpdateItemListByDays.FirstOrDefault(x => x.Value == SettingsController.CurrentSettings.UpdateItemListByDays);

            ItemListSourceUrl = SettingsController.CurrentSettings.ItemListSourceUrl;
            IsOpenItemWindowInNewWindowChecked = SettingsController.CurrentSettings.IsOpenItemWindowInNewWindowChecked;
            ShowInfoWindowOnStartChecked = SettingsController.CurrentSettings.IsInfoWindowShownOnStart;
            FullItemInformationUpdateCycleDays = SettingsController.CurrentSettings.FullItemInformationUpdateCycleDays;

            #endregion

            #region Alert Sounds

            AlertSounds.Clear();
            foreach (var sound in SoundController.AlertSounds)
            {
                AlertSounds.Add(new FileInformation(sound.FileName, sound.FilePath));
            }

            AlertSoundSelection = AlertSounds.FirstOrDefault(x => x.FileName == SettingsController.CurrentSettings.SelectedAlertSound);

            #endregion

            #region Api urls

            CityPricesApiUrl = SettingsController.CurrentSettings.CityPricesApiUrl;
            CityPricesHistoryApiUrl = SettingsController.CurrentSettings.CityPricesHistoryApiUrl;
            GoldStatsApiUrl = SettingsController.CurrentSettings.GoldStatsApiUrl;

            #endregion

            #region Loot logger

            IsLootLoggerSaveReminderActive = SettingsController.CurrentSettings.IsLootLoggerSaveReminderActive;

            #endregion
        }

        public void SaveSettings()
        {
            SettingsController.CurrentSettings.ItemListSourceUrl = ItemListSourceUrl;
            SettingsController.CurrentSettings.RefreshRate = RefreshRatesSelection.Value;
            SettingsController.CurrentSettings.UpdateItemListByDays = UpdateItemListByDaysSelection.Value;
            SettingsController.CurrentSettings.IsOpenItemWindowInNewWindowChecked = IsOpenItemWindowInNewWindowChecked;
            SettingsController.CurrentSettings.IsInfoWindowShownOnStart = ShowInfoWindowOnStartChecked;
            SettingsController.CurrentSettings.FullItemInformationUpdateCycleDays = FullItemInformationUpdateCycleDays;
            SettingsController.CurrentSettings.SelectedAlertSound = AlertSoundSelection?.FileName ?? string.Empty;

            LanguageController.CurrentCultureInfo = new CultureInfo(LanguagesSelection.FileName);
            LanguageController.SetLanguage();

            SettingsController.CurrentSettings.CityPricesApiUrl = string.IsNullOrEmpty(CityPricesApiUrl) ? Settings.Default.CityPricesApiUrlDefault : CityPricesApiUrl;
            SettingsController.CurrentSettings.CityPricesHistoryApiUrl = string.IsNullOrEmpty(CityPricesHistoryApiUrl) ? Settings.Default.CityPricesHistoryApiUrlDefault : CityPricesHistoryApiUrl;
            SettingsController.CurrentSettings.GoldStatsApiUrl = string.IsNullOrEmpty(GoldStatsApiUrl) ? Settings.Default.GoldStatsApiUrlDefault : GoldStatsApiUrl;

            SettingsController.CurrentSettings.IsLootLoggerSaveReminderActive = IsLootLoggerSaveReminderActive;

            SetAppSettingsAndTranslations();

            _settingsWindow.Close();
        }

        private void SetAppSettingsAndTranslations()
        {
            Translation = new SettingsWindowTranslation();

            _mainWindowViewModel.SetUiElements();
            _mainWindowViewModel.IsFullItemInformationCompleteCheck();
            _mainWindowViewModel.PlayerModeTranslation = new PlayerModeTranslation();
            _mainWindowViewModel.LoadTranslation = LanguageController.Translation("LOAD");
            _mainWindowViewModel.NumberOfValuesTranslation = LanguageController.Translation("NUMBER_OF_VALUES");
            _mainWindowViewModel.UpdateTranslation = LanguageController.Translation("UPDATE");
        }

        public struct FileSettingInformation
        {
            public string Name { get; set; }
            public int Value { get; set; }
        }

        #region Bindings

        public ObservableCollection<FileInformation> AlertSounds
        {
            get => _alertSounds;
            set
            {
                _alertSounds = value;
                OnPropertyChanged();
            }
        }

        public FileInformation AlertSoundSelection
        {
            get => _alertSoundSelection;
            set
            {
                _alertSoundSelection = value;
                OnPropertyChanged();
            }
        }

        public int FullItemInformationUpdateCycleDays
        {
            get => _fullItemInformationUpdateCycleDays;
            set
            {
                _fullItemInformationUpdateCycleDays = value;
                OnPropertyChanged();
            }
        }

        public FileSettingInformation UpdateItemListByDaysSelection
        {
            get => _updateItemListByDaysSelection;
            set
            {
                _updateItemListByDaysSelection = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<FileSettingInformation> UpdateItemListByDays
        {
            get => _updateItemListByDays;
            set
            {
                _updateItemListByDays = value;
                OnPropertyChanged();
            }
        }

        public FileSettingInformation RefreshRatesSelection
        {
            get => _refreshRatesSelection;
            set
            {
                _refreshRatesSelection = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<FileSettingInformation> RefreshRates
        {
            get => _refreshRates;
            set
            {
                _refreshRates = value;
                OnPropertyChanged();
            }
        }

        public FileInformation LanguagesSelection
        {
            get => _languagesSelection;
            set
            {
                _languagesSelection = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<FileInformation> Languages
        {
            get => _languages;
            set
            {
                _languages = value;
                OnPropertyChanged();
            }
        }

        public string ItemListSourceUrl
        {
            get => _itemListSourceUrl;
            set
            {
                _itemListSourceUrl = value;
                OnPropertyChanged();
            }
        }

        public SettingsWindowTranslation Translation
        {
            get => _translation;
            set
            {
                _translation = value;
                OnPropertyChanged();
            }
        }

        public bool IsOpenItemWindowInNewWindowChecked
        {
            get => _isOpenItemWindowInNewWindowChecked;
            set
            {
                _isOpenItemWindowInNewWindowChecked = value;
                OnPropertyChanged();
            }
        }

        public bool ShowInfoWindowOnStartChecked
        {
            get => _showInfoWindowOnStartChecked;
            set {
                _showInfoWindowOnStartChecked = value;
                OnPropertyChanged();
            }
        }

        public string CityPricesApiUrl
        {
            get => _cityPricesApiUrl;
            set
            {
                _cityPricesApiUrl = value;
                OnPropertyChanged();
            }
        }

        public string CityPricesHistoryApiUrl
        {
            get => _cityPricesHistoryApiUrl;
            set
            {
                _cityPricesHistoryApiUrl = value;
                OnPropertyChanged();
            }
        }

        public string GoldStatsApiUrl
        {
            get => _goldStatsApiUrl;
            set
            {
                _goldStatsApiUrl = value;
                OnPropertyChanged();
            }
        }

        public bool IsLootLoggerSaveReminderActive
        {
            get => _isLootLoggerSaveReminderActive;
            set
            {
                _isLootLoggerSaveReminderActive = value;
                OnPropertyChanged();
            }
        }

        public string AutomaticLootLoggerSavePath
        {
            get => _automaticLootLoggerSavePath;
            set
            {
                _automaticLootLoggerSavePath = value;
                OnPropertyChanged();
            }
        }

        public string ToolDirectory => System.AppDomain.CurrentDomain.BaseDirectory;

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion Bindings
    }
}