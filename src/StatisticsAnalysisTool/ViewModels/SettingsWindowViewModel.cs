using System;
using log4net;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Common.UserSettings;
using StatisticsAnalysisTool.Models;
using StatisticsAnalysisTool.Properties;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows;
using StatisticsAnalysisTool.Models.TranslationModel;
using StatisticsAnalysisTool.Views;

namespace StatisticsAnalysisTool.ViewModels
{
    public class SettingsWindowViewModel : INotifyPropertyChanged
    {
        private static string _itemListSourceUrl;
        private static ObservableCollection<FileInformation> _languages = new();
        private static FileInformation _languagesSelection;
        private static ObservableCollection<FileSettingInformation> _refreshRates = new();
        private static FileSettingInformation _refreshRatesSelection;
        private static ObservableCollection<FileSettingInformation> _updateItemListByDays = new();
        private static FileSettingInformation _updateItemListByDaysSelection;
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);
        private ObservableCollection<FileInformation> _alertSounds = new();
        private FileInformation _alertSoundSelection;
        private bool _isOpenItemWindowInNewWindowChecked;
        private bool _showInfoWindowOnStartChecked;
        private SettingsWindowTranslation _translation;
        private string _cityPricesApiUrl;
        private string _cityPricesHistoryApiUrl;
        private string _goldStatsApiUrl;
        private bool _isLootLoggerSaveReminderActive;
        private string _itemsJsonSourceUrl;
        private ObservableCollection<FileSettingInformation> _updateItemsJsonByDays = new();
        private FileSettingInformation _updateItemsJsonByDaysSelection;
        private bool _isSuggestPreReleaseUpdatesActive;
        private string _mainTrackingCharacterName;
        private bool _shortDamageMeterToClipboard;

        public SettingsWindowViewModel()
        {
            InitializeSettings();
            Translation = new SettingsWindowTranslation();
        }

        private void InitializeSettings()
        {
            #region Language

            SettingsController.LoadSettings();

            Languages.Clear();
            LanguageController.InitializeLanguage();

            foreach (var langInfo in LanguageController.LanguageFiles)
            {
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
            }

            LanguagesSelection = Languages.FirstOrDefault(x => x.FileName == LanguageController.CurrentCultureInfo.TextInfo.CultureName);

            #endregion

            #region Refrash rate

            RefreshRates.Clear();
            RefreshRates.Add(new FileSettingInformation { Name = SettingsWindowTranslation.FiveSeconds, Value = 5000 });
            RefreshRates.Add(new FileSettingInformation { Name = SettingsWindowTranslation.TenSeconds, Value = 10000 });
            RefreshRates.Add(new FileSettingInformation { Name = SettingsWindowTranslation.ThirtySeconds, Value = 30000 });
            RefreshRates.Add(new FileSettingInformation { Name = SettingsWindowTranslation.SixtySeconds, Value = 60000 });
            RefreshRates.Add(new FileSettingInformation { Name = SettingsWindowTranslation.FiveMinutes, Value = 300000 });
            RefreshRatesSelection = RefreshRates.FirstOrDefault(x => x.Value == SettingsController.CurrentSettings.RefreshRate);

            #endregion

            #region MainTrackingCharacterName

            MainTrackingCharacterName = SettingsController.CurrentSettings.MainTrackingCharacterName;

            #endregion

            #region Update item list by days

            UpdateItemListByDays.Clear();
            UpdateItemListByDays.Add(new FileSettingInformation { Name = LanguageController.Translation("EVERY_DAY"), Value = 1 });
            UpdateItemListByDays.Add(new FileSettingInformation { Name = LanguageController.Translation("EVERY_3_DAYS"), Value = 3 });
            UpdateItemListByDays.Add(new FileSettingInformation { Name = LanguageController.Translation("EVERY_7_DAYS"), Value = 7 });
            UpdateItemListByDays.Add(new FileSettingInformation { Name = LanguageController.Translation("EVERY_14_DAYS"), Value = 14 });
            UpdateItemListByDays.Add(new FileSettingInformation { Name = LanguageController.Translation("EVERY_28_DAYS"), Value = 28 });
            UpdateItemListByDaysSelection = UpdateItemListByDays.FirstOrDefault(x => x.Value == SettingsController.CurrentSettings.UpdateItemListByDays);

            ItemListSourceUrl = SettingsController.CurrentSettings.ItemListSourceUrl;

            #endregion

            #region Update items.json by days

            UpdateItemsJsonByDays.Clear();
            UpdateItemsJsonByDays.Add(new FileSettingInformation { Name = LanguageController.Translation("EVERY_DAY"), Value = 1 });
            UpdateItemsJsonByDays.Add(new FileSettingInformation { Name = LanguageController.Translation("EVERY_3_DAYS"), Value = 3 });
            UpdateItemsJsonByDays.Add(new FileSettingInformation { Name = LanguageController.Translation("EVERY_7_DAYS"), Value = 7 });
            UpdateItemsJsonByDays.Add(new FileSettingInformation { Name = LanguageController.Translation("EVERY_14_DAYS"), Value = 14 });
            UpdateItemsJsonByDays.Add(new FileSettingInformation { Name = LanguageController.Translation("EVERY_28_DAYS"), Value = 28 });
            UpdateItemsJsonByDaysSelection = UpdateItemsJsonByDays.FirstOrDefault(x => x.Value == SettingsController.CurrentSettings.UpdateItemsJsonByDays);

            ItemsJsonSourceUrl = SettingsController.CurrentSettings.ItemsJsonSourceUrl;
            
            #endregion

            #region Alert Sounds

            AlertSounds.Clear();
            SoundController.InitializeSoundFilesFromDirectory();
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

            #region Auto update

            IsSuggestPreReleaseUpdatesActive = SettingsController.CurrentSettings.IsSuggestPreReleaseUpdatesActive;

            #endregion

            #region Damage Meter

            ShortDamageMeterToClipboard = SettingsController.CurrentSettings.ShortDamageMeterToClipboard;

            #endregion
            
            IsOpenItemWindowInNewWindowChecked = SettingsController.CurrentSettings.IsOpenItemWindowInNewWindowChecked;
            ShowInfoWindowOnStartChecked = SettingsController.CurrentSettings.IsInfoWindowShownOnStart;
        }

        public void SaveSettings()
        {
            SettingsController.CurrentSettings.ItemListSourceUrl = ItemListSourceUrl;
            SettingsController.CurrentSettings.ItemsJsonSourceUrl = ItemsJsonSourceUrl;
            SettingsController.CurrentSettings.RefreshRate = RefreshRatesSelection.Value;
            SettingsController.CurrentSettings.MainTrackingCharacterName = MainTrackingCharacterName;
            SettingsController.CurrentSettings.UpdateItemListByDays = UpdateItemListByDaysSelection.Value;
            SettingsController.CurrentSettings.UpdateItemsJsonByDays = UpdateItemsJsonByDaysSelection.Value;
            SettingsController.CurrentSettings.IsOpenItemWindowInNewWindowChecked = IsOpenItemWindowInNewWindowChecked;
            SettingsController.CurrentSettings.IsInfoWindowShownOnStart = ShowInfoWindowOnStartChecked;
            SettingsController.CurrentSettings.SelectedAlertSound = AlertSoundSelection?.FileName ?? string.Empty;

            LanguageController.CurrentCultureInfo = new CultureInfo(LanguagesSelection.FileName);
            LanguageController.SetLanguage();

            SettingsController.CurrentSettings.CityPricesApiUrl = string.IsNullOrEmpty(CityPricesApiUrl) ? Settings.Default.CityPricesApiUrlDefault : CityPricesApiUrl;
            SettingsController.CurrentSettings.CityPricesHistoryApiUrl = string.IsNullOrEmpty(CityPricesHistoryApiUrl) ? Settings.Default.CityPricesHistoryApiUrlDefault : CityPricesHistoryApiUrl;
            SettingsController.CurrentSettings.GoldStatsApiUrl = string.IsNullOrEmpty(GoldStatsApiUrl) ? Settings.Default.GoldStatsApiUrlDefault : GoldStatsApiUrl;

            SettingsController.CurrentSettings.IsLootLoggerSaveReminderActive = IsLootLoggerSaveReminderActive;
            SettingsController.CurrentSettings.IsSuggestPreReleaseUpdatesActive = IsSuggestPreReleaseUpdatesActive;
            SettingsController.CurrentSettings.ShortDamageMeterToClipboard = ShortDamageMeterToClipboard;

            SetAppSettingsAndTranslations();
        }

        public void ReloadSettings()
        {
            MainTrackingCharacterName = SettingsController.CurrentSettings.MainTrackingCharacterName;
        }

        private void SetAppSettingsAndTranslations()
        {
            Translation = new SettingsWindowTranslation();
        }

        public struct FileSettingInformation
        {
            public string Name { get; set; }
            public int Value { get; set; }
        }

        public void OpenConsoleWindow()
        {
            try
            {
                if (Utilities.IsWindowOpen<ConsoleWindow>())
                {
                    var existWindow = Application.Current.Windows.OfType<ConsoleWindow>().FirstOrDefault();
                    existWindow?.Activate();
                }
                else
                {
                    var window = new ConsoleWindow();
                    window.Show();
                }
            }
            catch (Exception e)
            {
                ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
                Log.Error(MethodBase.GetCurrentMethod()?.DeclaringType, e);
            }
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
        
        public FileSettingInformation UpdateItemListByDaysSelection
        {
            get => _updateItemListByDaysSelection;
            set
            {
                _updateItemListByDaysSelection = value;
                OnPropertyChanged();
            }
        }

        public FileSettingInformation UpdateItemsJsonByDaysSelection
        {
            get => _updateItemsJsonByDaysSelection;
            set
            {
                _updateItemsJsonByDaysSelection = value;
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

        public ObservableCollection<FileSettingInformation> UpdateItemsJsonByDays
        {
            get => _updateItemsJsonByDays;
            set
            {
                _updateItemsJsonByDays = value;
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

        public string MainTrackingCharacterName
        {
            get => _mainTrackingCharacterName;
            set
            {
                _mainTrackingCharacterName = value;
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

        public string ItemsJsonSourceUrl
        {
            get => _itemsJsonSourceUrl;
            set
            {
                _itemsJsonSourceUrl = value;
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
            set
            {
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
        
        public bool ShortDamageMeterToClipboard
        {
            get => _shortDamageMeterToClipboard;
            set
            {
                _shortDamageMeterToClipboard = value;
                OnPropertyChanged();
            }
        }
        
        public bool IsSuggestPreReleaseUpdatesActive
        {
            get => _isSuggestPreReleaseUpdatesActive;
            set
            {
                _isSuggestPreReleaseUpdatesActive = value;
                OnPropertyChanged();
            }
        }
        
        public string ToolDirectory => AppDomain.CurrentDomain.BaseDirectory;

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion Bindings
    }
}