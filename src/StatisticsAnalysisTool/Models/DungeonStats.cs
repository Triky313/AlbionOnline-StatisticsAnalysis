using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Properties;
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
        private int _openedLegendaryBookChests;
        private int _openedRareBookChests;
        private int _openedStandardBookChests;
        private int _openedUncommonBookChests;
        private double _fame;
        private double _reSpec;
        private double _silver;
        private double _might;
        private double _favor;
        private double _fameAverage;
        private double _reSpecAverage;
        private double _silverAverage;
        private double _mightAverage;
        private double _favorAverage;
        private double _famePerHour;
        private double _reSpecPerHour;
        private double _silverPerHour;
        private double _mightPerHour;
        private double _favorPerHour;
        private int _dungeonRunTimeTotal;
        private DungeonLoot _bestLootedItem;

        public int EnteredDungeon
        {
            get => _enteredDungeon;
            set
            {
                _enteredDungeon = value;
                OnPropertyChanged();
            }
        }

        public int DungeonRunTimeTotal
        {
            get => _dungeonRunTimeTotal;
            set
            {
                _dungeonRunTimeTotal = value;

                FamePerHour = Utilities.GetValuePerHourToDouble(Fame, _dungeonRunTimeTotal);
                ReSpecPerHour = Utilities.GetValuePerHourToDouble(ReSpec, _dungeonRunTimeTotal);
                SilverPerHour = Utilities.GetValuePerHourToDouble(Silver, _dungeonRunTimeTotal);
                MightPerHour = Utilities.GetValuePerHourToDouble(Might, _dungeonRunTimeTotal);
                FavorPerHour = Utilities.GetValuePerHourToDouble(Favor, _dungeonRunTimeTotal);
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

        public int OpenedStandardBookChests
        {
            get => _openedStandardBookChests;
            set
            {
                _openedStandardBookChests = value;
                OnPropertyChanged();
            }
        }

        public int OpenedUncommonBookChests
        {
            get => _openedUncommonBookChests;
            set
            {
                _openedUncommonBookChests = value;
                OnPropertyChanged();
            }
        }

        public int OpenedRareBookChests
        {
            get => _openedRareBookChests;
            set
            {
                _openedRareBookChests = value;
                OnPropertyChanged();
            }
        }

        public int OpenedLegendaryBookChests
        {
            get => _openedLegendaryBookChests;
            set
            {
                _openedLegendaryBookChests = value;
                OnPropertyChanged();
            }
        }

        public double Fame
        {
            get => _fame;
            set
            {
                _fame = value;
                FameAverage = (_fame / EnteredDungeon).ToShortNumber(99999999.99);
                OnPropertyChanged();
            }
        }

        public double ReSpec
        {
            get => _reSpec;
            set
            {
                _reSpec = value;
                ReSpecAverage = (_reSpec / EnteredDungeon).ToShortNumber(99999999.99);
                OnPropertyChanged();
            }
        }

        public double Silver
        {
            get => _silver;
            set
            {
                _silver = value;
                SilverAverage = (_silver / EnteredDungeon).ToShortNumber(99999999.99);
                OnPropertyChanged();
            }
        }

        public double Might
        {
            get => _might;
            set
            {
                _might = value;
                MightAverage = (_might / EnteredDungeon).ToShortNumber(99999999.99);
                OnPropertyChanged();
            }
        }

        public double Favor
        {
            get => _favor;
            set
            {
                _favor = value;
                FavorAverage = (_favor / EnteredDungeon).ToShortNumber(99999999.99);
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

        public double ReSpecPerHour
        {
            get => _reSpecPerHour;
            set
            {
                _reSpecPerHour = value;
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

        public double FameAverage
        {
            get => _fameAverage;
            set
            {
                _fameAverage = value;
                OnPropertyChanged();
            }
        }

        public double ReSpecAverage
        {
            get => _reSpecAverage;
            set
            {
                _reSpecAverage = value;
                OnPropertyChanged();
            }
        }

        public double SilverAverage
        {
            get => _silverAverage;
            set
            {
                _silverAverage = value;
                OnPropertyChanged();
            }
        }

        public double MightAverage
        {
            get => _mightAverage;
            set
            {
                _mightAverage = value;
                OnPropertyChanged();
            }
        }

        public double FavorAverage
        {
            get => _favorAverage;
            set
            {
                _favorAverage = value;
                OnPropertyChanged();
            }
        }

        public DungeonLoot BestLootedItem
        {
            get => _bestLootedItem;
            set
            {
                _bestLootedItem = value;
                OnPropertyChanged();
            }
        }

        public static string TranslationFame => LanguageController.Translation("FAME");
        public static string TranslationReSpec => LanguageController.Translation("RESPEC");
        public static string TranslationSilver => LanguageController.Translation("SILVER");
        public static string TranslationMight => LanguageController.Translation("MIGHT");
        public static string TranslationFavor => LanguageController.Translation("FAVOR");

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}