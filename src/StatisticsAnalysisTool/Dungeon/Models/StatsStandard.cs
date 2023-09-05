using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.ViewModels;
using System.Windows;

namespace StatisticsAnalysisTool.Dungeon.Models;

public class StatsStandard : BaseViewModel
{
    private int _entered;
    private int _enteredEpic;
    private int _enteredRare;
    private int _enteredUncommon;
    private int _enteredLegendary;
    private Loot _mostValuableLoot;
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
    private int _runTimeTotal;
    private double _lootInSilver;
    private double _lootInSilverPerHour;
    private double _lootInSilverAverage;
    private Visibility _visibility = Visibility.Collapsed;
    private int _openedStandardChests;
    private int _openedUncommonChests;
    private int _openedRareChests;
    private int _openedLegendaryChests;
    private int _openedStandardBookChests;
    private int _openedUncommonBookChests;
    private int _openedRareBookChests;
    private int _enteredCommon;

    public Visibility Visibility
    {
        get => _visibility;
        set
        {
            _visibility = value;
            OnPropertyChanged();
        }
    }

    public int Entered
    {
        get => _entered;
        set
        {
            _entered = value;
            OnPropertyChanged();
        }
    }

    public int RunTimeTotal
    {
        get => _runTimeTotal;
        set
        {
            _runTimeTotal = value;
            OnPropertyChanged();
        }
    }

    public int EnteredCommon
    {
        get => _enteredCommon;
        set
        {
            _enteredCommon = value;
            OnPropertyChanged();
        }
    }

    public int EnteredUncommon
    {
        get => _enteredUncommon;
        set
        {
            _enteredUncommon = value;
            OnPropertyChanged();
        }
    }

    public int EnteredRare
    {
        get => _enteredRare;
        set
        {
            _enteredRare = value;
            OnPropertyChanged();
        }
    }

    public int EnteredEpic
    {
        get => _enteredEpic;
        set
        {
            _enteredEpic = value;
            OnPropertyChanged();
        }
    }

    public int EnteredLegendary
    {
        get => _enteredLegendary;
        set
        {
            _enteredLegendary = value;
            OnPropertyChanged();
        }
    }

    public Loot MostValuableLoot
    {
        get => _mostValuableLoot;
        set
        {
            _mostValuableLoot = value;
            OnPropertyChanged();
        }
    }

    public double Fame
    {
        get => _fame;
        set
        {
            _fame = value;
            FameAverage = (_fame / Entered).ToShortNumber(99999999.99);
            FamePerHour = value.GetValuePerHour(RunTimeTotal);
            OnPropertyChanged();
        }
    }

    public double ReSpec
    {
        get => _reSpec;
        set
        {
            _reSpec = value;
            ReSpecAverage = (_reSpec / Entered).ToShortNumber(99999999.99);
            ReSpecPerHour = value.GetValuePerHour(RunTimeTotal);
            OnPropertyChanged();
        }
    }

    public double Silver
    {
        get => _silver;
        set
        {
            _silver = value;
            SilverAverage = (_silver / Entered).ToShortNumber(99999999.99);
            SilverPerHour = value.GetValuePerHour(_runTimeTotal);
            OnPropertyChanged();
        }
    }

    public double Might
    {
        get => _might;
        set
        {
            _might = value;
            MightAverage = (_might / Entered).ToShortNumber(99999999.99);
            MightPerHour = value.GetValuePerHour(_runTimeTotal);
            OnPropertyChanged();
        }
    }

    public double Favor
    {
        get => _favor;
        set
        {
            _favor = value;
            FavorAverage = (_favor / Entered).ToShortNumber(99999999.99);
            FavorPerHour = value.GetValuePerHour(_runTimeTotal);
            OnPropertyChanged();
        }
    }

    public double LootInSilver
    {
        get => _lootInSilver;
        set
        {
            _lootInSilver = value;
            LootInSilverAverage = (_lootInSilver / Entered).ToShortNumber(99999999.99);
            LootInSilverPerHour = value.GetValuePerHour(_runTimeTotal);
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

    public double LootInSilverPerHour
    {
        get => _lootInSilverPerHour;
        set
        {
            _lootInSilverPerHour = value;
            OnPropertyChanged();
        }
    }

    public double LootInSilverAverage
    {
        get => _lootInSilverAverage;
        set
        {
            _lootInSilverAverage = value;
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

    public static string TranslationAverageAbbreviation => LanguageController.Translation("AVERAGE_ABBREVIATION");
    public static string TranslationHourAbbreviation => LanguageController.Translation("HOUR_ABBREVIATION");
    public static string TranslationStandardDungeon => LanguageController.Translation("STANDARD_DUNGEON");
    public static string TranslationMostValuableLoot => LanguageController.Translation("MOST_VALUABLE_LOOT");
    public static string TranslationTotal => LanguageController.Translation("TOTAL");
    public static string TranslationPerHour => LanguageController.Translation("PER_HOUR");
    public static string TranslationAverage => LanguageController.Translation("AVERAGE");
    public static string TranslationAll => LanguageController.Translation("ALL");
    public static string TranslationEnteredDungeons => LanguageController.Translation("ENTERED_DUNGEONS");
    public static string TranslationChestStats => LanguageController.Translation("CHEST_STATS");
    public static string TranslationOpenedStandardChests => LanguageController.Translation("STANDARD_CHESTS");
    public static string TranslationOpenedUncommonChests => LanguageController.Translation("UNCOMMON_CHESTS");
    public static string TranslationOpenedRareChests => LanguageController.Translation("RARE_CHESTS");
    public static string TranslationOpenedLegendaryChests => LanguageController.Translation("LEGENDARY_CHESTS");
    public static string TranslationBookChestStats => LanguageController.Translation("BOOK_CHEST_STATS");
    public static string TranslationType => LanguageController.Translation("TYPE");
    public static string TranslationNumberOfDungeons => LanguageController.Translation("NUMBER_OF_DUNGEONS");
}