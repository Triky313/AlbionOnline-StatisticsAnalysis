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
        private static LanguageController.FileInfo _languagesSelectedItem;

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
            _settingsWindow.CbRefreshRate.Items.Add(new SettingsWindow.RefreshRateStruct() { Name = LanguageController.Translation("5_SECONDS"), Seconds = 5000 });
            _settingsWindow.CbRefreshRate.Items.Add(new SettingsWindow.RefreshRateStruct() { Name = LanguageController.Translation("10_SECONDS"), Seconds = 10000 });
            _settingsWindow.CbRefreshRate.Items.Add(new SettingsWindow.RefreshRateStruct() { Name = LanguageController.Translation("30_SECONDS"), Seconds = 30000 });
            _settingsWindow.CbRefreshRate.Items.Add(new SettingsWindow.RefreshRateStruct() { Name = LanguageController.Translation("60_SECONDS"), Seconds = 60000 });
            _settingsWindow.CbRefreshRate.Items.Add(new SettingsWindow.RefreshRateStruct() { Name = LanguageController.Translation("5_MINUTES"), Seconds = 300000 });
            _settingsWindow.CbRefreshRate.SelectedValue = Settings.Default.RefreshRate;

            Languages.Clear();
            foreach (var langInfos in LanguageController.FileInfos)
                Languages.Add(new LanguageController.FileInfo() { FileName = langInfos.FileName });

            LanguagesSelectedItem = Languages.FirstOrDefault(x => x.FileName == LanguageController.CurrentLanguage);

            // Update item list by days
            _settingsWindow.CbUpdateItemListByDays.Items.Add(new SettingsWindow.UpdateItemListStruct() { Name = LanguageController.Translation("EVERY_DAY"), Value = 1 });
            _settingsWindow.CbUpdateItemListByDays.Items.Add(new SettingsWindow.UpdateItemListStruct() { Name = LanguageController.Translation("EVERY_3_DAYS"), Value = 3 });
            _settingsWindow.CbUpdateItemListByDays.Items.Add(new SettingsWindow.UpdateItemListStruct() { Name = LanguageController.Translation("EVERY_7_DAYS"), Value = 7 });
            _settingsWindow.CbUpdateItemListByDays.Items.Add(new SettingsWindow.UpdateItemListStruct() { Name = LanguageController.Translation("EVERY_14_DAYS"), Value = 14 });
            _settingsWindow.CbUpdateItemListByDays.Items.Add(new SettingsWindow.UpdateItemListStruct() { Name = LanguageController.Translation("EVERY_28_DAYS"), Value = 28 });
            _settingsWindow.CbUpdateItemListByDays.SelectedValue = Settings.Default.UpdateItemListByDays;

            CurrentItemListSourceUrl = Settings.Default.CurrentItemListSourceUrl;
        }

        public void SaveSettings()
        {
            Settings.Default.CurrentItemListSourceUrl = CurrentItemListSourceUrl;

            var refreshRateItem = (SettingsWindow.RefreshRateStruct)_settingsWindow.CbRefreshRate.SelectedItem;
            var updateItemListByDays = (SettingsWindow.UpdateItemListStruct)_settingsWindow.CbUpdateItemListByDays.SelectedItem;

            Settings.Default.RefreshRate = refreshRateItem.Seconds;

            Settings.Default.UpdateItemListByDays = updateItemListByDays.Value;

            LanguageController.SetLanguage(LanguagesSelectedItem.FileName);
            Settings.Default.CurrentLanguageCulture = LanguagesSelectedItem.FileName;

            _settingsWindow.Close();
        }

        public LanguageController.FileInfo LanguagesSelectedItem
        {
            get => _languagesSelectedItem;
            set
            {
                _languagesSelectedItem = value;
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

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}