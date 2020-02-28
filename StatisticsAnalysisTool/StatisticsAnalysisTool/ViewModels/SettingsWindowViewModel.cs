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
        private static string _filteredItems;
        private static ObservableCollection<LanguageController.FileInfo> _languages = new ObservableCollection<LanguageController.FileInfo>();
        private static LanguageController.FileInfo _languagesSelection;        
        private static ObservableCollection<RefreshRateStruct> _refreshRates = new ObservableCollection<RefreshRateStruct>();
        private static RefreshRateStruct _refreshRatesSelection;
        private static ObservableCollection<UpdateItemListStruct> _updateItemListByDays = new ObservableCollection<UpdateItemListStruct>();
        private static UpdateItemListStruct _updateItemListByDaysSelection;

        public SettingsWindowViewModel(SettingsWindow settingsWindow)
        {
            _settingsWindow = settingsWindow;
            InitializeTranslation();
            InitializeSettings();
        }


        private void InitializeTranslation()
        {
            _settingsWindow.LblSettingsWindowTitle.Content = LanguageController.Translation("SETTINGS");
            _settingsWindow.LblLanguage.Content = $"{LanguageController.Translation("LANGUAGE")}:";
            _settingsWindow.LblRefrashRate.Content = $"{LanguageController.Translation("REFRESH_RATE")}:";
            _settingsWindow.LblUpdateItemListByDays.Content = $"{LanguageController.Translation("UPDATE_ITEM_LIST_BY_DAYS")}";
            _settingsWindow.LblItemListSourceUrl.Content = $"{LanguageController.Translation("ITEM_LIST_SOURCE_URL")}";
            _settingsWindow.BtnSave.Content = $"{LanguageController.Translation("SAVE")}";
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
            foreach (var langInfos in LanguageController.FileInfos)
                Languages.Add(new LanguageController.FileInfo() { FileName = langInfos.FileName });

            LanguagesSelection = Languages.FirstOrDefault(x => x.FileName == LanguageController.CurrentCultureInfo.IetfLanguageTag);

            // Update item list by days
            UpdateItemListByDays.Clear();
            UpdateItemListByDays.Add(new UpdateItemListStruct() { Name = LanguageController.Translation("EVERY_DAY"), Value = 1 });
            UpdateItemListByDays.Add(new UpdateItemListStruct() { Name = LanguageController.Translation("EVERY_3_DAYS"), Value = 3 });
            UpdateItemListByDays.Add(new UpdateItemListStruct() { Name = LanguageController.Translation("EVERY_7_DAYS"), Value = 7 });
            UpdateItemListByDays.Add(new UpdateItemListStruct() { Name = LanguageController.Translation("EVERY_14_DAYS"), Value = 14 });
            UpdateItemListByDays.Add(new UpdateItemListStruct() { Name = LanguageController.Translation("EVERY_28_DAYS"), Value = 28 });
            UpdateItemListByDaysSelection = UpdateItemListByDays.FirstOrDefault(x => x.Value == Settings.Default.UpdateItemListByDays);

            CurrentItemListSourceUrl = Settings.Default.CurrentItemListSourceUrl;
        }

        public void SaveSettings()
        {
            Settings.Default.CurrentItemListSourceUrl = CurrentItemListSourceUrl;
            Settings.Default.RefreshRate = RefreshRatesSelection.Seconds;
            Settings.Default.UpdateItemListByDays = UpdateItemListByDaysSelection.Value;
            LanguageController.SetLanguage(LanguagesSelection.FileName);
            Settings.Default.DefaultLanguageCultureIetfLanguageTag = LanguagesSelection.FileName;

            _settingsWindow.Close();
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

        public string CurrentItemListSourceUrl
        {
            get => _filteredItems;
            set
            {
                _filteredItems = value;
                OnPropertyChanged();
            }
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

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}