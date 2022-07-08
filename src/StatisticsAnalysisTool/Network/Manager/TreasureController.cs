using log4net;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Models;
using StatisticsAnalysisTool.ViewModels;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace StatisticsAnalysisTool.Network.Manager;

public class TreasureController
{
    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);

    private readonly TrackingController _trackingController;
    private readonly MainWindowViewModel _mainWindowViewModel;
    private readonly ObservableCollection<TemporaryTreasure> _temporaryTreasures = new();
    private readonly ObservableCollection<Treasure> _treasures = new();

    public TreasureController(TrackingController trackingController, MainWindowViewModel mainWindowViewModel)
    {
        _trackingController = trackingController;
        _mainWindowViewModel = mainWindowViewModel;
    }

    public void AddTreasure(int objectId, string uniqueName, string uniqueNameWithLocation)
    {
        if (_temporaryTreasures.All(x => x.ObjectId != objectId))
        {
            _temporaryTreasures.Add(new TemporaryTreasure() { ObjectId = objectId, UniqueName = uniqueName, UniqueNameWithLocation = uniqueNameWithLocation });
        }
    }

    public void UpdateTreasure(int objectId, Guid openedBy)
    {
        if (openedBy == Guid.Empty)
        {
            return;
        }

        var temporaryTreasure = _temporaryTreasures?.FirstOrDefault(x => x?.ObjectId == objectId && x.AlreadyScanned == false);
        if (temporaryTreasure == null)
        {
            return;
        }

        var test = new Treasure()
        {
            OpenedBy = openedBy,
            TreasureRarity = GetRarity(temporaryTreasure.UniqueName),
            TreasureType = GetType(temporaryTreasure.UniqueName)
        };

        _treasures.Add(test);

        Debug.Print(test.TreasureRarity + " - " + test.TreasureType);
        temporaryTreasure.AlreadyScanned = true;
    }

    public void RemoveTemporaryTreasures()
    {
        _temporaryTreasures.Clear();
    }

    private static TreasureRarity GetRarity(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return TreasureRarity.Unknown;
        }

        if (Regex.IsMatch(value, "\\w*_STANDARD\\b|\\w*_STANDARD_[T][4-8]|\\w*_STANDARD_STANDARD_[T][4-8]|STATIC_\\w*_POI"))
        {
            return TreasureRarity.Standard;
        }
        if (Regex.IsMatch(value, "\\w*_UNCOMMON\\b|\\w*_UNCOMMON_[T][4-8]|\\w*_STANDARD_UNCOMMON_[T][4-8]|STATIC_\\w*_CHAMPION"))
        {
            return TreasureRarity.Uncommon;
        }

        if (Regex.IsMatch(value, "\\w*_RARE\\b|\\w*_RARE_[T][4-8]|\\w*_STANDARD_RARE_[T][4-8]|STATIC_\\w*_MINIBOSS"))
        {
            return TreasureRarity.Rare;
        }

        if (Regex.IsMatch(value, "\\w*_LEGENDARY\\b|\\w*_LEGENDARY_[T][4-8]|\\w*_STANDARD_LEGENDARY_[T][4-8]|STATIC_\\w*_BOSS"))
        {
            return TreasureRarity.Legendary;
        }

        return TreasureRarity.Unknown;
    }

    private static TreasureType GetType(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return TreasureType.Unknown;
        }

        if (value.Contains("TREASURE"))
        {
            return TreasureType.OpenWorld;
        }

        if (value.Contains("STATIC"))
        {
            return TreasureType.StaticDungeon;
        }

        if (value.Contains("AVALON"))
        {
            return TreasureType.Avalon;
        }

        if (value.Contains("CORRUPTED"))
        {
            return TreasureType.Corrupted;
        }

        if (value.Contains("HELL"))
        {
            return TreasureType.HellGate;
        }

        if (Regex.IsMatch(value, "_VETERAN_CHEST_|[^SOLO]_CHEST_BOSS_HALLOWEEN_"))
        {
            return TreasureType.RandomGroupDungeon;
        }

        if (Regex.IsMatch(value, "_SOLO_BOOKCHEST_|_SOLO_CHEST_"))
        {
            return TreasureType.RandomSoloDungeon;
        }

        return TreasureType.Unknown;
    }
}