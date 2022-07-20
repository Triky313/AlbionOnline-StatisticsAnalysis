using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace StatisticsAnalysisTool.Models
{
    public class DashboardBindings : INotifyPropertyChanged
    {
        private double _famePerHour;
        private double _reSpecPointsPerHour;
        private double _silverPerHour;
        private double _mightPerHour;
        private double _favorPerHour;
        private double _totalGainedFameInSessionInSession;
        private double _totalGainedReSpecPointsInSessionInSession;
        private double _totalGainedSilverInSessionInSession;
        private double _totalGainedMightInSession;
        private double _totalGainedFavorInSession;
        private double _highestValue;
        private double _fameInPercent;
        private double _silverInPercent;
        private double _reSpecPointsInPercent;
        private double _mightInPercent;
        private double _favorInPercent;
        private int _killsToday;
        private int _deathsToday;
        private int _killsThisWeek;
        private int _deathsThisWeek;
        private DateTime? _lastUpdate;
        private double _averageItemPowerWhenKilling;
        private double _averageItemPowerOfTheKilledEnemies;
        private double _averageItemPowerWhenDying;
        private int _killsThisMonth;
        private int _deathsThisMonth;
        private int _soloKillsToday;
        private int _soloKillsThisWeek;
        private int _soloKillsThisMonth;
        private LootedChests _lootedChests = new();

        #region Fame / Respec / Silver / Might / Faction

        public double GetHighestValue()
        {
            var values = new List<double>()
            {
                TotalGainedFameInSession,
                TotalGainedSilverInSession,
                TotalGainedReSpecPointsInSession,
                TotalGainedMightInSession,
                TotalGainedFavorInSession
            };

            return values.Max<double>();
        }

        public void Reset()
        {
            HighestValue = 0;

            FamePerHour = 0;
            SilverPerHour = 0;
            ReSpecPointsPerHour = 0;
            MightPerHour = 0;
            FavorPerHour = 0;

            TotalGainedFameInSession = 0;
            TotalGainedSilverInSession = 0;
            TotalGainedReSpecPointsInSession = 0;
            TotalGainedMightInSession = 0;
            TotalGainedFavorInSession = 0;
        }

        public double HighestValue
        {
            get => _highestValue;
            set
            {
                _highestValue = value;
                OnPropertyChanged();
            }
        }

        public double FamePerHour
        {
            get => _famePerHour;
            set
            {
                _famePerHour = value;
                OnPropertyChanged();
            }
        }

        public double SilverPerHour
        {
            get => _silverPerHour;
            set
            {
                _silverPerHour = value;
                OnPropertyChanged();
            }
        }

        public double ReSpecPointsPerHour
        {
            get => _reSpecPointsPerHour;
            set
            {
                _reSpecPointsPerHour = value;
                OnPropertyChanged();
            }
        }

        public double MightPerHour
        {
            get => _mightPerHour;
            set
            {
                _mightPerHour = value;
                OnPropertyChanged();
            }
        }

        public double FavorPerHour
        {
            get => _favorPerHour;
            set
            {
                _favorPerHour = value;
                OnPropertyChanged();
            }
        }

        #region Percent values

        public double FameInPercent
        {
            get => _fameInPercent;
            set
            {
                _fameInPercent = value;
                OnPropertyChanged();
            }
        }

        public double SilverInPercent
        {
            get => _silverInPercent;
            set
            {
                _silverInPercent = value;
                OnPropertyChanged();
            }
        }

        public double ReSpecPointsInPercent
        {
            get => _reSpecPointsInPercent;
            set
            {
                _reSpecPointsInPercent = value;
                OnPropertyChanged();
            }
        }

        public double MightInPercent
        {
            get => _mightInPercent;
            set
            {
                _mightInPercent = value;
                OnPropertyChanged();
            }
        }

        public double FavorInPercent
        {
            get => _favorInPercent;
            set
            {
                _favorInPercent = value;
                OnPropertyChanged();
            }
        }

        #endregion

        public double TotalGainedFameInSession
        {
            get => _totalGainedFameInSessionInSession;
            set
            {
                _totalGainedFameInSessionInSession = value;
                HighestValue = GetHighestValue();
                FameInPercent = _totalGainedFameInSessionInSession / HighestValue * 100;
                OnPropertyChanged();
            }
        }

        public double TotalGainedSilverInSession
        {
            get => _totalGainedSilverInSessionInSession;
            set
            {
                _totalGainedSilverInSessionInSession = value;
                HighestValue = GetHighestValue();
                SilverInPercent = _totalGainedSilverInSessionInSession / HighestValue * 100;
                OnPropertyChanged();
            }
        }

        public double TotalGainedReSpecPointsInSession
        {
            get => _totalGainedReSpecPointsInSessionInSession;
            set
            {
                _totalGainedReSpecPointsInSessionInSession = value;
                HighestValue = GetHighestValue();
                ReSpecPointsInPercent = _totalGainedReSpecPointsInSessionInSession / HighestValue * 100;
                OnPropertyChanged();
            }
        }

        public double TotalGainedMightInSession
        {
            get => _totalGainedMightInSession;
            set
            {
                _totalGainedMightInSession = value;
                HighestValue = GetHighestValue();
                MightInPercent = _totalGainedMightInSession / HighestValue * 100;
                OnPropertyChanged();
            }
        }

        public double TotalGainedFavorInSession
        {
            get => _totalGainedFavorInSession;
            set
            {
                _totalGainedFavorInSession = value;
                HighestValue = GetHighestValue();
                FavorInPercent = _totalGainedFavorInSession / HighestValue * 100;
                OnPropertyChanged();
            }
        }

        #endregion

        #region Kill / Death stats

        public int SoloKillsToday
        {
            get => _soloKillsToday;
            set
            {
                _soloKillsToday = value;
                OnPropertyChanged();
            }
        }

        public int SoloKillsThisWeek
        {
            get => _soloKillsThisWeek;
            set
            {
                _soloKillsThisWeek = value;
                OnPropertyChanged();
            }
        }

        public int SoloKillsThisMonth
        {
            get => _soloKillsThisMonth;
            set
            {
                _soloKillsThisMonth = value;
                OnPropertyChanged();
            }
        }

        public int KillsToday
        {
            get => _killsToday;
            set
            {
                _killsToday = value;
                OnPropertyChanged();
            }
        }

        public int KillsThisWeek
        {
            get => _killsThisWeek;
            set
            {
                _killsThisWeek = value;
                OnPropertyChanged();
            }
        }

        public int KillsThisMonth
        {
            get => _killsThisMonth;
            set
            {
                _killsThisMonth = value;
                OnPropertyChanged();
            }
        }

        public int DeathsToday
        {
            get => _deathsToday;
            set
            {
                _deathsToday = value;
                OnPropertyChanged();
            }
        }

        public int DeathsThisWeek
        {
            get => _deathsThisWeek;
            set
            {
                _deathsThisWeek = value;
                OnPropertyChanged();
            }
        }

        public int DeathsThisMonth
        {
            get => _deathsThisMonth;
            set
            {
                _deathsThisMonth = value;
                OnPropertyChanged();
            }
        }

        public double AverageItemPowerWhenKilling
        {
            get => _averageItemPowerWhenKilling;
            set
            {
                _averageItemPowerWhenKilling = value;
                OnPropertyChanged();
            }
        }

        public double AverageItemPowerOfTheKilledEnemies
        {
            get => _averageItemPowerOfTheKilledEnemies;
            set
            {
                _averageItemPowerOfTheKilledEnemies = value;
                OnPropertyChanged();
            }
        }

        public double AverageItemPowerWhenDying
        {
            get => _averageItemPowerWhenDying;
            set
            {
                _averageItemPowerWhenDying = value;
                OnPropertyChanged();
            }
        }

        public DateTime? LastUpdate
        {
            get => _lastUpdate;
            set
            {
                _lastUpdate = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region Chest stats

        public LootedChests LootedChests
        {
            get => _lootedChests;
            set
            {
                _lootedChests = value;
                OnPropertyChanged();
            }
        }

        #endregion

        public static string TranslationTitle => $"{LanguageController.Translation("DASHBOARD")}";
        public static string TranslationFame => LanguageController.Translation("FAME");
        public static string TranslationSilver => LanguageController.Translation("SILVER");
        public static string TranslationReSpec => LanguageController.Translation("RESPEC");
        public static string TranslationFaction => LanguageController.Translation("FACTION");
        public static string TranslationMight => LanguageController.Translation("MIGHT");
        public static string TranslationFavor => LanguageController.Translation("FAVOR");
        public static string TranslationResetTrackingCounter => LanguageController.Translation("RESET_TRACKING_COUNTER");
        public static string TranslationToday => LanguageController.Translation("TODAY").ToLower();
        public static string TranslationWeek => LanguageController.Translation("WEEK").ToLower();
        public static string TranslationMonth => LanguageController.Translation("MONTH").ToLower();
        public static string TranslationKills => LanguageController.Translation("KILLS");
        public static string TranslationSoloKills => LanguageController.Translation("SOLO_KILLS");
        public static string TranslationDeaths => LanguageController.Translation("DEATHS");
        public static string TranslationLastUpdate => LanguageController.Translation("LAST_UPDATE");
        public static string TranslationDataFromAlbionOnlineServers => LanguageController.Translation("DATA_FROM_ALBION_ONLINE_SERVERS");
        public static string TranslationAverageItemPowerWhenKilling => LanguageController.Translation("AVERAGE_ITEM_POWER_WHEN_KILLING");
        public static string TranslationAverageItemPowerOfTheKilledEnemies => LanguageController.Translation("AVERAGE_ITEM_POWER_OF_THE_KILLED_ENEMIES");
        public static string TranslationAverageItemPowerWhenDying => LanguageController.Translation("AVERAGE_ITEM_POWER_WHEN_DYING");

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}