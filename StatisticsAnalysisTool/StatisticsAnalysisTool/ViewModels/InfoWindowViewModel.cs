using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Properties;
using System;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Net;
using System.Runtime.CompilerServices;

namespace StatisticsAnalysisTool.ViewModels
{
    class InfoWindowViewModel : INotifyPropertyChanged
    {
        private string _title;
        private string _showNotAgain;
        private bool _showNotAgainChecked;
        private int _progress;
        private int _maximum;
        private int _minimum;
        private string _lastUpdate;
        private string _featureDescription;

        public InfoWindowViewModel()
        {
            Init();
        }

        private void Init()
        {
            ShowNotAgain = LanguageController.Translation("SHOW_NOT_AGAIN");
            Title = LanguageController.Translation("DONATION_GOAL_FOR_A_NEW_FEATURE");
            FeatureDescription = LanguageController.Translation("FEATURE_DESCRIPTION");

            GetDonationProgressData();
        }

        private void GetDonationProgressData()
        {
            try
            {
                var webRequest = WebRequest.Create("https://raw.githubusercontent.com/Triky313/AlbionOnline-StatisticsAnalysis/master/StatisticsAnalysisTool/StatisticsAnalysisTool/donation-progress.txt");
                using (var response = webRequest.GetResponse())
                using (var content = response.GetResponseStream())
                using (var reader = new StreamReader(content ?? throw new InvalidOperationException()))
                {
                    var strContent = reader.ReadToEnd();
                    var dataArray = strContent.Split(',');

                    if (int.TryParse(dataArray[0], out var minimum))
                    {
                        Minimum = minimum;
                    }

                    if (int.TryParse(dataArray[1], out var maximum))
                    {
                        Maximum = maximum;
                    }

                    if (int.TryParse(dataArray[2], out var progress))
                    {
                        Progress = progress;
                    }

                    if (DateTime.TryParse(dataArray[3], out var timestamp))
                    {
                        LastUpdate = $"{LanguageController.Translation("LAST_UPDATE")}: {timestamp.ToString(CultureInfo.CurrentCulture)}";
                    }
                }
            }
            catch (Exception)
            {
                Minimum = 0;
                Maximum = 200;
                Progress = 0;
                LastUpdate = "";
            }
        }

        public void SaveShowNotAgainSetting()
        {
            Settings.Default.ShowInfoWindowOnStartChecked = ShowNotAgainChecked;
        }

        public string DonateUrl => Settings.Default.DonateUrl;

        public string LastUpdate {
            get => _lastUpdate;
            set {
                _lastUpdate = value;
                OnPropertyChanged();
            }
        }
        
        public int Progress {
            get => _progress;
            set {
                _progress = value;
                OnPropertyChanged();
            }
        }

        public int Maximum {
            get => _maximum;
            set {
                _maximum = value;
                OnPropertyChanged();
            }
        }

        public int Minimum {
            get => _minimum;
            set {
                _minimum = value;
                OnPropertyChanged();
            }
        }

        public string Title {
            get => _title;
            set {
                _title = value;
                OnPropertyChanged();
            }
        }

        public string FeatureDescription {
            get => _featureDescription;
            set {
                _featureDescription = value;
                OnPropertyChanged();
            }
        }

        public string ShowNotAgain {
            get => _showNotAgain;
            set {
                _showNotAgain = value;
                OnPropertyChanged();
            }
        }

        public bool ShowNotAgainChecked {
            get => _showNotAgainChecked;
            set {
                _showNotAgainChecked = value;
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
