using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Models;
using StatisticsAnalysisTool.Properties;

namespace StatisticsAnalysisTool.ViewModels
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private readonly MainWindow _mainWindow;

        private GameInfoSearchResponse _gameInfoSearchResponse;
        private SearchPlayerResponse _searchPlayer;
        private GameInfoPlayersResponse _gameInfoPlayer;
        
        public MainWindowViewModel(MainWindow mainWindow)
        {
            _mainWindow = mainWindow;
            InitLanguage();
        }

        private void InitLanguage()
        {
            LanguageController.InitializeLanguageFiles();

            if (LanguageController.SetLanguage(Settings.Default.CurrentLanguageCulture))
                return;

            if (LanguageController.SetLanguage(LanguageController.FileInfos.FirstOrDefault()?.FileName))
                return;

            MessageBox.Show("ERROR: No language file found!");
            _mainWindow.Close();
        }

        public GameInfoSearchResponse GameInfoSearch {
            get => _gameInfoSearchResponse;
            set {
                _gameInfoSearchResponse = value;
                OnPropertyChanged();
            }
        }

        public SearchPlayerResponse SearchPlayer {
            get => _searchPlayer;
            set {
                _searchPlayer = value;
                OnPropertyChanged();
            }
        }

        public GameInfoPlayersResponse GameInfoPlayers
        {
            get => _gameInfoPlayer;
            set {
                _gameInfoPlayer = value;
                OnPropertyChanged();
            }
        }

        public PlayerModeTranslation PlayerModeTranslation => new PlayerModeTranslation();

        public string DonateUrl => Settings.Default.DonateUrl;

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}