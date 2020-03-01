using System.Globalization;
using StatisticsAnalysisTool.Models;

namespace StatisticsAnalysisTool.ViewModels
{
    using Common;
    using Properties;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using Views;

    public class SettingsWindowViewModel : INotifyPropertyChanged
    {
        private static SettingsWindow _settingsWindow;
        private readonly MainWindowViewModel _mainWindowViewModel;
        private static string _filteredItems;
        private static ObservableCollection<LanguageController.FileInfo> _languages = new ObservableCollection<LanguageController.FileInfo>();
        private static LanguageController.FileInfo _languagesSelection;        
        private static ObservableCollection<RefreshRateStruct> _refreshRates = new ObservableCollection<RefreshRateStruct>();
        private static RefreshRateStruct _refreshRatesSelection;
        private static ObservableCollection<UpdateItemListStruct> _updateItemListByDays = new ObservableCollection<UpdateItemListStruct>();
        private static UpdateItemListStruct _updateItemListByDaysSelection;
        private SettingsWindowTranslation _translation;
        private bool _isOpenItemWindowInNewWindowChecked;

        public SettingsWindowViewModel(SettingsWindow settingsWindow, MainWindowViewModel mainWindowViewModel)
        {
            _settingsWindow = settingsWindow;
            _mainWindowViewModel = mainWindowViewModel;
            InitializeTranslation();
            InitializeSettings();
        }


        private void InitializeTranslation()
        {
            Translation = new SettingsWindowTranslation()
            {
                Settings = LanguageController.Translation("SETTINGS"),
                Language = LanguageController.Translation("LANGUAGE"),
                RefrashRate = LanguageController.Translation("REFRESH_RATE"),
                UpdateItemListByDays = LanguageController.Translation("UPDATE_ITEM_LIST_BY_DAYS"),
                ItemListSourceUrl = LanguageController.Translation("ITEM_LIST_SOURCE_URL"),
                OpenItemWindowInNewWindow = LanguageController.Translation("OPEN_ITEM_WINDOW_IN_NEW_WINDOW"),
                Save = LanguageController.Translation("SAVE")
            };
        }

        private void InitializeSettings()
        {
            // Refresh rate
            RefreshRates.Clear();
            RefreshRates.Add(new RefreshRateStruct() { Name = LanguageController.Translation("5_SECONDS"), Seconds = 5000 });
            RefreshRates.Add(new RefreshRateStruct() { Name = LanguageController.Translation("10_SECONDS"), Seconds = 10000 });
            RefreshRates.Add(new RefreshRateStruct() { Name = LanguageController.Translation("30_SECONDS"), Seconds = 30000 });
            RefreshRates.Add(new RefreshRateStruct() { Name = LanguageController.Translation("60_SECONDS"), Seconds = 60000 });
            RefreshRates.Add(new RefreshRateStruct() { Name = LanguageController.Translation("5_MINUTES"), Seconds = 300000 });
            RefreshRatesSelection = RefreshRates.FirstOrDefault(x => x.Seconds == Settings.Default.RefreshRate);

            Languages.Clear();
            foreach (var langInfos in LanguageController.LanguageFiles)
                Languages.Add(new LanguageController.FileInfo() { FileName = langInfos.FileName });

            LanguagesSelection = Languages.FirstOrDefault(x => x.FileName == LanguageController.CurrentCultureInfo.TextInfo.CultureName);

            // Update item list by days
            UpdateItemListByDays.Clear();
            UpdateItemListByDays.Add(new UpdateItemListStruct() { Name = LanguageController.Translation("EVERY_DAY"), Value = 1 });
            UpdateItemListByDays.Add(new UpdateItemListStruct() { Name = LanguageController.Translation("EVERY_3_DAYS"), Value = 3 });
            UpdateItemListByDays.Add(new UpdateItemListStruct() { Name = LanguageController.Translation("EVERY_7_DAYS"), Value = 7 });
            UpdateItemListByDays.Add(new UpdateItemListStruct() { Name = LanguageController.Translation("EVERY_14_DAYS"), Value = 14 });
            UpdateItemListByDays.Add(new UpdateItemListStruct() { Name = LanguageController.Translation("EVERY_28_DAYS"), Value = 28 });
            UpdateItemListByDaysSelection = UpdateItemListByDays.FirstOrDefault(x => x.Value == Settings.Default.UpdateItemListByDays);

            ItemListSourceUrl = Settings.Default.ItemListSourceUrl;
            IsOpenItemWindowInNewWindowChecked = Settings.Default.IsOpenItemWindowInNewWindowChecked;
        }

        public void SaveSettings()
        {
            Settings.Default.ItemListSourceUrl = ItemListSourceUrl;
            Settings.Default.RefreshRate = RefreshRatesSelection.Seconds;
            Settings.Default.UpdateItemListByDays = UpdateItemListByDaysSelection.Value;
            Settings.Default.IsOpenItemWindowInNewWindowChecked = IsOpenItemWindowInNewWindowChecked;

            LanguageController.CurrentCultureInfo = new CultureInfo(LanguagesSelection.FileName);
            LanguageController.SetLanguage();

            SetAppTranslations();

            _settingsWindow.Close();
        }

        private void SetAppTranslations()
        {
            InitializeTranslation();

            _mainWindowViewModel.SetModeCombobox();
            _mainWindowViewModel.PlayerModeTranslation = new PlayerModeTranslation();
            _mainWindowViewModel.LoadTranslation = LanguageController.Translation("LOAD");
            _mainWindowViewModel.NumberOfValuesTranslation = LanguageController.Translation("NUMBER_OF_VALUES");
            _mainWindowViewModel.UpdateTranslation = LanguageController.Translation("UPDATE");
        }

        public UpdateItemListStruct UpdateItemListByDaysSelection
        {
            get => _updateItemListByDaysSelection;
            set
            {
                _updateItemListByDaysSelection = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<UpdateItemListStruct> UpdateItemListByDays
        {
            get => _updateItemListByDays;
            set
            {
                _updateItemListByDays = value;
                OnPropertyChanged();
            }
        }

        public RefreshRateStruct RefreshRatesSelection
        {
            get => _refreshRatesSelection;
            set
            {
                _refreshRatesSelection = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<RefreshRateStruct> RefreshRates
        {
            get => _refreshRates;
            set
            {
                _refreshRates = value;
                OnPropertyChanged();
            }
        }

        public LanguageController.FileInfo LanguagesSelection
        {
            get => _languagesSelection;
            set
            {
                _languagesSelection = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<LanguageController.FileInfo> Languages
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
            get => _filteredItems;
            set
            {
                _filteredItems = value;
                OnPropertyChanged();
            }
        }

        public SettingsWindowTranslation Translation {
            get => _translation;
            set {
                _translation = value;
                OnPropertyChanged();
            }
        }

        public bool IsOpenItemWindowInNewWindowChecked {
            get => _isOpenItemWindowInNewWindowChecked;
            set {
                _isOpenItemWindowInNewWindowChecked = value;
                OnPropertyChanged();
            }
        }
        
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public struct RefreshRateStruct
        {
            public string Name { get; set; }
            public int Seconds { get; set; }
        }

        public struct UpdateItemListStruct
        {
            public string Name { get; set; }
            public int Value { get; set; }
        }
    }
}