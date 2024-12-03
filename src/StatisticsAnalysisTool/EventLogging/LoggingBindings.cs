using StatisticsAnalysisTool.Common.UserSettings;
using StatisticsAnalysisTool.EventLogging.Notification;
using StatisticsAnalysisTool.Models.TranslationModel;
using StatisticsAnalysisTool.ViewModels;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.StorageHistory;

namespace StatisticsAnalysisTool.EventLogging;

public class LoggingBindings : BaseViewModel
{
    private ListCollectionView _gameLoggingCollectionView;
    private ObservableCollection<TrackingNotification> _trackingNotifications = new();
    private ObservableCollection<TopLooterObject> _topLooters = new();
    private bool _isTrackingSilver;
    private bool _isTrackingFame;
    private bool _isTrackingMobLoot;
    private ObservableCollection<LoggingFilterObject> _filters = new();
    private ListCollectionView _topLootersCollectionView;
    private ListCollectionView _lootingPlayersCollectionView;
    private ObservableCollection<LootingPlayer> _lootingPlayers = new();
    private LoggingTranslation _translation = new ();
    private CancellationTokenSource _cancellationTokenSource = new();
    private bool _isShowingLost = true;
    private bool _isShowingResolved = true;
    private bool _isShowingDonated = true;
    private bool _isShowingTrash = true;
    private bool _isShowingT1ToT3 = true;
    private bool _isShowingT4 = true;
    private bool _isShowingT5 = true;
    private bool _isShowingT6 = true;
    private bool _isShowingT7 = true;
    private bool _isShowingT8 = true;
    private bool _isShowingBag = true;
    private bool _isShowingCape = true;
    private bool _isShowingFood = true;
    private bool _isShowingPotion = true;
    private bool _isShowingMount = true;
    private bool _isShowingOthers = true;
    private Vault _currentVault = new ();
    private ObservableCollection<VaultContainer> _selectedContainers = new ();

    public void Init()
    {
        LootingPlayersCollectionView = CollectionViewSource.GetDefaultView(LootingPlayers) as ListCollectionView;

        TopLootersCollectionView = CollectionViewSource.GetDefaultView(TopLooters) as ListCollectionView;
        if (TopLootersCollectionView != null)
        {
            TopLootersCollectionView.IsLiveSorting = true;
            TopLootersCollectionView.CustomSort = new TopLooterComparer();
        }

        Filters.Add(new LoggingFilterObject(LoggingFilterType.Fame)
        {
            IsSelected = SettingsController.CurrentSettings.IsMainTrackerFilterFame,
            Name = MainWindowTranslation.Fame
        });

        Filters.Add(new LoggingFilterObject(LoggingFilterType.Silver)
        {
            IsSelected = SettingsController.CurrentSettings.IsMainTrackerFilterSilver,
            Name = MainWindowTranslation.Silver
        });

        Filters.Add(new LoggingFilterObject(LoggingFilterType.Faction)
        {
            IsSelected = SettingsController.CurrentSettings.IsMainTrackerFilterFaction,
            Name = MainWindowTranslation.Faction
        });

        Filters.Add(new LoggingFilterObject(LoggingFilterType.ConsumableLoot)
        {
            IsSelected = SettingsController.CurrentSettings.IsMainTrackerFilterConsumableLoot,
            Name = MainWindowTranslation.ConsumableLoot
        });

        Filters.Add(new LoggingFilterObject(LoggingFilterType.EquipmentLoot)
        {
            IsSelected = SettingsController.CurrentSettings.IsMainTrackerFilterEquipmentLoot,
            Name = MainWindowTranslation.EquipmentLoot
        });

        Filters.Add(new LoggingFilterObject(LoggingFilterType.SimpleLoot)
        {
            IsSelected = SettingsController.CurrentSettings.IsMainTrackerFilterSimpleLoot,
            Name = MainWindowTranslation.SimpleLoot
        });

        Filters.Add(new LoggingFilterObject(LoggingFilterType.UnknownLoot)
        {
            IsSelected = SettingsController.CurrentSettings.IsMainTrackerFilterUnknownLoot,
            Name = MainWindowTranslation.UnknownLoot
        });

        Filters.Add(new LoggingFilterObject(LoggingFilterType.ShowLootFromMob)
        {
            IsSelected = SettingsController.CurrentSettings.IsLootFromMobShown,
            Name = MainWindowTranslation.ShowLootFromMobs
        });

        Filters.Add(new LoggingFilterObject(LoggingFilterType.Kill)
        {
            IsSelected = SettingsController.CurrentSettings.IsMainTrackerFilterKill,
            Name = MainWindowTranslation.ShowKills
        });
    }

    #region Bindings

    public ObservableCollection<LootingPlayer> LootingPlayers
    {
        get => _lootingPlayers;
        set
        {
            _lootingPlayers = value;
            OnPropertyChanged();
        }
    }

    public ListCollectionView LootingPlayersCollectionView
    {
        get => _lootingPlayersCollectionView;
        set
        {
            _lootingPlayersCollectionView = value;
            OnPropertyChanged();
        }
    }

    public Vault CurrentVault
    {
        get => _currentVault;
        set
        {
            _currentVault = value;
            OnPropertyChanged();
        }
    }

    public ObservableCollection<VaultContainer> SelectedContainers
    {
        get => _selectedContainers;
        set
        {
            _selectedContainers = value;
            OnPropertyChanged();
        }
    }

    public ListCollectionView GameLoggingCollectionView
    {
        get => _gameLoggingCollectionView;
        set
        {
            _gameLoggingCollectionView = value;
            OnPropertyChanged();
        }
    }

    public ObservableCollection<TrackingNotification> TrackingNotifications
    {
        get => _trackingNotifications;
        set
        {
            _trackingNotifications = value;
            OnPropertyChanged();
        }
    }

    public ListCollectionView TopLootersCollectionView
    {
        get => _topLootersCollectionView;
        set
        {
            _topLootersCollectionView = value;
            OnPropertyChanged();
        }
    }

    public ObservableCollection<TopLooterObject> TopLooters
    {
        get => _topLooters;
        set
        {
            _topLooters = value;
            OnPropertyChanged();
        }
    }

    public bool IsTrackingSilver
    {
        get => _isTrackingSilver;
        set
        {
            _isTrackingSilver = value;

            SettingsController.CurrentSettings.IsTrackingSilver = _isTrackingSilver;
            OnPropertyChanged();
        }
    }

    public bool IsTrackingFame
    {
        get => _isTrackingFame;
        set
        {
            _isTrackingFame = value;

            SettingsController.CurrentSettings.IsTrackingFame = _isTrackingFame;
            OnPropertyChanged();
        }
    }

    public bool IsTrackingMobLoot
    {
        get => _isTrackingMobLoot;
        set
        {
            _isTrackingMobLoot = value;

            SettingsController.CurrentSettings.IsTrackingMobLoot = _isTrackingMobLoot;
            OnPropertyChanged();
        }
    }

    public ObservableCollection<LoggingFilterObject> Filters
    {
        get => _filters;
        set
        {
            _filters = value;
            OnPropertyChanged();
        }
    }

    public bool IsShowingLost
    {
        get => _isShowingLost;
        set
        {
            _isShowingLost = value;
            _ = UpdateFilteredLootedItemsAsync();
            OnPropertyChanged();
        }
    }

    public bool IsShowingResolved
    {
        get => _isShowingResolved;
        set
        {
            _isShowingResolved = value;
            _ = UpdateFilteredLootedItemsAsync();
            OnPropertyChanged();
        }
    }

    public bool IsShowingDonated
    {
        get => _isShowingDonated;
        set
        {
            _isShowingDonated = value;
            _ = UpdateFilteredLootedItemsAsync();
            OnPropertyChanged();
        }
    }

    public bool IsShowingTrash
    {
        get => _isShowingTrash;
        set
        {
            _isShowingTrash = value;
            _ = UpdateFilteredLootedItemsAsync();
            OnPropertyChanged();
        }
    }

    public bool IsShowingT1ToT3
    {
        get => _isShowingT1ToT3;
        set
        {
            _isShowingT1ToT3 = value;
            _ = UpdateFilteredLootedItemsAsync();
            OnPropertyChanged();
        }
    }

    public bool IsShowingT4
    {
        get => _isShowingT4;
        set
        {
            _isShowingT4 = value;
            _ = UpdateFilteredLootedItemsAsync();
            OnPropertyChanged();
        }
    }

    public bool IsShowingT5
    {
        get => _isShowingT5;
        set
        {
            _isShowingT5 = value;
            _ = UpdateFilteredLootedItemsAsync();
            OnPropertyChanged();
        }
    }

    public bool IsShowingT6
    {
        get => _isShowingT6;
        set
        {
            _isShowingT6 = value;
            _ = UpdateFilteredLootedItemsAsync();
            OnPropertyChanged();
        }
    }

    public bool IsShowingT7
    {
        get => _isShowingT7;
        set
        {
            _isShowingT7 = value;
            _ = UpdateFilteredLootedItemsAsync();
            OnPropertyChanged();
        }
    }

    public bool IsShowingT8
    {
        get => _isShowingT8;
        set
        {
            _isShowingT8 = value;
            _ = UpdateFilteredLootedItemsAsync();
            OnPropertyChanged();
        }
    }

    public bool IsShowingBag
    {
        get => _isShowingBag;
        set
        {
            _isShowingBag = value;
            _ = UpdateFilteredLootedItemsAsync();
            OnPropertyChanged();
        }
    }

    public bool IsShowingCape
    {
        get => _isShowingCape;
        set
        {
            _isShowingCape = value;
            _ = UpdateFilteredLootedItemsAsync();
            OnPropertyChanged();
        }
    }

    public bool IsShowingFood
    {
        get => _isShowingFood;
        set
        {
            _isShowingFood = value;
            _ = UpdateFilteredLootedItemsAsync();
            OnPropertyChanged();
        }
    }

    public bool IsShowingPotion
    {
        get => _isShowingPotion;
        set
        {
            _isShowingPotion = value;
            _ = UpdateFilteredLootedItemsAsync();
            OnPropertyChanged();
        }
    }

    public bool IsShowingMount
    {
        get => _isShowingMount;
        set
        {
            _isShowingMount = value;
            _ = UpdateFilteredLootedItemsAsync();
            OnPropertyChanged();
        }
    }

    public bool IsShowingOthers
    {
        get => _isShowingOthers;
        set
        {
            _isShowingOthers = value;
            _ = UpdateFilteredLootedItemsAsync();
            OnPropertyChanged();
        }
    }

    public LoggingTranslation Translation
    {
        get => _translation;
        set
        {
            _translation = value;
            OnPropertyChanged();
        }
    }

    #endregion

    #region Filter

    public async Task UpdateFilteredLootedItemsAsync()
    {
        if (LootingPlayers?.Count <= 0 && LootingPlayersCollectionView?.Count <= 0)
        {
            return;
        }

        _cancellationTokenSource = new CancellationTokenSource();

        try
        {
            var filteredLootedItems = await Task.Run(ParallelLootedItemsFilterProcess, _cancellationTokenSource.Token);
            LootingPlayersCollectionView = CollectionViewSource.GetDefaultView(filteredLootedItems) as ListCollectionView;
        }
        catch (TaskCanceledException)
        {
            // ignored
        }
    }

    public List<LootingPlayer> ParallelLootedItemsFilterProcess()
    {
        var partitioner = Partitioner.Create(LootingPlayers, EnumerablePartitionerOptions.NoBuffering);
        var result = new ConcurrentBag<LootingPlayer>();

        Parallel.ForEach(partitioner, (lootingPlayer, state) =>
        {
            if (_cancellationTokenSource.Token.IsCancellationRequested)
            {
                state.Stop();
            }
            
            foreach (var lootedItem in lootingPlayer.LootedItems.ToList())
            {
                lootedItem.Visibility = Filter(lootedItem) ? Visibility.Visible : Visibility.Collapsed;
            }

            if (lootingPlayer.LootedItems.Count >= 1)
            {
                result.Add(lootingPlayer);
            }
        });

        return result.OrderByDescending(d => d.PlayerName).ToList();
    }

    private bool Filter(LootedItem lootedItem)
    {
        bool isStatusOkay = false;
        bool isTierOkay = false;

        if (IsStatusOkay(lootedItem))
        {
            isStatusOkay = true;
        }

        if (IsTierOkay(lootedItem))
        {
            isTierOkay = true;
        }

        if (IsTypeOkay(lootedItem))
        {
            isTierOkay = true;
        }

        return isStatusOkay && isTierOkay;
    }

    private bool IsStatusOkay(LootedItem lootedItem)
    {
        if (_isShowingLost && lootedItem.Status == LootedItemStatus.Lost)
        {
            return true;
        }

        if (_isShowingResolved && lootedItem.Status == LootedItemStatus.Resolved)
        {
            return true;
        }

        if (_isShowingDonated && lootedItem.Status == LootedItemStatus.Donated)
        {
            return true;
        }

        return false;
    }

    private bool IsTierOkay(LootedItem lootedItem)
    {
        if (_isShowingT1ToT3 && lootedItem.Item.Tier is >= 1 and <= 3)
        {
            return true;
        }

        if (_isShowingT4 && lootedItem.Item.Tier == 4)
        {
            return true;
        }

        if (_isShowingT5 && lootedItem.Item.Tier == 5)
        {
            return true;
        }

        if (_isShowingT6 && lootedItem.Item.Tier == 6)
        {
            return true;
        }

        if (_isShowingT7 && lootedItem.Item.Tier == 7)
        {
            return true;
        }

        if (_isShowingT8 && lootedItem.Item.Tier == 8)
        {
            return true;
        }

        return false;
    }

    private bool IsTypeOkay(LootedItem lootedItem)
    {
        if (_isShowingFood && lootedItem.Item.ShopCategory == ShopCategory.Consumables && lootedItem.Item.ShopShopSubCategory1 is ShopSubCategory.Fish or ShopSubCategory.Cooked)
        {
            return true;
        }

        if (_isShowingPotion && lootedItem.Item.ShopCategory == ShopCategory.Consumables && lootedItem.Item.ShopShopSubCategory1 == ShopSubCategory.Potion)
        {
            return true;
        }

        if (_isShowingMount && lootedItem.Item.ShopCategory == ShopCategory.Mounts)
        {
            return true;
        }

        if (_isShowingOthers && lootedItem.Item.ShopCategory == ShopCategory.Other)
        {
            return true;
        }

        if (_isShowingBag && lootedItem.Item.ShopCategory == ShopCategory.Accessories && lootedItem.Item.ShopShopSubCategory1 == ShopSubCategory.Bag)
        {
            return true;
        }
        
        if (_isShowingCape && lootedItem.Item.ShopCategory == ShopCategory.Accessories && lootedItem.Item.ShopShopSubCategory1 == ShopSubCategory.Cape)
        {
            return true;
        }

        if (_isShowingTrash && lootedItem.IsTrash)
        {
            return true;
        }

        return false;
    }

    #endregion
}