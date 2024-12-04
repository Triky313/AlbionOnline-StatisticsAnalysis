using Ookii.Dialogs.Wpf;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Common.UserSettings;
using StatisticsAnalysisTool.EventLogging.Notification;
using StatisticsAnalysisTool.Localization;
using StatisticsAnalysisTool.Models.TranslationModel;
using StatisticsAnalysisTool.ViewModels;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

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
    private LoggingTranslation _translation = new();
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
    private ObservableCollection<VaultContainerLogItem> _vaultLogItems = new();
    private bool _isAllButtonsEnabled = true;

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

    #region Loot comparator

    public void UpdateItemsStatus()
    {
        // Remove all Vault items
        foreach (var player in LootingPlayers)
        {
            var itemsToRemove = player.LootedItems.Where(item => item.IsItemFromVaultLog).ToList();
            foreach (var item in itemsToRemove)
            {
                player.LootedItems.Remove(item);
            }
        }

        foreach (VaultContainerLogItem logItem in VaultLogItems)
        {
            var lootingPlayer = LootingPlayers.FirstOrDefault(x => x.PlayerName == logItem.PlayerName);
            var vaultLogLocalizedItem = ItemController.GetItemByLocalizedName(logItem.LocalizedName, logItem.Enchantment);

            if (vaultLogLocalizedItem is null)
            {
                continue;
            }

            var vaultLogItem = new LootedItem()
            {
                UtcPickupTime = logItem.Timestamp,
                ItemIndex = vaultLogLocalizedItem.Index,
                IsItemFromVaultLog = true,
                IsTrash = vaultLogLocalizedItem.ShopShopSubCategory1 == ShopSubCategory.Trash,
                LootedByName = logItem.PlayerName,
                Quantity = logItem.Quantity
            };

            if (lootingPlayer is not null)
            {
                var newItems = new List<LootedItem>();
                if (lootingPlayer.LootedItems.Count <= 0)
                {
                    newItems.Add(new LootedItem
                    {
                        UtcPickupTime = logItem.Timestamp,
                        ItemIndex = vaultLogLocalizedItem.Index,
                        IsItemFromVaultLog = true,
                        IsTrash = vaultLogLocalizedItem.ShopShopSubCategory1 == ShopSubCategory.Trash,
                        LootedByName = logItem.PlayerName,
                        Quantity = logItem.Quantity,
                        Status = LootedItemStatus.Donated
                    });
                }
                else
                {
                    foreach (LootedItem lootedItem in lootingPlayer.LootedItems)
                    {
                        if (lootedItem.GetHashCode() == vaultLogItem.GetHashCode())
                        {
                            lootedItem.Status = LootedItemStatus.Resolved;
                        }
                        else
                        {
                            newItems.Add(new LootedItem
                            {
                                UtcPickupTime = logItem.Timestamp,
                                ItemIndex = vaultLogLocalizedItem.Index,
                                IsItemFromVaultLog = true,
                                IsTrash = vaultLogLocalizedItem.ShopShopSubCategory1 == ShopSubCategory.Trash,
                                LootedByName = logItem.PlayerName,
                                Quantity = logItem.Quantity,
                                Status = LootedItemStatus.Donated
                            });
                        }
                    }
                }

                foreach (var newItem in newItems)
                {
                    lootingPlayer.LootedItems.Add(newItem);
                }
            }
            else
            {
                LootingPlayers.Add(new LootingPlayer()
                {
                    PlayerName = logItem.PlayerName,
                    LootingPlayerVisibility = Visibility.Visible,
                    LootedItems = new ObservableCollection<LootedItem>
                    {
                        new ()
                        {
                            UtcPickupTime = logItem.Timestamp,
                            ItemIndex = vaultLogLocalizedItem.Index,
                            IsItemFromVaultLog = true,
                            IsTrash = vaultLogLocalizedItem.ShopShopSubCategory1 == ShopSubCategory.Trash,
                            LootedByName = logItem.PlayerName,
                            Quantity = logItem.Quantity,
                            Status = LootedItemStatus.Donated
                        }
                    }
                });
            }
        }

        MarkLostItems(LootingPlayers);
    }

    private void MarkLostItems(ObservableCollection<LootingPlayer> lootingPlayers)
    {
        var itemOwnerMap = new Dictionary<int, LootingPlayer>();

        foreach (var player in lootingPlayers)
        {
            foreach (var item in player.LootedItems)
            {
                itemOwnerMap[item.GetHashCode()] = player;
            }
        }

        foreach (var player in lootingPlayers)
        {
            foreach (var item in player.LootedItems)
            {
                if (itemOwnerMap.ContainsKey(item.GetHashCode()))
                {
                    var originalOwner = itemOwnerMap[item.GetHashCode()];

                    if (originalOwner != player)
                    {
                        item.Status = LootedItemStatus.Lost;
                    }
                }
            }
        }
    }

    public void OpenVaultFilePathSelection()
    {
        var dialog = new VistaOpenFileDialog()
        {
            Filter = @"CSV files (*.csv)|*.csv",
            Title = LocalizationController.Translation("SELECT_CHEST_LOG_FILES"),
            Multiselect = true
        };

        IsAllButtonsEnabled = false;
        var result = dialog.ShowDialog();

        if (result.HasValue && result.Value)
        {
            List<VaultContainerLogItem> items = new List<VaultContainerLogItem>();

            foreach (var filePath in dialog.FileNames)
            {
                try
                {
                    var lines = File.ReadAllLines(filePath);
                    var parsedItems = lines.Skip(1).Select(ParseCsvLine).Where(item => item != null).ToList();
                    items.AddRange(parsedItems);
                }
                catch (Exception ex)
                {
                    Debug.Print($"Fehler beim Verarbeiten von Datei '{filePath}': {ex.Message}");
                }
            }

            VaultLogItems = new ObservableCollection<VaultContainerLogItem>(items.Where(x => x.Quantity > 0));

            Debug.Print($"Insgesamt {VaultLogItems.Count} Einträge erfolgreich geladen.");
        }

        IsAllButtonsEnabled = false;
    }

    private VaultContainerLogItem ParseCsvLine(string line)
    {
        try
        {
            var values = line.Split('\t');

            if (values.Length != 6)
            {
                return null;
            }

            for (int i = 0; i < values.Length; i++)
            {
                values[i] = values[i].Trim('"');
            }

            if (values[5].Length <= 0)
            {
                return null;
            }

            return new VaultContainerLogItem
            {
                Timestamp = DateTime.ParseExact(values[0], "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture),
                PlayerName = values[1],
                LocalizedName = values[2],
                Enchantment = int.Parse(values[3]),
                Quality = int.Parse(values[4]),
                Quantity = int.Parse(values[5])
            };
        }
        catch
        {
            return null;
        }
    }

    #endregion

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

    public ObservableCollection<VaultContainerLogItem> VaultLogItems
    {
        get => _vaultLogItems;
        set
        {
            _vaultLogItems = value;
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

    public bool IsAllButtonsEnabled
    {
        get => _isAllButtonsEnabled;
        set
        {
            _isAllButtonsEnabled = value;
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
        if (LootingPlayers is not { Count: > 0 } || LootingPlayersCollectionView == null)
        {
            return;
        }

        _cancellationTokenSource = new CancellationTokenSource();

        try
        {
            await Task.Run(ParallelLootedItemsFilterProcess, _cancellationTokenSource.Token);
            
            LootingPlayersCollectionView.Filter = item =>
            {
                if (item is LootingPlayer lootingPlayer)
                {
                    return lootingPlayer.LootingPlayerVisibility == Visibility.Visible;
                }

                return false;
            };
            
            LootingPlayersCollectionView.CustomSort = new LootingPlayerComparer();
        }
        catch (TaskCanceledException)
        {
            // ignore
        }
    }

    public void ParallelLootedItemsFilterProcess()
    {
        var partitioner = Partitioner.Create(LootingPlayers, EnumerablePartitionerOptions.NoBuffering);

        Parallel.ForEach(partitioner, (lootingPlayer, state) =>
        {
            if (_cancellationTokenSource.Token.IsCancellationRequested)
            {
                state.Stop();
            }

            var itemsToProcess = lootingPlayer.LootedItems.ToList();

            foreach (var lootedItem in itemsToProcess)
            {
                lootedItem.Visibility = Filter(lootedItem) ? Visibility.Visible : Visibility.Collapsed;
            }

            var hasVisibleItems = itemsToProcess.Any(item => item.Visibility == Visibility.Visible);
            lootingPlayer.LootingPlayerVisibility = hasVisibleItems ? Visibility.Visible : Visibility.Collapsed;
        });
    }

    public void SetPlayerVisibility(List<LootingPlayer> lootingPlayers)
    {
        foreach (LootingPlayer lootingPlayer in lootingPlayers)
        {
            if (lootingPlayer.LootedItems.Count > 0 && lootingPlayer.LootedItems.All(x => x.Visibility != Visibility.Visible))
            {
                lootingPlayer.LootingPlayerVisibility = Visibility.Collapsed;
            }
            else if (lootingPlayer.LootedItems.Count <= 0)
            {
                lootingPlayer.LootingPlayerVisibility = Visibility.Collapsed;
            }
            else
            {
                lootingPlayer.LootingPlayerVisibility = Visibility.Visible;
            }
        }
    }

    private bool Filter(LootedItem lootedItem)
    {
        bool isStatusOkay = false;
        bool isTierOkay = false;
        bool isTypeOkay = false;

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
            isTypeOkay = true;
        }

        return isStatusOkay && isTierOkay && isTypeOkay;
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

        if (lootedItem.Status == LootedItemStatus.Unknown)
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
        if (lootedItem.Item.ShopCategory is ShopCategory.Armor or ShopCategory.Melee or ShopCategory.Ranged or ShopCategory.Magic or ShopCategory.GatheringGear or ShopCategory.OffHand)
        {
            return true;
        }

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

        if (_isShowingOthers && lootedItem.Item.ShopCategory is ShopCategory.Other or ShopCategory.Artifact or ShopCategory.CityResources or ShopCategory.Farmable or ShopCategory.Furniture
            or ShopCategory.Labourers or ShopCategory.Products or ShopCategory.Materials or ShopCategory.LuxuryGoods or ShopCategory.Resources or ShopCategory.SkillBooks or ShopCategory.Token
            or ShopCategory.Trophies or ShopCategory.Tools or ShopCategory.Unknown)
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