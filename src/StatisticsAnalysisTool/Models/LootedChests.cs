using StatisticsAnalysisTool.Localization;
using StatisticsAnalysisTool.ViewModels;

namespace StatisticsAnalysisTool.Models;

public class LootedChests : BaseViewModel
{
    private int _outlandsCommonWeek;
    private int _outlandsCommonMonth;
    private int _outlandsUncommonWeek;
    private int _outlandsUncommonMonth;
    private int _outlandsEpicWeek;
    private int _outlandsEpicMonth;
    private int _outlandsLegendaryWeek;
    private int _outlandsLegendaryMonth;
    private int _randomGroupDungeonCommonWeek;
    private int _randomGroupDungeonCommonMonth;
    private int _randomGroupDungeonUncommonWeek;
    private int _randomGroupDungeonUncommonMonth;
    private int _randomGroupDungeonEpicWeek;
    private int _randomGroupDungeonEpicMonth;
    private int _randomGroupDungeonLegendaryWeek;
    private int _randomGroupDungeonLegendaryMonth;
    private int _avalonianRoadCommonWeek;
    private int _avalonianRoadCommonMonth;
    private int _avalonianRoadUncommonWeek;
    private int _avalonianRoadUncommonMonth;
    private int _avalonianRoadEpicWeek;
    private int _avalonianRoadEpicMonth;
    private int _avalonianRoadLegendaryWeek;
    private int _avalonianRoadLegendaryMonth;
    private int _hellGatesCommonWeek;
    private int _hellGatesCommonMonth;
    private int _hellGatesUncommonWeek;
    private int _hellGatesUncommonMonth;
    private int _hellGatesEpicWeek;
    private int _hellGatesEpicMonth;
    private int _hellGatesLegendaryWeek;
    private int _hellGatesLegendaryMonth;
    private int _corruptedCommonWeek;
    private int _corruptedCommonMonth;
    private int _corruptedUncommonWeek;
    private int _corruptedUncommonMonth;
    private int _corruptedEpicWeek;
    private int _corruptedEpicMonth;
    private int _corruptedLegendaryWeek;
    private int _corruptedLegendaryMonth;
    private int _corruptedCommonYear;
    private int _corruptedUncommonYear;
    private int _corruptedEpicYear;
    private int _corruptedLegendaryYear;
    private int _mistCommonWeek;
    private int _mistCommonMonth;
    private int _mistUncommonWeek;
    private int _mistUncommonMonth;
    private int _mistEpicWeek;
    private int _mistEpicMonth;
    private int _mistLegendaryWeek;
    private int _mistLegendaryMonth;
    private int _outlandsCommonYear;
    private int _outlandsUncommonYear;
    private int _outlandsEpicYear;
    private int _outlandsLegendaryYear;
    private int _randomGroupDungeonCommonYear;
    private int _randomGroupDungeonUncommonYear;
    private int _randomGroupDungeonEpicYear;
    private int _randomGroupDungeonLegendaryYear;
    private int _avalonianRoadCommonYear;
    private int _avalonianRoadUncommonYear;
    private int _avalonianRoadEpicYear;
    private int _avalonianRoadLegendaryYear;
    private int _hellGatesCommonYear;
    private int _hellGatesUncommonYear;
    private int _hellGatesEpicYear;
    private int _hellGatesLegendaryYear;
    private int _mistCommonYear;
    private int _mistUncommonYear;
    private int _mistEpicYear;
    private int _mistLegendaryYear;
    private int _randomSoloDungeonCommonWeek;
    private int _randomSoloDungeonCommonMonth;
    private int _randomSoloDungeonCommonYear;
    private int _randomSoloDungeonUncommonWeek;
    private int _randomSoloDungeonUncommonMonth;
    private int _randomSoloDungeonUncommonYear;
    private int _randomSoloDungeonEpicWeek;
    private int _randomSoloDungeonEpicMonth;
    private int _randomSoloDungeonEpicYear;
    private int _randomSoloDungeonLegendaryWeek;
    private int _randomSoloDungeonLegendaryMonth;
    private int _randomSoloDungeonLegendaryYear;

    #region OpenWorld bindings

    public int OpenWorldCommonWeek
    {
        get => _outlandsCommonWeek;
        set
        {
            _outlandsCommonWeek = value;
            OnPropertyChanged();
        }
    }

    public int OpenWorldCommonMonth
    {
        get => _outlandsCommonMonth;
        set
        {
            _outlandsCommonMonth = value;
            OnPropertyChanged();
        }
    }

    public int OpenWorldCommonYear
    {
        get => _outlandsCommonYear;
        set
        {
            _outlandsCommonYear = value;
            OnPropertyChanged();
        }
    }

    public int OpenWorldUncommonWeek
    {
        get => _outlandsUncommonWeek;
        set
        {
            _outlandsUncommonWeek = value;
            OnPropertyChanged();
        }
    }

    public int OpenWorldUncommonMonth
    {
        get => _outlandsUncommonMonth;
        set
        {
            _outlandsUncommonMonth = value;
            OnPropertyChanged();
        }
    }

    public int OpenWorldUncommonYear
    {
        get => _outlandsUncommonYear;
        set
        {
            _outlandsUncommonYear = value;
            OnPropertyChanged();
        }
    }

    public int OpenWorldEpicWeek
    {
        get => _outlandsEpicWeek;
        set
        {
            _outlandsEpicWeek = value;
            OnPropertyChanged();
        }
    }

    public int OpenWorldEpicMonth
    {
        get => _outlandsEpicMonth;
        set
        {
            _outlandsEpicMonth = value;
            OnPropertyChanged();
        }
    }

    public int OpenWorldEpicYear
    {
        get => _outlandsEpicYear;
        set
        {
            _outlandsEpicYear = value;
            OnPropertyChanged();
        }
    }

    public int OpenWorldLegendaryWeek
    {
        get => _outlandsLegendaryWeek;
        set
        {
            _outlandsLegendaryWeek = value;
            OnPropertyChanged();
        }
    }

    public int OpenWorldLegendaryMonth
    {
        get => _outlandsLegendaryMonth;
        set
        {
            _outlandsLegendaryMonth = value;
            OnPropertyChanged();
        }
    }

    public int OpenWorldLegendaryYear
    {
        get => _outlandsLegendaryYear;
        set
        {
            _outlandsLegendaryYear = value;
            OnPropertyChanged();
        }
    }

    #endregion

    #region Random group dungeon bindings

    public int RandomGroupDungeonCommonWeek
    {
        get => _randomGroupDungeonCommonWeek;
        set
        {
            _randomGroupDungeonCommonWeek = value;
            OnPropertyChanged();
        }
    }

    public int RandomGroupDungeonCommonMonth
    {
        get => _randomGroupDungeonCommonMonth;
        set
        {
            _randomGroupDungeonCommonMonth = value;
            OnPropertyChanged();
        }
    }

    public int RandomGroupDungeonCommonYear
    {
        get => _randomGroupDungeonCommonYear;
        set
        {
            _randomGroupDungeonCommonYear = value;
            OnPropertyChanged();
        }
    }

    public int RandomGroupDungeonUncommonWeek
    {
        get => _randomGroupDungeonUncommonWeek;
        set
        {
            _randomGroupDungeonUncommonWeek = value;
            OnPropertyChanged();
        }
    }

    public int RandomGroupDungeonUncommonMonth
    {
        get => _randomGroupDungeonUncommonMonth;
        set
        {
            _randomGroupDungeonUncommonMonth = value;
            OnPropertyChanged();
        }
    }

    public int RandomGroupDungeonUncommonYear
    {
        get => _randomGroupDungeonUncommonYear;
        set
        {
            _randomGroupDungeonUncommonYear = value;
            OnPropertyChanged();
        }
    }

    public int RandomGroupDungeonEpicWeek
    {
        get => _randomGroupDungeonEpicWeek;
        set
        {
            _randomGroupDungeonEpicWeek = value;
            OnPropertyChanged();
        }
    }

    public int RandomGroupDungeonEpicMonth
    {
        get => _randomGroupDungeonEpicMonth;
        set
        {
            _randomGroupDungeonEpicMonth = value;
            OnPropertyChanged();
        }
    }

    public int RandomGroupDungeonEpicYear
    {
        get => _randomGroupDungeonEpicYear;
        set
        {
            _randomGroupDungeonEpicYear = value;
            OnPropertyChanged();
        }
    }

    public int RandomGroupDungeonLegendaryWeek
    {
        get => _randomGroupDungeonLegendaryWeek;
        set
        {
            _randomGroupDungeonLegendaryWeek = value;
            OnPropertyChanged();
        }
    }

    public int RandomGroupDungeonLegendaryMonth
    {
        get => _randomGroupDungeonLegendaryMonth;
        set
        {
            _randomGroupDungeonLegendaryMonth = value;
            OnPropertyChanged();
        }
    }

    public int RandomGroupDungeonLegendaryYear
    {
        get => _randomGroupDungeonLegendaryYear;
        set
        {
            _randomGroupDungeonLegendaryYear = value;
            OnPropertyChanged();
        }
    }

    #endregion

    #region Random solo dungeon bindings

    public int RandomSoloDungeonCommonWeek
    {
        get => _randomSoloDungeonCommonWeek;
        set
        {
            _randomSoloDungeonCommonWeek = value;
            OnPropertyChanged();
        }
    }

    public int RandomSoloDungeonCommonMonth
    {
        get => _randomSoloDungeonCommonMonth;
        set
        {
            _randomSoloDungeonCommonMonth = value;
            OnPropertyChanged();
        }
    }

    public int RandomSoloDungeonCommonYear
    {
        get => _randomSoloDungeonCommonYear;
        set
        {
            _randomSoloDungeonCommonYear = value;
            OnPropertyChanged();
        }
    }

    public int RandomSoloDungeonUncommonWeek
    {
        get => _randomSoloDungeonUncommonWeek;
        set
        {
            _randomSoloDungeonUncommonWeek = value;
            OnPropertyChanged();
        }
    }

    public int RandomSoloDungeonUncommonMonth
    {
        get => _randomSoloDungeonUncommonMonth;
        set
        {
            _randomSoloDungeonUncommonMonth = value;
            OnPropertyChanged();
        }
    }

    public int RandomSoloDungeonUncommonYear
    {
        get => _randomSoloDungeonUncommonYear;
        set
        {
            _randomSoloDungeonUncommonYear = value;
            OnPropertyChanged();
        }
    }

    public int RandomSoloDungeonEpicWeek
    {
        get => _randomSoloDungeonEpicWeek;
        set
        {
            _randomSoloDungeonEpicWeek = value;
            OnPropertyChanged();
        }
    }

    public int RandomSoloDungeonEpicMonth
    {
        get => _randomSoloDungeonEpicMonth;
        set
        {
            _randomSoloDungeonEpicMonth = value;
            OnPropertyChanged();
        }
    }

    public int RandomSoloDungeonEpicYear
    {
        get => _randomSoloDungeonEpicYear;
        set
        {
            _randomSoloDungeonEpicYear = value;
            OnPropertyChanged();
        }
    }

    public int RandomSoloDungeonLegendaryWeek
    {
        get => _randomSoloDungeonLegendaryWeek;
        set
        {
            _randomSoloDungeonLegendaryWeek = value;
            OnPropertyChanged();
        }
    }

    public int RandomSoloDungeonLegendaryMonth
    {
        get => _randomSoloDungeonLegendaryMonth;
        set
        {
            _randomSoloDungeonLegendaryMonth = value;
            OnPropertyChanged();
        }
    }

    public int RandomSoloDungeonLegendaryYear
    {
        get => _randomSoloDungeonLegendaryYear;
        set
        {
            _randomSoloDungeonLegendaryYear = value;
            OnPropertyChanged();
        }
    }

    #endregion

    #region Avalonian Road bindings

    public int AvalonianRoadCommonWeek
    {
        get => _avalonianRoadCommonWeek;
        set
        {
            _avalonianRoadCommonWeek = value;
            OnPropertyChanged();
        }
    }

    public int AvalonianRoadCommonMonth
    {
        get => _avalonianRoadCommonMonth;
        set
        {
            _avalonianRoadCommonMonth = value;
            OnPropertyChanged();
        }
    }

    public int AvalonianRoadCommonYear
    {
        get => _avalonianRoadCommonYear;
        set
        {
            _avalonianRoadCommonYear = value;
            OnPropertyChanged();
        }
    }

    public int AvalonianRoadUncommonWeek
    {
        get => _avalonianRoadUncommonWeek;
        set
        {
            _avalonianRoadUncommonWeek = value;
            OnPropertyChanged();
        }
    }

    public int AvalonianRoadUncommonMonth
    {
        get => _avalonianRoadUncommonMonth;
        set
        {
            _avalonianRoadUncommonMonth = value;
            OnPropertyChanged();
        }
    }

    public int AvalonianRoadUncommonYear
    {
        get => _avalonianRoadUncommonYear;
        set
        {
            _avalonianRoadUncommonYear = value;
            OnPropertyChanged();
        }
    }

    public int AvalonianRoadEpicWeek
    {
        get => _avalonianRoadEpicWeek;
        set
        {
            _avalonianRoadEpicWeek = value;
            OnPropertyChanged();
        }
    }

    public int AvalonianRoadEpicMonth
    {
        get => _avalonianRoadEpicMonth;
        set
        {
            _avalonianRoadEpicMonth = value;
            OnPropertyChanged();
        }
    }

    public int AvalonianRoadEpicYear
    {
        get => _avalonianRoadEpicYear;
        set
        {
            _avalonianRoadEpicYear = value;
            OnPropertyChanged();
        }
    }

    public int AvalonianRoadLegendaryWeek
    {
        get => _avalonianRoadLegendaryWeek;
        set
        {
            _avalonianRoadLegendaryWeek = value;
            OnPropertyChanged();
        }
    }

    public int AvalonianRoadLegendaryMonth
    {
        get => _avalonianRoadLegendaryMonth;
        set
        {
            _avalonianRoadLegendaryMonth = value;
            OnPropertyChanged();
        }
    }

    public int AvalonianRoadLegendaryYear
    {
        get => _avalonianRoadLegendaryYear;
        set
        {
            _avalonianRoadLegendaryYear = value;
            OnPropertyChanged();
        }
    }

    #endregion

    #region Hellgate bindings

    public int HellGateCommonWeek
    {
        get => _hellGatesCommonWeek;
        set
        {
            _hellGatesCommonWeek = value;
            OnPropertyChanged();
        }
    }

    public int HellGateCommonMonth
    {
        get => _hellGatesCommonMonth;
        set
        {
            _hellGatesCommonMonth = value;
            OnPropertyChanged();
        }
    }

    public int HellGateCommonYear
    {
        get => _hellGatesCommonYear;
        set
        {
            _hellGatesCommonYear = value;
            OnPropertyChanged();
        }
    }

    public int HellGateUncommonWeek
    {
        get => _hellGatesUncommonWeek;
        set
        {
            _hellGatesUncommonWeek = value;
            OnPropertyChanged();
        }
    }

    public int HellGateUncommonMonth
    {
        get => _hellGatesUncommonMonth;
        set
        {
            _hellGatesUncommonMonth = value;
            OnPropertyChanged();
        }
    }

    public int HellGateUncommonYear
    {
        get => _hellGatesUncommonYear;
        set
        {
            _hellGatesUncommonYear = value;
            OnPropertyChanged();
        }
    }

    public int HellGateEpicWeek
    {
        get => _hellGatesEpicWeek;
        set
        {
            _hellGatesEpicWeek = value;
            OnPropertyChanged();
        }
    }

    public int HellGateEpicMonth
    {
        get => _hellGatesEpicMonth;
        set
        {
            _hellGatesEpicMonth = value;
            OnPropertyChanged();
        }
    }

    public int HellGateEpicYear
    {
        get => _hellGatesEpicYear;
        set
        {
            _hellGatesEpicYear = value;
            OnPropertyChanged();
        }
    }

    public int HellGateLegendaryWeek
    {
        get => _hellGatesLegendaryWeek;
        set
        {
            _hellGatesLegendaryWeek = value;
            OnPropertyChanged();
        }
    }

    public int HellGateLegendaryMonth
    {
        get => _hellGatesLegendaryMonth;
        set
        {
            _hellGatesLegendaryMonth = value;
            OnPropertyChanged();
        }
    }

    public int HellGateLegendaryYear
    {
        get => _hellGatesLegendaryYear;
        set
        {
            _hellGatesLegendaryYear = value;
            OnPropertyChanged();
        }
    }

    #endregion

    #region Mist bindings

    public int MistCommonWeek
    {
        get => _mistCommonWeek;
        set
        {
            _mistCommonWeek = value;
            OnPropertyChanged();
        }
    }

    public int MistCommonMonth
    {
        get => _mistCommonMonth;
        set
        {
            _mistCommonMonth = value;
            OnPropertyChanged();
        }
    }

    public int MistCommonYear
    {
        get => _mistCommonYear;
        set
        {
            _mistCommonYear = value;
            OnPropertyChanged();
        }
    }

    public int MistUncommonWeek
    {
        get => _mistUncommonWeek;
        set
        {
            _mistUncommonWeek = value;
            OnPropertyChanged();
        }
    }

    public int MistUncommonMonth
    {
        get => _mistUncommonMonth;
        set
        {
            _mistUncommonMonth = value;
            OnPropertyChanged();
        }
    }

    public int MistUncommonYear
    {
        get => _mistUncommonYear;
        set
        {
            _mistUncommonYear = value;
            OnPropertyChanged();
        }
    }

    public int MistEpicWeek
    {
        get => _mistEpicWeek;
        set
        {
            _mistEpicWeek = value;
            OnPropertyChanged();
        }
    }

    public int MistEpicMonth
    {
        get => _mistEpicMonth;
        set
        {
            _mistEpicMonth = value;
            OnPropertyChanged();
        }
    }

    public int MistEpicYear
    {
        get => _mistEpicYear;
        set
        {
            _mistEpicYear = value;
            OnPropertyChanged();
        }
    }

    public int MistLegendaryWeek
    {
        get => _mistLegendaryWeek;
        set
        {
            _mistLegendaryWeek = value;
            OnPropertyChanged();
        }
    }

    public int MistLegendaryMonth
    {
        get => _mistLegendaryMonth;
        set
        {
            _mistLegendaryMonth = value;
            OnPropertyChanged();
        }
    }

    public int MistLegendaryYear
    {
        get => _mistLegendaryYear;
        set
        {
            _mistLegendaryYear = value;
            OnPropertyChanged();
        }
    }

    #endregion

    #region Corrupted bindings

    public int CorruptedCommonWeek
    {
        get => _corruptedCommonWeek;
        set
        {
            _corruptedCommonWeek = value;
            OnPropertyChanged();
        }
    }

    public int CorruptedCommonMonth
    {
        get => _corruptedCommonMonth;
        set
        {
            _corruptedCommonMonth = value;
            OnPropertyChanged();
        }
    }

    public int CorruptedCommonYear
    {
        get => _corruptedCommonYear;
        set
        {
            _corruptedCommonYear = value;
            OnPropertyChanged();
        }
    }

    public int CorruptedUncommonWeek
    {
        get => _corruptedUncommonWeek;
        set
        {
            _corruptedUncommonWeek = value;
            OnPropertyChanged();
        }
    }

    public int CorruptedUncommonMonth
    {
        get => _corruptedUncommonMonth;
        set
        {
            _corruptedUncommonMonth = value;
            OnPropertyChanged();
        }
    }

    public int CorruptedUncommonYear
    {
        get => _corruptedUncommonYear;
        set
        {
            _corruptedUncommonYear = value;
            OnPropertyChanged();
        }
    }

    public int CorruptedEpicWeek
    {
        get => _corruptedEpicWeek;
        set
        {
            _corruptedEpicWeek = value;
            OnPropertyChanged();
        }
    }

    public int CorruptedEpicMonth
    {
        get => _corruptedEpicMonth;
        set
        {
            _corruptedEpicMonth = value;
            OnPropertyChanged();
        }
    }

    public int CorruptedEpicYear
    {
        get => _corruptedEpicYear;
        set
        {
            _corruptedEpicYear = value;
            OnPropertyChanged();
        }
    }

    public int CorruptedLegendaryWeek
    {
        get => _corruptedLegendaryWeek;
        set
        {
            _corruptedLegendaryWeek = value;
            OnPropertyChanged();
        }
    }

    public int CorruptedLegendaryMonth
    {
        get => _corruptedLegendaryMonth;
        set
        {
            _corruptedLegendaryMonth = value;
            OnPropertyChanged();
        }
    }

    public int CorruptedLegendaryYear
    {
        get => _corruptedLegendaryYear;
        set
        {
            _corruptedLegendaryYear = value;
            OnPropertyChanged();
        }
    }

    #endregion

    public static string TranslationOpenWorld => LanguageController.Translation("OPEN_WORLD");
    public static string TranslationStaticDungeons => LanguageController.Translation("STATIC_DUNGEONS");
    public static string TranslationAvalonianRoads => LanguageController.Translation("AVALONIAN_ROADS");
    public static string TranslationRandomSoloDungeons => LanguageController.Translation("RANDOM_SOLO_DUNGEONS");
    public static string TranslationRandomGroupDungeons => LanguageController.Translation("RANDOM_GROUP_DUNGEONS");
    public static string TranslationHellGates => LanguageController.Translation("HELLGATES");
    public static string TranslationCorrupted => LanguageController.Translation("CORRUPTED");
    public static string TranslationMists => LanguageController.Translation("MISTS");
    public static string TranslationLast7Days => LanguageController.Translation("LAST_7_DAYS");
    public static string TranslationLast30Days => LanguageController.Translation("LAST_30_DAYS");
    public static string TranslationLast365Days => LanguageController.Translation("LAST_365_DAYS");
}