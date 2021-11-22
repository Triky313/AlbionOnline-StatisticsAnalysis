using StatisticsAnalysisTool.Annotations;
using StatisticsAnalysisTool.Common;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace StatisticsAnalysisTool.Models
{
    public class DungeonStats : INotifyPropertyChanged
    {
        private int _enteredDungeon;
        private int _openedLegendaryChests;
        private int _openedRareChests;
        private int _openedStandardChests;
        private int _openedUncommonChests;
        private double _fame;
        private double _reSpec;
        private double _silver;
        private string _translationTitle;
        private double _fameAverage;
        private double _reSpecAverage;
        private double _silverAverage;

        public int EnteredDungeon
        {
            get => _enteredDungeon;
            set
            {
                _enteredDungeon = value;
                OnPropertyChanged();
            }
        }

        public int OpenedStandardChests
        {
            get => _openedStandardChests;
            set
            {
                _openedStandardChests = value;
                OnPropertyChanged();
            }
        }

        public int OpenedUncommonChests
        {
            get => _openedUncommonChests;
            set
            {
                _openedUncommonChests = value;
                OnPropertyChanged();
            }
        }

        public int OpenedRareChests
        {
            get => _openedRareChests;
            set
            {
                _openedRareChests = value;
                OnPropertyChanged();
            }
        }

        public int OpenedLegendaryChests
        {
            get => _openedLegendaryChests;
            set
            {
                _openedLegendaryChests = value;
                OnPropertyChanged();
            }
        }

        public double Fame {
            get => _fame;
            set
            {
                _fame = value;

                FameAverage = (value / EnteredDungeon).ToShortNumber(99999999.99);
                OnPropertyChanged();
            }
        }

        public double ReSpec {
            get => _reSpec;
            set
            {
                _reSpec = value;
                ReSpecAverage = (value / EnteredDungeon).ToShortNumber(99999999.99);
                OnPropertyChanged();
            }
        }

        public double Silver {
            get => _silver;
            set
            {
                _silver = value;
                SilverAverage = (value / EnteredDungeon).ToShortNumber(99999999.99);
                OnPropertyChanged();
            }
        }

        public double FameAverage {
            get => _fameAverage;
            set
            {
                _fameAverage = value;
                OnPropertyChanged();
            }
        }

        public double ReSpecAverage {
            get => _reSpecAverage;
            set
            {
                _reSpecAverage = value;
                OnPropertyChanged();
            }
        }

        public double SilverAverage {
            get => _silverAverage;
            set
            {
                _silverAverage = value;
                OnPropertyChanged();
            }
        }

        public string TranslationTitle {
            get => _translationTitle;
            set
            {
                _translationTitle = value;
                OnPropertyChanged();
            }
        }

        public string TranslationEnteredDungeon => LanguageController.Translation("ENTERED_DUNGEON");
        public string TranslationOpenedStandardChests => LanguageController.Translation("OPENED_STANDARD_CHESTS");
        public string TranslationOpenedUncommonChests => LanguageController.Translation("OPENED_UNCOMMON_CHESTS");
        public string TranslationOpenedRareChests => LanguageController.Translation("OPENED_RARE_CHESTS");
        public string TranslationOpenedLegendaryChests => LanguageController.Translation("OPENED_LEGENDARY_CHESTS");
        public string TranslationFame => LanguageController.Translation("FAME");
        public string TranslationReSpec => LanguageController.Translation("RESPEC");
        public string TranslationSilver => LanguageController.Translation("SILVER");
        public string TranslationAverageFame => LanguageController.Translation("AVERAGE_FAME");
        public string TranslationAverageReSpec => LanguageController.Translation("AVERAGE_RESPEC");
        public string TranslationAverageSilver => LanguageController.Translation("AVERAGE_SILVER");

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}