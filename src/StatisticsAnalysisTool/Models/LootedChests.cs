using System.ComponentModel;
using System.Runtime.CompilerServices;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Properties;

namespace StatisticsAnalysisTool.Models;

public class LootedChests : INotifyPropertyChanged
{
    private int _outlandsCommonWeek;
    private int _outlandsCommonMonth;
    private int _outlandsUncommonWeek;
    private int _outlandsUncommonMonth;
    private int _outlandsEpicWeek;
    private int _outlandsEpicMonth;
    private int _outlandsLegendaryWeek;
    private int _outlandsLegendaryMonth;
    private int _staticCommonWeek;
    private int _staticCommonMonth;
    private int _staticUncommonWeek;
    private int _staticUncommonMonth;
    private int _staticEpicWeek;
    private int _staticEpicMonth;
    private int _staticLegendaryWeek;
    private int _staticLegendaryMonth;
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

    public LootedChests(LootedChestsSaveObject lootedChestsSaveObject)
    {
        OpenWorldCommonWeek = lootedChestsSaveObject.OpenWorldCommonWeek;
        OpenWorldCommonMonth = lootedChestsSaveObject.OpenWorldCommonMonth;
        OpenWorldUncommonWeek = lootedChestsSaveObject.OpenWorldUncommonWeek;
        OpenWorldUncommonMonth = lootedChestsSaveObject.OpenWorldUncommonMonth;
        OpenWorldEpicWeek = lootedChestsSaveObject.OpenWorldEpicWeek;
        OpenWorldEpicMonth = lootedChestsSaveObject.OpenWorldEpicMonth;
        OpenWorldLegendaryWeek = lootedChestsSaveObject.OpenWorldLegendaryWeek;
        OpenWorldLegendaryMonth = lootedChestsSaveObject.OpenWorldLegendaryMonth;

        StaticCommonWeek = lootedChestsSaveObject.StaticCommonWeek;
        StaticCommonMonth = lootedChestsSaveObject.StaticCommonMonth;
        StaticUncommonWeek = lootedChestsSaveObject.StaticUncommonWeek;
        StaticUncommonMonth = lootedChestsSaveObject.StaticUncommonMonth;
        StaticEpicWeek = lootedChestsSaveObject.StaticEpicWeek;
        StaticEpicMonth = lootedChestsSaveObject.StaticEpicMonth;
        StaticLegendaryWeek = lootedChestsSaveObject.StaticLegendaryWeek;
        StaticLegendaryMonth = lootedChestsSaveObject.StaticLegendaryMonth;

        AvalonianRoadCommonWeek = lootedChestsSaveObject.AvalonianRoadCommonWeek;
        AvalonianRoadCommonMonth = lootedChestsSaveObject.AvalonianRoadCommonMonth;
        AvalonianRoadUncommonWeek = lootedChestsSaveObject.AvalonianRoadUncommonWeek;
        AvalonianRoadUncommonMonth = lootedChestsSaveObject.AvalonianRoadUncommonMonth;
        AvalonianRoadEpicWeek = lootedChestsSaveObject.AvalonianRoadEpicWeek;
        AvalonianRoadEpicMonth = lootedChestsSaveObject.AvalonianRoadEpicMonth;
        AvalonianRoadLegendaryWeek = lootedChestsSaveObject.AvalonianRoadLegendaryWeek;
        AvalonianRoadLegendaryMonth = lootedChestsSaveObject.AvalonianRoadLegendaryMonth;

        HellGateCommonWeek = lootedChestsSaveObject.HellGateCommonWeek;
        HellGateCommonMonth = lootedChestsSaveObject.HellGateCommonMonth;
        HellGateUncommonWeek = lootedChestsSaveObject.HellGateUncommonWeek;
        HellGateUncommonMonth = lootedChestsSaveObject.HellGateUncommonMonth;
        HellGateEpicWeek = lootedChestsSaveObject.HellGateEpicWeek;
        HellGateEpicMonth = lootedChestsSaveObject.HellGateEpicMonth;
        HellGateLegendaryWeek = lootedChestsSaveObject.HellGateLegendaryWeek;
        HellGateLegendaryMonth = lootedChestsSaveObject.HellGateLegendaryMonth;
    }

    public LootedChests()
    {
    }

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

    #endregion

    #region Static bindings

    public int StaticCommonWeek
    {
        get => _staticCommonWeek;
        set
        {
            _staticCommonWeek = value;
            OnPropertyChanged();
        }
    }

    public int StaticCommonMonth
    {
        get => _staticCommonMonth;
        set
        {
            _staticCommonMonth = value;
            OnPropertyChanged();
        }
    }

    public int StaticUncommonWeek
    {
        get => _staticUncommonWeek;
        set
        {
            _staticUncommonWeek = value;
            OnPropertyChanged();
        }
    }

    public int StaticUncommonMonth
    {
        get => _staticUncommonMonth;
        set
        {
            _staticUncommonMonth = value;
            OnPropertyChanged();
        }
    }

    public int StaticEpicWeek
    {
        get => _staticEpicWeek;
        set
        {
            _staticEpicWeek = value;
            OnPropertyChanged();
        }
    }

    public int StaticEpicMonth
    {
        get => _staticEpicMonth;
        set
        {
            _staticEpicMonth = value;
            OnPropertyChanged();
        }
    }
    public int StaticLegendaryWeek
    {
        get => _staticLegendaryWeek;
        set
        {
            _staticLegendaryWeek = value;
            OnPropertyChanged();
        }
    }

    public int StaticLegendaryMonth
    {
        get => _staticLegendaryMonth;
        set
        {
            _staticLegendaryMonth = value;
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

    #endregion

    public static string TranslationLootedChests => LanguageController.Translation("LOOTED_CHESTS");
    public static string TranslationOpenWorld => LanguageController.Translation("OPEN_WORLD");
    public static string TranslationStaticDungeons => LanguageController.Translation("STATIC_DUNGEONS");
    public static string TranslationAvalonianRoads => LanguageController.Translation("AVALONIAN_ROADS");
    public static string TranslationHellGates => LanguageController.Translation("HELLGATES");

    public event PropertyChangedEventHandler PropertyChanged;

    [NotifyPropertyChangedInvocator]
    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}