using Ookii.Dialogs.Wpf;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Common.UserSettings;
using StatisticsAnalysisTool.EventLogging.Notification;
using StatisticsAnalysisTool.Localization;
using StatisticsAnalysisTool.Models;
using StatisticsAnalysisTool.Models.TranslationModel;
using StatisticsAnalysisTool.ViewModels;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace StatisticsAnalysisTool.EventLogging;

public class LoggingBindings : BaseViewModel
{
    private CancellationTokenSource _cancellationTokenSource = new();
    private ObservableCollection<LootingPlayer> _lootingPlayers = new();
    private readonly HashSet<LootingPlayer> _subscribedLootingPlayers = [];
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
    private bool _isShowingWeapon = true;
    private bool _isShowingArmor = true;
    private bool _isShowingFood = true;
    private bool _isShowingPotion = true;
    private bool _isShowingMount = true;
    private bool _isShowingOthers = true;
    private const int LootLogTimeToleranceSeconds = 2;
    private static readonly string[] LootLogHeaderColumns =
    [
        "timestamp_utc",
        "looted_by__alliance",
        "looted_by__guild",
        "looted_by__name",
        "item_id",
        "item_name",
        "quantity",
        "looted_from__alliance",
        "looted_from__guild",
        "looted_from__name"
    ];
    private int _chestLogSourceCount;
    private int _lootLogFileCount;

    public LoggingBindings()
    {
        SubscribeLootingPlayers(_lootingPlayers);
    }

    public void Init()
    {
        LootingPlayersCollectionView = CollectionViewSource.GetDefaultView(LootingPlayers) as ListCollectionView;

        TopLootersCollectionView = CollectionViewSource.GetDefaultView(TopLooters) as ListCollectionView;
        if (TopLootersCollectionView != null)
        {
            TopLootersCollectionView.IsLiveSorting = true;
            TopLootersCollectionView.CustomSort = new TopLooterComparer();
        }

        ClearFilters();

        AddFilter(new LoggingFilterObject(LoggingFilterType.Fame)
        {
            IsSelected = SettingsController.CurrentSettings.IsMainTrackerFilterFame,
            Name = MainWindowTranslation.Fame
        });

        AddFilter(new LoggingFilterObject(LoggingFilterType.Silver)
        {
            IsSelected = SettingsController.CurrentSettings.IsMainTrackerFilterSilver,
            Name = MainWindowTranslation.Silver
        });

        AddFilter(new LoggingFilterObject(LoggingFilterType.Faction)
        {
            IsSelected = SettingsController.CurrentSettings.IsMainTrackerFilterFaction,
            Name = MainWindowTranslation.Faction
        });

        AddFilter(new LoggingFilterObject(LoggingFilterType.ConsumableLoot)
        {
            IsSelected = SettingsController.CurrentSettings.IsMainTrackerFilterConsumableLoot,
            Name = MainWindowTranslation.ConsumableLoot
        });

        AddFilter(new LoggingFilterObject(LoggingFilterType.EquipmentLoot)
        {
            IsSelected = SettingsController.CurrentSettings.IsMainTrackerFilterEquipmentLoot,
            Name = MainWindowTranslation.EquipmentLoot
        });

        AddFilter(new LoggingFilterObject(LoggingFilterType.SimpleLoot)
        {
            IsSelected = SettingsController.CurrentSettings.IsMainTrackerFilterSimpleLoot,
            Name = MainWindowTranslation.SimpleLoot
        });

        AddFilter(new LoggingFilterObject(LoggingFilterType.UnknownLoot)
        {
            IsSelected = SettingsController.CurrentSettings.IsMainTrackerFilterUnknownLoot,
            Name = MainWindowTranslation.UnknownLoot
        });

        AddFilter(new LoggingFilterObject(LoggingFilterType.ShowLootFromMob)
        {
            IsSelected = SettingsController.CurrentSettings.IsLootFromMobShown,
            Name = MainWindowTranslation.ShowLootFromMobs
        });

        AddFilter(new LoggingFilterObject(LoggingFilterType.Kill)
        {
            IsSelected = SettingsController.CurrentSettings.IsMainTrackerFilterKill,
            Name = MainWindowTranslation.ShowKills
        });
    }

    private void AddFilter(LoggingFilterObject filter)
    {
        filter.PropertyChanged += FilterPropertyChanged;
        Filters.Add(filter);
    }

    private void ClearFilters()
    {
        foreach (var filter in Filters)
        {
            filter.PropertyChanged -= FilterPropertyChanged;
        }

        Filters.Clear();
    }

    private void FilterPropertyChanged(object sender, PropertyChangedEventArgs args)
    {
        if (args.PropertyName == nameof(LoggingFilterObject.IsSelected))
        {
            OnPropertyChanged(nameof(NotificationFilterSummary));
        }
    }

    private void SubscribeLootingPlayers(ObservableCollection<LootingPlayer> lootingPlayers)
    {
        lootingPlayers.CollectionChanged += LootingPlayersCollectionChanged;

        foreach (var lootingPlayer in lootingPlayers)
        {
            SubscribeLootingPlayer(lootingPlayer);
        }
    }

    private void UnsubscribeLootingPlayers(ObservableCollection<LootingPlayer> lootingPlayers)
    {
        lootingPlayers.CollectionChanged -= LootingPlayersCollectionChanged;

        foreach (var lootingPlayer in lootingPlayers)
        {
            UnsubscribeLootingPlayer(lootingPlayer);
        }
    }

    private void LootingPlayersCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
    {
        if (args.Action == NotifyCollectionChangedAction.Reset)
        {
            foreach (var lootingPlayer in _subscribedLootingPlayers.Except(LootingPlayers).ToList())
            {
                UnsubscribeLootingPlayer(lootingPlayer);
            }
        }

        if (args.OldItems is not null)
        {
            foreach (LootingPlayer oldPlayer in args.OldItems)
            {
                UnsubscribeLootingPlayer(oldPlayer);
            }
        }

        if (args.NewItems is not null)
        {
            foreach (LootingPlayer newPlayer in args.NewItems)
            {
                SubscribeLootingPlayer(newPlayer);
            }
        }

        RefreshLootComparatorGuildFilters();
    }

    private void SubscribeLootingPlayer(LootingPlayer lootingPlayer)
    {
        if (!_subscribedLootingPlayers.Add(lootingPlayer))
        {
            return;
        }

        lootingPlayer.PropertyChanged += LootingPlayerPropertyChanged;
    }

    private void UnsubscribeLootingPlayer(LootingPlayer lootingPlayer)
    {
        if (!_subscribedLootingPlayers.Remove(lootingPlayer))
        {
            return;
        }

        lootingPlayer.PropertyChanged -= LootingPlayerPropertyChanged;
    }

    private void LootingPlayerPropertyChanged(object sender, PropertyChangedEventArgs args)
    {
        if (args.PropertyName == nameof(LootingPlayer.PlayerGuild))
        {
            RefreshLootComparatorGuildFilters();
        }
    }

    #region Loot comparator

    public void UpdateItemsStatus()
    {
        RemoveAllVaultItems();
        ResetLootLogItemStatuses();

        foreach (var logItem in VaultLogItems.ToList())
        {
            var lootingPlayer = LootingPlayers.FirstOrDefault(x => string.Equals(x.PlayerName, logItem.PlayerName, StringComparison.OrdinalIgnoreCase));

            var vaultLogLocalizedItem = ItemController.GetItemByLocalizedName(logItem.LocalizedName, logItem.Enchantment);
            if (vaultLogLocalizedItem is null)
            {
                continue;
            }

            var utcTimestamp = ToUtc(logItem.Timestamp);

            var vaultLogItem = new LootedItem
            {
                UtcPickupTime = utcTimestamp,
                ItemIndex = vaultLogLocalizedItem.Index,
                IsItemFromVaultLog = true,
                IsTrash = string.Equals(vaultLogLocalizedItem.FullItemInformation?.ShopSubCategory1, "trash", StringComparison.OrdinalIgnoreCase),
                LootedByName = logItem.PlayerName, Quantity = logItem.Quantity
            };

            if (lootingPlayer is not null)
            {
                var newItems = new List<LootedItem>();

                if (lootingPlayer.LootedItemCount <= 0 || !MatchLootedItem(lootingPlayer, vaultLogItem))
                {
                    AddNewLootedItem(newItems, logItem, vaultLogLocalizedItem, LootedItemStatus.Donated);
                }

                foreach (var newItem in newItems)
                {
                    lootingPlayer.AddLootedItem(newItem);
                }
            }
            else
            {
                AddNewPlayerToLootingPlayers(logItem, vaultLogLocalizedItem, LootedItemStatus.Donated);
            }
        }

        MarkLostItems(LootingPlayers);
        ApplyLootComparatorFilters();
        RefreshLootComparatorLogCounts();
    }

    private static bool MatchLootedItem(LootingPlayer lootingPlayer, LootedItem vaultLogItem)
    {
        var matchedItem = lootingPlayer.GetLootedItemsSnapshot()
            .Where(lootedItem => !lootedItem.IsItemFromVaultLog)
            .Where(lootedItem => lootedItem.Status != LootedItemStatus.Resolved)
            .Where(lootedItem => lootedItem.ItemIndex == vaultLogItem.ItemIndex)
            .Where(lootedItem => string.Equals(lootedItem.LootedByName, vaultLogItem.LootedByName, StringComparison.OrdinalIgnoreCase))
            .Where(lootedItem => lootedItem.Quantity == vaultLogItem.Quantity)
            .Where(lootedItem => ToUtc(lootedItem.UtcPickupTime) <= vaultLogItem.UtcPickupTime)
            .OrderByDescending(lootedItem => ToUtc(lootedItem.UtcPickupTime))
            .FirstOrDefault();

        if (matchedItem is null)
        {
            return false;
        }

        matchedItem.Status = LootedItemStatus.Resolved;
        return true;
    }

    private static void AddNewLootedItem(List<LootedItem> newItems, VaultContainerLogItem logItem, Item vaultLogLocalizedItem, LootedItemStatus status)
    {
        newItems.Add(new LootedItem
        {
            UtcPickupTime = ToUtc(logItem.Timestamp),
            ItemIndex = vaultLogLocalizedItem.Index,
            IsItemFromVaultLog = true,
            IsTrash = vaultLogLocalizedItem.FullItemInformation?.ShopSubCategory1 == "trash",
            LootedByName = logItem.PlayerName,
            Quantity = logItem.Quantity,
            Status = status
        });
    }

    private void AddNewPlayerToLootingPlayers(VaultContainerLogItem logItem, Item vaultLogLocalizedItem, LootedItemStatus status)
    {
        LootingPlayers.Add(new LootingPlayer()
        {
            PlayerName = logItem.PlayerName,
            LootingPlayerVisibility = Visibility.Visible,
            LootedItems =
            [
                new LootedItem
                {
                    UtcPickupTime = ToUtc(logItem.Timestamp),
                    ItemIndex = vaultLogLocalizedItem.Index,
                    IsItemFromVaultLog = true,
                    IsTrash = vaultLogLocalizedItem.FullItemInformation?.ShopSubCategory1 == "trash",
                    LootedByName = logItem.PlayerName,
                    Quantity = logItem.Quantity,
                    Status = status
                }
            ]
        });
    }

    public void RemoveAllVaultItems()
    {
        var playerToRemove = new List<LootingPlayer>();
        foreach (var player in LootingPlayers.ToList())
        {
            var itemsToRemove = player.GetLootedItemsSnapshot().Where(item => item.IsItemFromVaultLog).ToList();
            player.RemoveLootedItems(itemsToRemove);

            if (player.LootedItemCount <= 0)
            {
                playerToRemove.Add(player);
            }
        }

        foreach (var player in playerToRemove)
        {
            LootingPlayers.Remove(player);
        }
    }

    private void ResetLootLogItemStatuses()
    {
        foreach (var player in LootingPlayers.ToList())
        {
            foreach (var item in player.GetLootedItemsSnapshot())
            {
                if (!item.IsItemFromVaultLog)
                {
                    item.Status = LootedItemStatus.Unknown;
                }
            }
        }
    }

    public void ClearVaultLogs()
    {
        VaultLogItems.Clear();
        _chestLogSourceCount = 0;
        RemoveAllVaultItems();
        ResetLootLogItemStatuses();
        RefreshLootComparatorLogCounts();
    }

    public void ClearLootLogs()
    {
        LootingPlayers.Clear();
        _lootLogFileCount = 0;
        RefreshLootComparatorLogCounts();
    }

    private void MarkLostItems(ObservableCollection<LootingPlayer> lootingPlayers)
    {
        var itemLootHistory = new Dictionary<int, List<(LootingPlayer Player, DateTime Timestamp)>>();

        foreach (var player in lootingPlayers)
        {
            foreach (var item in player.GetLootedItemsSnapshot())
            {
                if (!itemLootHistory.ContainsKey(item.ItemIndex))
                {
                    itemLootHistory[item.ItemIndex] = new List<(LootingPlayer, DateTime)>();
                }

                itemLootHistory[item.ItemIndex].Add((player, item.UtcPickupTime));
            }
        }

        foreach (var player in lootingPlayers)
        {
            foreach (var item in player.GetLootedItemsSnapshot())
            {
                if (itemLootHistory.TryGetValue(item.ItemIndex, out var history))
                {
                    var sortedHistory = history.OrderBy(h => h.Timestamp).ToList();

                    var playerIndex = sortedHistory.FindIndex(h => h.Player == player);

                    if (playerIndex >= 0 && playerIndex < sortedHistory.Count - 1)
                    {
                        var nextEntry = sortedHistory[playerIndex + 1];
                        if (nextEntry.Player != player)
                        {
                            item.Status = LootedItemStatus.Lost;
                        }
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
            var loadedFiles = LoadVaultLogFiles(dialog.FileNames);

            Debug.Print($"Loaded {loadedFiles} chest log files with {VaultLogItems.Count} chest log entries.");
        }

        IsAllButtonsEnabled = true;
    }

    internal int LoadVaultLogFiles(IEnumerable<string> filePaths)
    {
        var loadedFiles = 0;
        ClearLootComparatorImportEventLine();

        foreach (var filePath in filePaths)
        {
            try
            {
                if (!TryReadVaultLogText(File.ReadAllText(filePath), out var fileItems))
                {
                    SetLootComparatorImportEventLine("LOOT_COMPARATOR_INVALID_CHEST_LOG_FILE", Path.GetFileName(filePath));
                    continue;
                }

                var addedItems = AddVaultLogItems(fileItems);
                if (addedItems > 0)
                {
                    loadedFiles++;
                }
            }
            catch (Exception ex)
            {
                Debug.Print($"Error processing chest log file '{filePath}': {ex.Message}");
                SetLootComparatorImportEventLine("LOOT_COMPARATOR_INVALID_CHEST_LOG_FILE", Path.GetFileName(filePath));
            }
        }

        if (loadedFiles > 0)
        {
            _chestLogSourceCount += loadedFiles;
            RefreshLootComparatorLogCounts();
        }

        return loadedFiles;
    }

    public int AddVaultLogText(string chestLogText)
    {
        ClearLootComparatorImportEventLine();
        if (!TryReadVaultLogText(chestLogText, out var items))
        {
            SetLootComparatorImportEventLine(LocalizationController.Translation("LOOT_COMPARATOR_INVALID_CHEST_LOG_TEXT"));
            Debug.Print("Rejected chest log text because it does not match the expected chest log format.");
            return 0;
        }

        var addedItems = AddVaultLogItems(items);
        if (addedItems > 0)
        {
            _chestLogSourceCount++;
            RefreshLootComparatorLogCounts();
        }

        Debug.Print($"Added {addedItems} chest log entries from pasted chest log text.");

        return addedItems;
    }

    public void OpenLootLogFilePathSelection()
    {
        var dialog = new VistaOpenFileDialog()
        {
            Filter = @"CSV files (*.csv)|*.csv",
            Title = LocalizationController.Translation("SELECT_LOOT_LOG_FILES"),
            Multiselect = true
        };

        IsAllButtonsEnabled = false;
        var result = dialog.ShowDialog();

        if (result.HasValue && result.Value)
        {
            var addedItems = AddLootLogFiles(dialog.FileNames);
            Debug.Print($"Imported {addedItems} loot log entries.");
            _ = UpdateFilteredLootedItemsAsync();
        }

        IsAllButtonsEnabled = true;
    }

    internal int AddLootLogFiles(IEnumerable<string> filePaths)
    {
        var addedItems = 0;
        ClearLootComparatorImportEventLine();

        foreach (var filePath in filePaths)
        {
            try
            {
                var fileAddedItems = 0;
                if (!TryReadLootLogFile(filePath, out var lootLogItems))
                {
                    SetLootComparatorImportEventLine("LOOT_COMPARATOR_INVALID_LOOT_LOG_FILE", Path.GetFileName(filePath));
                    continue;
                }

                foreach (var lootLogItem in lootLogItems)
                {
                    if (TryAddLootLogItem(lootLogItem))
                    {
                        addedItems++;
                        fileAddedItems++;
                    }
                }

                if (fileAddedItems > 0)
                {
                    _lootLogFileCount++;
                }
            }
            catch (Exception ex)
            {
                Debug.Print($"Error processing loot log file '{filePath}': {ex.Message}");
                SetLootComparatorImportEventLine("LOOT_COMPARATOR_INVALID_LOOT_LOG_FILE", Path.GetFileName(filePath));
            }
        }

        RefreshLootComparatorLogCounts();
        return addedItems;
    }

    private static bool TryReadLootLogFile(string filePath, out List<ImportedLootLogItem> lootLogItems)
    {
        lootLogItems = [];
        var isHeaderLine = true;

        foreach (var line in File.ReadLines(filePath))
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            if (!TrySplitDelimitedLine(line, ';', out var values))
            {
                return false;
            }

            TrimDelimitedValues(values);

            if (isHeaderLine)
            {
                if (!IsLootLogHeader(values))
                {
                    return false;
                }

                isHeaderLine = false;
                continue;
            }

            if (!TryParseLootLogFields(values, out var lootLogItem))
            {
                return false;
            }

            lootLogItems.Add(lootLogItem);
        }

        return !isHeaderLine && lootLogItems.Count > 0;
    }

    private static bool IsLootLogHeader(string[] values)
    {
        return values.Length >= LootLogHeaderColumns.Length
               && LootLogHeaderColumns
                   .Select((columnName, index) => string.Equals(values[index], columnName, StringComparison.OrdinalIgnoreCase))
                   .All(isMatchingColumn => isMatchingColumn);
    }

    private static bool TryParseLootLogFields(string[] values, out ImportedLootLogItem lootLogItem)
    {
        lootLogItem = null;

        if (values is not { Length: >= 10 })
        {
            return false;
        }

        if (!TryParseLootLogTimestamp(values[0], out var utcPickupTime))
        {
            return false;
        }

        if (!int.TryParse(values[6], NumberStyles.Integer, CultureInfo.InvariantCulture, out var quantity) || quantity <= 0)
        {
            return false;
        }

        var item = GetLootLogItem(values[4], values[5]);
        if (item is null)
        {
            return false;
        }

        lootLogItem = new ImportedLootLogItem
        {
            UtcPickupTime = utcPickupTime,
            LootedByAlliance = values[1],
            LootedByGuild = values[2],
            LootedByName = values[3],
            Item = item,
            Quantity = quantity,
            LootedFromAlliance = values[7],
            LootedFromGuild = values[8],
            LootedFromName = values[9]
        };

        return !string.IsNullOrWhiteSpace(lootLogItem.LootedByName);
    }

    private static bool TryParseLootLogTimestamp(string value, out DateTime utcPickupTime)
    {
        utcPickupTime = DateTime.MinValue;

        if (DateTimeOffset.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var dateTimeOffset))
        {
            utcPickupTime = dateTimeOffset.UtcDateTime;
            return true;
        }

        if (DateTime.TryParseExact(value, SupportedFormats, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var dateTime))
        {
            utcPickupTime = dateTime;
            return true;
        }

        if (DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out dateTime))
        {
            utcPickupTime = dateTime;
            return true;
        }

        return false;
    }

    private static Item GetLootLogItem(string itemIdentifier, string itemName)
    {
        if (int.TryParse(itemIdentifier, NumberStyles.Integer, CultureInfo.InvariantCulture, out var itemIndex))
        {
            return ItemController.GetItemByIndex(itemIndex);
        }

        var item = ItemController.GetItemByUniqueName(itemIdentifier);
        if (item is not null)
        {
            return item;
        }

        var cleanItemIdentifier = ItemController.GetCleanUniqueName(itemIdentifier);
        if (!string.Equals(cleanItemIdentifier, itemIdentifier, StringComparison.Ordinal))
        {
            item = ItemController.GetItemByUniqueName(cleanItemIdentifier);
            if (item is not null)
            {
                return item;
            }
        }

        var enchantment = ItemController.GetItemLevel(itemIdentifier);
        return ItemController.GetItemByLocalizedName(itemName, enchantment);
    }

    private bool TryAddLootLogItem(ImportedLootLogItem lootLogItem)
    {
        if (IsDuplicateLootLogItem(lootLogItem))
        {
            return false;
        }

        var lootingPlayer = LootingPlayers.FirstOrDefault(x => string.Equals(x.PlayerName, lootLogItem.LootedByName, StringComparison.OrdinalIgnoreCase));
        if (lootingPlayer is not null)
        {
            UpdateLootingPlayerAffiliations(lootingPlayer, lootLogItem);
            lootingPlayer.AddLootedItem(CreateLootedItem(lootLogItem));
            return true;
        }

        LootingPlayers.Add(new LootingPlayer()
        {
            PlayerName = lootLogItem.LootedByName,
            PlayerGuild = lootLogItem.LootedByGuild,
            PlayerAlliance = lootLogItem.LootedByAlliance,
            LootingPlayerVisibility = Visibility.Visible,
            LootedItems =
            [
                CreateLootedItem(lootLogItem)
            ]
        });

        return true;
    }

    private bool IsDuplicateLootLogItem(ImportedLootLogItem lootLogItem)
    {
        return LootingPlayers
            .SelectMany(player => player.GetLootedItemsSnapshot())
            .Where(item => !item.IsItemFromVaultLog)
            .Any(item => IsSameLootLogItem(item, lootLogItem));
    }

    private static bool IsSameLootLogItem(LootedItem lootedItem, ImportedLootLogItem lootLogItem)
    {
        return lootedItem.ItemIndex == lootLogItem.Item.Index
               && lootedItem.Quantity == lootLogItem.Quantity
               && string.Equals(lootedItem.LootedByName, lootLogItem.LootedByName, StringComparison.OrdinalIgnoreCase)
               && Math.Abs((lootedItem.UtcPickupTime - lootLogItem.UtcPickupTime).TotalSeconds) <= LootLogTimeToleranceSeconds;
    }

    private static LootedItem CreateLootedItem(ImportedLootLogItem lootLogItem)
    {
        return new LootedItem
        {
            UtcPickupTime = lootLogItem.UtcPickupTime,
            ItemIndex = lootLogItem.Item.Index,
            Quantity = lootLogItem.Quantity,
            LootedByName = lootLogItem.LootedByName,
            LootedFromName = lootLogItem.LootedFromName,
            LootedFromGuild = lootLogItem.LootedFromGuild,
            IsTrash = string.Equals(lootLogItem.Item.FullItemInformation?.ShopSubCategory1, "trash", StringComparison.OrdinalIgnoreCase)
        };
    }

    private static void UpdateLootingPlayerAffiliations(LootingPlayer lootingPlayer, ImportedLootLogItem lootLogItem)
    {
        if (string.IsNullOrWhiteSpace(lootingPlayer.PlayerGuild) && !string.IsNullOrWhiteSpace(lootLogItem.LootedByGuild))
        {
            lootingPlayer.PlayerGuild = lootLogItem.LootedByGuild;
        }

        if (string.IsNullOrWhiteSpace(lootingPlayer.PlayerAlliance) && !string.IsNullOrWhiteSpace(lootLogItem.LootedByAlliance))
        {
            lootingPlayer.PlayerAlliance = lootLogItem.LootedByAlliance;
        }
    }

    private static IEnumerable<VaultContainerLogItem> ReadVaultLogText(string chestLogText)
    {
        return TryReadVaultLogText(chestLogText, out var items) ? items : [];
    }

    private static bool TryReadVaultLogText(string chestLogText, out List<VaultContainerLogItem> items)
    {
        items = [];

        if (string.IsNullOrWhiteSpace(chestLogText))
        {
            return false;
        }

        using var reader = new StringReader(chestLogText);
        var isFirstDataLine = true;
        var hasDataLine = false;

        while (reader.ReadLine() is { } line)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            if (isFirstDataLine && IsVaultLogHeader(line))
            {
                isFirstDataLine = false;
                continue;
            }

            isFirstDataLine = false;
            hasDataLine = true;
            if (!TryParseVaultLogLine(line, out var item))
            {
                return false;
            }

            items.Add(item);
        }

        return hasDataLine;
    }

    private static bool IsVaultLogHeader(string line)
    {
        var values = SplitVaultLogLine(line);
        return values.Length >= 6
               && IsAnyVaultLogColumn(values[0], "Datum", "Date")
               && IsAnyVaultLogColumn(values[1], "Spieler", "Player")
               && IsAnyVaultLogColumn(values[2], "Gegenstand", "Item")
               && IsAnyVaultLogColumn(values[3], "Verzauberung", "Enchantment")
               && IsAnyVaultLogColumn(values[4], "Qualit\u00E4t", "Qualitat", "Quality")
               && IsAnyVaultLogColumn(values[5], "Anzahl", "Amount");
    }

    private static bool TryParseVaultLogLine(string line, out VaultContainerLogItem item)
    {
        item = null;
        var values = SplitVaultLogLine(line);

        if (values.Length != 6)
        {
            return false;
        }

        if (values[5].Length <= 0)
        {
            return false;
        }

        if (!TryParseVaultLogTimestamp(values[0], out var utcTimestamp))
        {
            return false;
        }

        if (!int.TryParse(values[3], NumberStyles.Integer, CultureInfo.InvariantCulture, out var enchantment))
        {
            return false;
        }

        if (!int.TryParse(values[4], NumberStyles.Integer, CultureInfo.InvariantCulture, out var quality))
        {
            return false;
        }

        if (!int.TryParse(values[5], NumberStyles.Integer, CultureInfo.InvariantCulture, out var quantity))
        {
            return false;
        }

        item = new VaultContainerLogItem
        {
            Timestamp = utcTimestamp,
            PlayerName = values[1],
            LocalizedName = values[2],
            Enchantment = enchantment,
            Quality = quality,
            Quantity = quantity
        };

        return !string.IsNullOrWhiteSpace(item.PlayerName) && !string.IsNullOrWhiteSpace(item.LocalizedName);
    }

    private static bool TryParseVaultLogTimestamp(string value, out DateTime utcTimestamp)
    {
        utcTimestamp = DateTime.MinValue;

        if (!DateTime.TryParseExact(value, SupportedFormats, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out var localTimestamp))
        {
            return false;
        }

        utcTimestamp = ToUtc(localTimestamp);
        return true;
    }

    private static DateTime ToUtc(DateTime timestamp)
    {
        return timestamp.Kind switch
        {
            DateTimeKind.Utc => timestamp,
            DateTimeKind.Local => timestamp.ToUniversalTime(),
            _ => DateTime.SpecifyKind(timestamp, DateTimeKind.Local).ToUniversalTime()
        };
    }

    private static string[] SplitVaultLogLine(string line)
    {
        var delimiter = line.Contains('\t') ? '\t' : ',';
        if (!TrySplitDelimitedLine(line, delimiter, out var values))
        {
            return [];
        }

        TrimDelimitedValues(values);
        return values;
    }

    private static bool IsAnyVaultLogColumn(string value, params string[] expectedColumnNames)
    {
        return expectedColumnNames.Any(expectedColumnName => string.Equals(value, expectedColumnName, StringComparison.OrdinalIgnoreCase));
    }

    private static void TrimDelimitedValues(string[] values)
    {
        for (var i = 0; i < values.Length; i++)
        {
            values[i] = values[i].Trim().TrimStart('\uFEFF');
        }
    }

    private static bool TrySplitDelimitedLine(string line, char delimiter, out string[] values)
    {
        values = [];
        List<string> fields = [];
        var currentField = new StringBuilder();
        var isInsideQuotes = false;

        for (var i = 0; i < line.Length; i++)
        {
            var currentChar = line[i];

            if (currentChar == '"')
            {
                if (isInsideQuotes && i + 1 < line.Length && line[i + 1] == '"')
                {
                    currentField.Append('"');
                    i++;
                    continue;
                }

                isInsideQuotes = !isInsideQuotes;
                continue;
            }

            if (currentChar == delimiter && !isInsideQuotes)
            {
                fields.Add(currentField.ToString());
                currentField.Clear();
                continue;
            }

            currentField.Append(currentChar);
        }

        if (isInsideQuotes)
        {
            return false;
        }

        fields.Add(currentField.ToString());
        values = fields.ToArray();
        return true;
    }

    private int AddVaultLogItems(IEnumerable<VaultContainerLogItem> items)
    {
        var addedItems = 0;
        foreach (var item in items.Where(x => x.Quantity > 0))
        {
            if (VaultLogItems.Any(existingItem => IsSameVaultLogItem(existingItem, item)))
            {
                continue;
            }

            VaultLogItems.Add(item);
            addedItems++;
        }

        return addedItems;
    }

    private static bool IsSameVaultLogItem(VaultContainerLogItem firstItem, VaultContainerLogItem secondItem)
    {
        return ToUtc(firstItem.Timestamp) == ToUtc(secondItem.Timestamp)
               && string.Equals(firstItem.PlayerName, secondItem.PlayerName, StringComparison.OrdinalIgnoreCase)
               && string.Equals(firstItem.LocalizedName, secondItem.LocalizedName, StringComparison.OrdinalIgnoreCase)
               && firstItem.Enchantment == secondItem.Enchantment
               && firstItem.Quality == secondItem.Quality
               && firstItem.Quantity == secondItem.Quantity;
    }

    public Visibility IsLootComparatorInfoPopupVisible
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = Visibility.Collapsed;

    public void ToggleLootComparatorInfoPopupVisibility()
    {
        IsLootComparatorInfoPopupVisible = IsLootComparatorInfoPopupVisible == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
    }

    private void RemoveLootingPlayer(object value)
    {
        if (value is not LootingPlayer lootingPlayer)
        {
            return;
        }

        RemoveLootingPlayer(lootingPlayer);
    }

    public void RemoveLootingPlayer(LootingPlayer lootingPlayer)
    {
        if (lootingPlayer is null)
        {
            return;
        }

        RemoveVaultLogItemsForPlayer(lootingPlayer.PlayerName);
        LootingPlayers.Remove(lootingPlayer);
        LootingPlayersCollectionView?.Refresh();
        RefreshLootComparatorLogCounts();
    }

    private void RemoveVaultLogItemsForPlayer(string playerName)
    {
        if (string.IsNullOrWhiteSpace(playerName))
        {
            return;
        }

        var vaultItemsToRemove = VaultLogItems
            .Where(item => string.Equals(item.PlayerName, playerName, StringComparison.OrdinalIgnoreCase))
            .ToList();

        foreach (var vaultItem in vaultItemsToRemove)
        {
            VaultLogItems.Remove(vaultItem);
        }
    }

    public string StatusFilterSummary => BuildFilterSummary(LoggingTranslation.FilterStatus, CountSelectedFilters(IsShowingLost, IsShowingResolved, IsShowingDonated, IsShowingTrash), 4);
    public string TierFilterSummary => BuildFilterSummary(LoggingTranslation.FilterTier, CountSelectedFilters(IsShowingT1ToT3, IsShowingT4, IsShowingT5, IsShowingT6, IsShowingT7, IsShowingT8), 6);
    public string TypeFilterSummary => BuildFilterSummary(LoggingTranslation.FilterType, CountSelectedFilters(IsShowingFood, IsShowingPotion, IsShowingBag, IsShowingCape, IsShowingWeapon, IsShowingArmor, IsShowingMount, IsShowingOthers), 8);
    public string GuildFilterSummary => LootComparatorGuildFilters.Count <= 0
        ? $"{LoggingTranslation.Guild}: {LoggingTranslation.FilterAll}"
        : BuildFilterSummary(LoggingTranslation.Guild, LootComparatorGuildFilters.Count(filter => filter.IsSelected), LootComparatorGuildFilters.Count);
    public string NotificationFilterSummary => BuildFilterSummary(LoggingTranslation.Filter, Filters.Count(filter => filter.IsSelected == true), Filters.Count);
    public string TrackingSummary => BuildFilterSummary(LoggingTranslation.Tracking, CountSelectedFilters(IsTrackingPartyLootOnly, IsTrackingSilver, IsTrackingFame, IsTrackingMobLoot, IsTrackingKill), 5);
    public int ChestLogCount => _chestLogSourceCount;
    public int LootLogCount => _lootLogFileCount;
    public string LootComparatorLogCountSummary => LocalizationController.Translation("LOOT_COMPARATOR_LOG_COUNT_SUMMARY",
        ["CHEST_COUNT", "LOOT_COUNT"],
        [ChestLogCount.ToString(CultureInfo.InvariantCulture), LootLogCount.ToString(CultureInfo.InvariantCulture)]);
    public string LootComparatorLogCountTooltip => LocalizationController.Translation("LOOT_COMPARATOR_LOG_COUNT_TOOLTIP");

    private static string BuildFilterSummary(string filterName, int selectedCount, int totalCount)
    {
        var selectedText = selectedCount switch
        {
            0 => LoggingTranslation.FilterNone,
            var count when count == totalCount => LoggingTranslation.FilterAll,
            _ => LocalizationController.Translation("LOOT_FILTER_SELECTED_COUNT",
                ["COUNT"],
                [selectedCount.ToString(CultureInfo.InvariantCulture)])
        };

        return $"{filterName}: {selectedText}";
    }

    public void RefreshLootComparatorLogCounts()
    {
        OnPropertyChanged(nameof(ChestLogCount));
        OnPropertyChanged(nameof(LootLogCount));
        OnPropertyChanged(nameof(LootComparatorLogCountSummary));
        OnPropertyChanged(nameof(LootComparatorLogCountTooltip));
    }

    private void ClearLootComparatorImportEventLine()
    {
        SetLootComparatorImportEventLine(string.Empty);
    }

    private void SetLootComparatorImportEventLine(string translationKey, string fileName)
    {
        var message = LocalizationController.Translation(translationKey,
            ["FILE_NAME"],
            [fileName]);

        SetLootComparatorImportEventLine(message == translationKey ? $"{translationKey}: {fileName}" : message);
    }

    private void SetLootComparatorImportEventLine(string message)
    {
        LootComparatorImportEventLine = message;
    }

    private static int CountSelectedFilters(params bool[] values)
    {
        return values.Count(value => value);
    }

    private void RefreshLootComparatorGuildFilters()
    {
        var previousSelections = LootComparatorGuildFilters
            .GroupBy(filter => filter.GuildName, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.Last().IsSelected, StringComparer.OrdinalIgnoreCase);

        foreach (var filter in LootComparatorGuildFilters)
        {
            filter.PropertyChanged -= LootComparatorGuildFilterPropertyChanged;
        }

        LootComparatorGuildFilters.Clear();

        var guildNames = LootingPlayers
            .Select(player => player.PlayerGuild)
            .Where(guildName => !string.IsNullOrWhiteSpace(guildName))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(guildName => guildName, StringComparer.OrdinalIgnoreCase);

        foreach (var guildName in guildNames)
        {
            var filter = new LootComparatorGuildFilter(guildName)
            {
                IsSelected = !previousSelections.TryGetValue(guildName, out var wasSelected) || wasSelected
            };

            filter.PropertyChanged += LootComparatorGuildFilterPropertyChanged;
            LootComparatorGuildFilters.Add(filter);
        }

        OnPropertyChanged(nameof(GuildFilterSummary));
    }

    private void LootComparatorGuildFilterPropertyChanged(object sender, PropertyChangedEventArgs args)
    {
        if (args.PropertyName == nameof(LootComparatorGuildFilter.IsSelected))
        {
            NotifyGuildFilterChanged();
        }
    }

    private void NotifyStatusFilterChanged()
    {
        _ = UpdateFilteredLootedItemsAsync();
        OnPropertyChanged(nameof(StatusFilterSummary));
    }

    private void NotifyTierFilterChanged()
    {
        _ = UpdateFilteredLootedItemsAsync();
        OnPropertyChanged(nameof(TierFilterSummary));
    }

    private void NotifyTypeFilterChanged()
    {
        _ = UpdateFilteredLootedItemsAsync();
        OnPropertyChanged(nameof(TypeFilterSummary));
    }

    private void NotifyGuildFilterChanged()
    {
        _ = UpdateFilteredLootedItemsAsync();
        OnPropertyChanged(nameof(GuildFilterSummary));
    }

    private static readonly string[] SupportedFormats =
    [
        "MM/dd/yyyy HH:mm:ss",
        "dd/MM/yyyy HH:mm:ss",
        "yyyy-MM-dd HH:mm:ss",
        "dd.MM.yyyy HH:mm:ss"
    ];

    private sealed class ImportedLootLogItem
    {
        public DateTime UtcPickupTime { get; init; }
        public string LootedByAlliance { get; init; }
        public string LootedByGuild { get; init; }
        public string LootedByName { get; init; }
        public Item Item { get; init; }
        public int Quantity { get; init; }
        public string LootedFromAlliance { get; init; }
        public string LootedFromGuild { get; init; }
        public string LootedFromName { get; init; }
    }

    #endregion

    #region Bindings

    public ObservableCollection<LootingPlayer> LootingPlayers
    {
        get => _lootingPlayers;
        set
        {
            UnsubscribeLootingPlayers(_lootingPlayers);
            _lootingPlayers = value ?? [];
            SubscribeLootingPlayers(_lootingPlayers);
            OnPropertyChanged();
            RefreshLootComparatorGuildFilters();
        }
    }

    public ListCollectionView LootingPlayersCollectionView
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public ObservableCollection<VaultContainerLogItem> VaultLogItems
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = [];

    public string ChestLogText
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = string.Empty;

    public string LootComparatorImportEventLine
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(LootComparatorImportEventLineVisibility));
        }
    } = string.Empty;

    public Visibility LootComparatorImportEventLineVisibility => string.IsNullOrWhiteSpace(LootComparatorImportEventLine) ? Visibility.Collapsed : Visibility.Visible;

    public ListCollectionView GameLoggingCollectionView
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public ObservableCollection<TrackingNotification> TrackingNotifications
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = new();

    public LootLoggerStats LootLoggerStats
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = new();

    public ListCollectionView TopLootersCollectionView
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public ObservableCollection<TopLooterObject> TopLooters
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = new();

    public bool IsTrackingSilver
    {
        get;
        set
        {
            field = value;

            SettingsController.CurrentSettings.IsTrackingSilver = field;
            OnPropertyChanged();
            OnPropertyChanged(nameof(TrackingSummary));
        }
    }

    public bool IsTrackingFame
    {
        get;
        set
        {
            field = value;

            SettingsController.CurrentSettings.IsTrackingFame = field;
            OnPropertyChanged();
            OnPropertyChanged(nameof(TrackingSummary));
        }
    }

    public bool IsTrackingMobLoot
    {
        get;
        set
        {
            field = value;

            SettingsController.CurrentSettings.IsTrackingMobLoot = field;
            OnPropertyChanged();
            OnPropertyChanged(nameof(TrackingSummary));
        }
    }

    public bool IsTrackingPartyLootOnly
    {
        get;
        set
        {
            field = value;

            SettingsController.CurrentSettings.IsTrackingPartyLootOnly = field;
            OnPropertyChanged();
            OnPropertyChanged(nameof(TrackingSummary));
        }
    }

    public bool IsTrackingKill
    {
        get;
        set
        {
            field = value;

            SettingsController.CurrentSettings.IsTrackingKill = field;
            OnPropertyChanged();
            OnPropertyChanged(nameof(TrackingSummary));
        }
    }

    public ObservableCollection<LoggingFilterObject> Filters
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = new();

    public ObservableCollection<LootComparatorGuildFilter> LootComparatorGuildFilters
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(GuildFilterSummary));
        }
    } = new();

    public bool IsShowingLost
    {
        get => _isShowingLost;
        set
        {
            _isShowingLost = value;
            OnPropertyChanged();
            NotifyStatusFilterChanged();
        }
    }

    public bool IsShowingResolved
    {
        get => _isShowingResolved;
        set
        {
            _isShowingResolved = value;
            OnPropertyChanged();
            NotifyStatusFilterChanged();
        }
    }

    public bool IsShowingDonated
    {
        get => _isShowingDonated;
        set
        {
            _isShowingDonated = value;
            OnPropertyChanged();
            NotifyStatusFilterChanged();
        }
    }

    public bool IsShowingTrash
    {
        get => _isShowingTrash;
        set
        {
            _isShowingTrash = value;
            OnPropertyChanged();
            NotifyStatusFilterChanged();
        }
    }

    public bool IsShowingT1ToT3
    {
        get => _isShowingT1ToT3;
        set
        {
            _isShowingT1ToT3 = value;
            OnPropertyChanged();
            NotifyTierFilterChanged();
        }
    }

    public bool IsShowingT4
    {
        get => _isShowingT4;
        set
        {
            _isShowingT4 = value;
            OnPropertyChanged();
            NotifyTierFilterChanged();
        }
    }

    public bool IsShowingT5
    {
        get => _isShowingT5;
        set
        {
            _isShowingT5 = value;
            OnPropertyChanged();
            NotifyTierFilterChanged();
        }
    }

    public bool IsShowingT6
    {
        get => _isShowingT6;
        set
        {
            _isShowingT6 = value;
            OnPropertyChanged();
            NotifyTierFilterChanged();
        }
    }

    public bool IsShowingT7
    {
        get => _isShowingT7;
        set
        {
            _isShowingT7 = value;
            OnPropertyChanged();
            NotifyTierFilterChanged();
        }
    }

    public bool IsShowingT8
    {
        get => _isShowingT8;
        set
        {
            _isShowingT8 = value;
            OnPropertyChanged();
            NotifyTierFilterChanged();
        }
    }

    public bool IsShowingBag
    {
        get => _isShowingBag;
        set
        {
            _isShowingBag = value;
            OnPropertyChanged();
            NotifyTypeFilterChanged();
        }
    }

    public bool IsShowingCape
    {
        get => _isShowingCape;
        set
        {
            _isShowingCape = value;
            OnPropertyChanged();
            NotifyTypeFilterChanged();
        }
    }

    public bool IsShowingWeapon
    {
        get => _isShowingWeapon;
        set
        {
            _isShowingWeapon = value;
            OnPropertyChanged();
            NotifyTypeFilterChanged();
        }
    }

    public bool IsShowingArmor
    {
        get => _isShowingArmor;
        set
        {
            _isShowingArmor = value;
            OnPropertyChanged();
            NotifyTypeFilterChanged();
        }
    }

    public bool IsShowingFood
    {
        get => _isShowingFood;
        set
        {
            _isShowingFood = value;
            OnPropertyChanged();
            NotifyTypeFilterChanged();
        }
    }

    public bool IsShowingPotion
    {
        get => _isShowingPotion;
        set
        {
            _isShowingPotion = value;
            OnPropertyChanged();
            NotifyTypeFilterChanged();
        }
    }

    public bool IsShowingMount
    {
        get => _isShowingMount;
        set
        {
            _isShowingMount = value;
            OnPropertyChanged();
            NotifyTypeFilterChanged();
        }
    }

    public bool IsShowingOthers
    {
        get => _isShowingOthers;
        set
        {
            _isShowingOthers = value;
            OnPropertyChanged();
            NotifyTypeFilterChanged();
        }
    }

    public bool IsAllButtonsEnabled
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = true;

    public LoggingTranslation Translation
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    } = new();

    public ICommand RemoveLootingPlayerCommand => field ??= new CommandHandler(RemoveLootingPlayer, true);

    #endregion

    #region Loot comparator filter

    public async Task UpdateFilteredLootedItemsAsync()
    {
        if (LootingPlayers is not { Count: > 0 })
        {
            return;
        }

        _cancellationTokenSource = new CancellationTokenSource();

        try
        {
            await Task.Run(ParallelLootedItemsFilterProcess, _cancellationTokenSource.Token);
            ApplyLootingPlayersCollectionViewFilter();
        }
        catch (TaskCanceledException)
        {
            // ignore
        }
    }

    private void ApplyLootComparatorFilters()
    {
        if (LootingPlayers is not { Count: > 0 })
        {
            return;
        }

        _cancellationTokenSource = new CancellationTokenSource();
        ParallelLootedItemsFilterProcess();
        ApplyLootingPlayersCollectionViewFilter();
    }

    private void ApplyLootingPlayersCollectionViewFilter()
    {
        if (LootingPlayersCollectionView == null)
        {
            return;
        }

        LootingPlayersCollectionView.Filter = item =>
        {
            if (item is LootingPlayer lootingPlayer)
            {
                return lootingPlayer.LootingPlayerVisibility == Visibility.Visible;
            }

            return false;
        };

        LootingPlayersCollectionView.CustomSort = new LootingPlayerComparer();
        LootingPlayersCollectionView.Refresh();
    }

    public void ParallelLootedItemsFilterProcess()
    {
        var partitioner = Partitioner.Create(LootingPlayers, EnumerablePartitionerOptions.NoBuffering);
        var guildFilters = LootComparatorGuildFilters.ToList();
        var selectedGuilds = guildFilters
            .Where(filter => filter.IsSelected)
            .Select(filter => filter.GuildName)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        var isGuildFilterRestricted = guildFilters.Count > 0 && selectedGuilds.Count < guildFilters.Count;

        Parallel.ForEach(partitioner, (lootingPlayer, state) =>
        {
            if (_cancellationTokenSource.Token.IsCancellationRequested)
            {
                state.Stop();
            }

            var itemsToProcess = lootingPlayer.GetLootedItemsSnapshot();

            foreach (var lootedItem in itemsToProcess)
            {
                lootedItem.Visibility = Filter(lootedItem, lootingPlayer, isGuildFilterRestricted, selectedGuilds) ? Visibility.Visible : Visibility.Collapsed;
            }

            var hasVisibleItems = itemsToProcess.Any(item => item.Visibility == Visibility.Visible);
            lootingPlayer.LootingPlayerVisibility = hasVisibleItems ? Visibility.Visible : Visibility.Collapsed;
        });
    }

    public void SetPlayerVisibility(List<LootingPlayer> lootingPlayers)
    {
        foreach (LootingPlayer lootingPlayer in lootingPlayers)
        {
            var lootedItems = lootingPlayer.GetLootedItemsSnapshot();
            if (lootedItems.Count > 0 && lootedItems.All(x => x.Visibility != Visibility.Visible))
            {
                lootingPlayer.LootingPlayerVisibility = Visibility.Collapsed;
            }
            else if (lootedItems.Count <= 0)
            {
                lootingPlayer.LootingPlayerVisibility = Visibility.Collapsed;
            }
            else
            {
                lootingPlayer.LootingPlayerVisibility = Visibility.Visible;
            }
        }
    }

    private bool Filter(LootedItem lootedItem, LootingPlayer lootingPlayer, bool isGuildFilterRestricted, IReadOnlySet<string> selectedGuilds)
    {
        return IsStatusOkay(lootedItem)
               && IsTierOkay(lootedItem)
               && IsTypeOkay(lootedItem)
               && IsGuildOkay(lootingPlayer, isGuildFilterRestricted, selectedGuilds);
    }

    private bool IsStatusOkay(LootedItem lootedItem)
    {
        if (lootedItem.IsTrash && !_isShowingTrash)
        {
            return false;
        }

        return lootedItem.Status switch
        {
            LootedItemStatus.Lost => _isShowingLost,
            LootedItemStatus.Resolved => _isShowingResolved,
            LootedItemStatus.Donated => _isShowingDonated,
            LootedItemStatus.Unknown => true,
            _ => true
        };
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
        var itemInformation = lootedItem.Item.FullItemInformation;
        if (itemInformation == null)
        {
            return _isShowingOthers;
        }

        var cat = itemInformation.ShopCategory;
        var sub1 = itemInformation.ShopSubCategory1;

        if (_isShowingFood && cat == "consumables" && sub1 is "food" or "tomes")
        {
            return true;
        }

        if (_isShowingPotion && cat == "consumables" && sub1 == "potions")
        {
            return true;
        }

        if (_isShowingMount && cat is "mounts" or "raremounts" or "battle_mount")
        {
            return true;
        }

        if (_isShowingWeapon && cat is "weapons" or "offhands")
        {
            return true;
        }

        if (_isShowingArmor && cat is "armors" or "head" or "shoes" or "gathering")
        {
            return true;
        }

        if (_isShowingOthers && cat is
                "other" or "artefacts" or "cityresources" or "furniture" or "vanity"
                or "materials" or "resources" or "labourers" or "crafting"
                or "token" or "trophies" or "farming" or "unknown")
        {
            return true;
        }

        if (_isShowingBag && cat == "accessoires" && sub1 == "bag")
        {
            return true;
        }

        if (_isShowingCape && cat == "accessoires" && sub1 == "cape")
        {
            return true;
        }

        return false;
    }

    private static bool IsGuildOkay(LootingPlayer lootingPlayer, bool isGuildFilterRestricted, IReadOnlySet<string> selectedGuilds)
    {
        if (!isGuildFilterRestricted)
        {
            return true;
        }

        return !string.IsNullOrWhiteSpace(lootingPlayer.PlayerGuild)
               && selectedGuilds.Contains(lootingPlayer.PlayerGuild);
    }

    #endregion
}
