using Serilog;
using StatisticsAnalysisTool.Cluster;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Diagnostics;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.EventLogging;
using StatisticsAnalysisTool.EventLogging.Notification;
using StatisticsAnalysisTool.Localization;
using StatisticsAnalysisTool.Models;
using StatisticsAnalysisTool.Models.NetworkModel;
using StatisticsAnalysisTool.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;

namespace StatisticsAnalysisTool.Network.Manager;

public class LootController : ILootController
{
    private readonly TrackingController _trackingController;
    private readonly MainWindowViewModel _mainWindowViewModel;
    private readonly List<LootLoggerObject> _lootLoggerObjects = [];
    private ItemContainerObject _currentItemContainer;
    private readonly List<DiscoveredItem> _discoveredLoot = [];
    private Loot _lastLootedItem;
    private Loot _lastComparedLootedItem;

    private const int MaxLoot = 5000;

    public LootController(TrackingController trackingController, MainWindowViewModel mainWindowViewModel)
    {
        _trackingController = trackingController;
        _mainWindowViewModel = mainWindowViewModel;

#if DEBUG
        _ = AddTestLootNotificationsAsync(20);
#endif
    }

    public void RegisterEvents()
    {
        OnAddLoot += AddTopLooter;
    }

    public void UnregisterEvents()
    {
        OnAddLoot -= AddTopLooter;
    }

    public event Action<string, int> OnAddLoot;

    #region Loot comparator

    public async Task AddLootedItemAsync(Loot loot)
    {
        if (loot == null || loot.IsSilver)
        {
            return;
        }

        if (_mainWindowViewModel.LoggingBindings.IsTrackingPartyLootOnly
            && !_trackingController.EntityController.IsEntityInParty(loot.LootedByName)
            && !_trackingController.EntityController.IsEntityInParty(loot.LootedFromName))
        {
            return;
        }

        if (!_mainWindowViewModel.LoggingBindings.IsTrackingMobLoot && loot.LootedFromName.ToUpper().Equals("MOB"))
        {
            return;
        }

        if (IsLastComparedLootedItem(loot))
        {
            return;
        }

        _lastComparedLootedItem = loot;

        var lootedByUser = _trackingController.EntityController.GetEntity(loot.LootedByName);
        var lootedFromUser = _trackingController.EntityController.GetEntity(loot.LootedFromName);

        await Application.Current.Dispatcher.InvokeAsync(() =>
        {
            var player =
                _mainWindowViewModel.LoggingBindings.LootingPlayers.FirstOrDefault(x =>
                    x.PlayerName == loot.LootedByName);
            if (player is not null)
            {
                UpdateLootingPlayerAffiliations(player, lootedByUser?.Value);
                player.AddLootedItem(new LootedItem()
                {
                    ItemIndex = loot.ItemIndex,
                    Quantity = loot.Quantity,
                    LootedByName = loot.LootedByName,
                    LootedFromName = loot.LootedFromName,
                    LootedFromGuild = lootedFromUser?.Value?.Guild,
                    IsTrash = loot.IsTrash
                });
            }
            else
            {
                _mainWindowViewModel.LoggingBindings.LootingPlayers.Add(new LootingPlayer()
                {
                    PlayerName = loot.LootedByName,
                    PlayerGuild = lootedByUser?.Value?.Guild,
                    PlayerAlliance = lootedByUser?.Value?.Alliance,
                    LootedItems = new ObservableCollection<LootedItem>()
                    {
                        new()
                        {
                            ItemIndex = loot.ItemIndex,
                            Quantity = loot.Quantity,
                            LootedByName = loot.LootedByName,
                            LootedFromName = loot.LootedFromName,
                            LootedFromGuild = lootedFromUser?.Value?.Guild,
                            IsTrash = loot.IsTrash
                        }
                    }
                });
            }

            _mainWindowViewModel.LoggingBindings.RefreshLootComparatorLogCounts();
        });
    }

    #endregion

    public async Task AddLootAsync(Loot loot)
    {
        if (loot == null || loot.IsSilver || loot.IsTrash)
        {
            return;
        }

        if (_mainWindowViewModel.LoggingBindings.IsTrackingPartyLootOnly
            && !_trackingController.EntityController.IsEntityInParty(loot.LootedByName)
            && !_trackingController.EntityController.IsEntityInParty(loot.LootedFromName))
        {
            return;
        }

        if (!_mainWindowViewModel.LoggingBindings.IsTrackingMobLoot && loot.LootedFromName.ToUpper().Equals("MOB"))
        {
            return;
        }

        if (IsLastLootedItem(loot))
        {
            return;
        }

        _lastLootedItem = loot;

        var item = ItemController.GetItemByIndex(loot.ItemIndex);
        var lootedByUser = _trackingController.EntityController.GetEntity(loot.LootedByName);
        var lootedFromUser = _trackingController.EntityController.GetEntity(loot.LootedFromName);
        var clusterName = ClusterController.GetCurrentClusterDisplayName();

        var notification = SetNotificationAsync(loot.LootedByName, loot.LootedFromName, lootedByUser?.Value?.Guild, lootedFromUser?.Value?.Guild, item, loot.Quantity);
        notification.SetClusterName(clusterName);
        await _trackingController.AddNotificationAsync(notification);

        _lootLoggerObjects.Add(new LootLoggerObject
        {
            LootedFromName = loot.LootedFromName,
            LootedFromGuild = lootedFromUser?.Value?.Guild,
            LootedFromAlliance = lootedFromUser?.Value?.Alliance,
            LootedByName = loot.LootedByName,
            LootedByGuild = lootedByUser?.Value?.Guild,
            LootedByAlliance = lootedByUser?.Value?.Alliance,
            Quantity = loot.Quantity,
            ItemId = item.Index,
            UniqueItemName = item.UniqueName,
            AverageEstMarketValue = item.AverageEstMarketValue,
            ClusterName = clusterName
        });

        _mainWindowViewModel.LoggingBindings.LootLoggerStats.RecordLoot(loot, item);

        OnAddLoot?.Invoke(loot.LootedByName, loot.Quantity);

        await RemoveLootIfMoreThanLimitAsync(MaxLoot);
    }

    private async Task RemoveLootIfMoreThanLimitAsync(int limit)
    {
        try
        {
            var numberOfItemsToBeDeleted = _lootLoggerObjects.Count - limit;
            if (numberOfItemsToBeDeleted <= 0)
            {
                return;
            }

            var itemsToBeRemoved = (from loot in _lootLoggerObjects orderby loot?.UtcPickupTime select loot).Take(numberOfItemsToBeDeleted);
            await foreach (var item in itemsToBeRemoved.ToAsyncEnumerable())
            {
                _lootLoggerObjects.Remove(item);
            }
        }
        catch (Exception e)
        {
            DebugConsole.WriteError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
            Log.Error(e, "{message}", MethodBase.GetCurrentMethod()?.DeclaringType);
        }
    }

    private bool IsLastLootedItem(Loot loot)
    {
        var lastItem = _lastLootedItem;

        if (_lastLootedItem == null)
        {
            return false;
        }

        double secondsDifference = Math.Abs((lastItem.UtcPickupTime - (loot?.UtcPickupTime ?? DateTime.MinValue)).TotalSeconds);
        var isSameTimeArea = secondsDifference <= 2;

        return lastItem.ItemIndex == loot?.ItemIndex
               && lastItem.Quantity == loot.Quantity
               && lastItem.LootedFromName == loot.LootedFromName
               && isSameTimeArea;
    }

    private bool IsLastComparedLootedItem(Loot loot)
    {
        var lastItem = _lastComparedLootedItem;

        if (_lastComparedLootedItem == null)
        {
            return false;
        }

        double secondsDifference = Math.Abs((lastItem.UtcPickupTime - (loot?.UtcPickupTime ?? DateTime.MinValue)).TotalSeconds);
        var isSameTimeArea = secondsDifference <= 2;

        return lastItem.ItemIndex == loot?.ItemIndex
               && lastItem.Quantity == loot.Quantity
               && lastItem.LootedFromName == loot.LootedFromName
               && isSameTimeArea;
    }

    public void ClearLootLogger()
    {
        _lootLoggerObjects.Clear();
        Application.Current.Dispatcher.Invoke(() =>
        {
            _mainWindowViewModel?.LoggingBindings?.TopLooters?.Clear();
            _mainWindowViewModel?.LoggingBindings?.LootLoggerStats?.Reset();
        });
    }

    public async Task AddKillDeathAsync(string died, string diedPlayerGuild, string killedBy, string killedByGuild, string clusterName)
    {
        _lootLoggerObjects.Add(new LootLoggerObject
        {
            Died = died,
            DiedPlayerGuild = diedPlayerGuild,
            KilledBy = killedBy,
            KilledByGuild = killedByGuild,
            ClusterName = clusterName
        });

        _mainWindowViewModel.LoggingBindings.LootLoggerStats.RecordKillDeath(died, killedBy);

        await RemoveLootIfMoreThanLimitAsync(MaxLoot);
    }

    public string GetLootLoggerObjectsAsCsv()
    {
        try
        {
            const string csvHeader = "timestamp_utc;looted_by__alliance;looted_by__guild;looted_by__name;item_id;item_name;quantity;looted_from__alliance;looted_from__guild;looted_from__name;died;died_player_guild;killed_by;killed_by_guild;average_est_market_value;cluster\n";
            return csvHeader + string.Join(Environment.NewLine, _lootLoggerObjects.Select(loot => loot.CsvOutput).ToArray());
        }
        catch (Exception e)
        {
            DebugConsole.WriteError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
            Log.Error(e, "{message}", MethodBase.GetCurrentMethod()?.DeclaringType);
            return string.Empty;
        }
    }

    public string GetLootLoggerObjectsAsJson()
    {
        try
        {
            var export = new
            {
                schema_version = 2,
                exported_at_utc = DateTime.UtcNow,
                entries = _lootLoggerObjects.Select(loot => loot.JsonOutput).ToArray()
            };

            return JsonSerializer.Serialize(export, new JsonSerializerOptions
            {
                WriteIndented = true
            });
        }
        catch (Exception e)
        {
            DebugConsole.WriteError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
            Log.Error(e, "{message}", MethodBase.GetCurrentMethod()?.DeclaringType);
            return string.Empty;
        }
    }

    private static TrackingNotification SetNotificationAsync(string lootedByName, string lootedFromName, string lootedByGuild, string lootedFromGuild, Item item, int quantity)
    {
        return new TrackingNotification(DateTime.Now,
            new OtherGrabbedLootNotificationFragment(lootedByName, lootedFromName, lootedByGuild, lootedFromGuild, item, quantity), item.Index);
    }

    #region Loot tracking

    private readonly ObservableCollection<IdentifiedBody> _identifiedBodies = new();

    public struct IdentifiedBody
    {
        public long ObjectId { get; set; }
        public string Name { get; set; }
    }

    public void SetIdentifiedBody(long objectId, string lootBody)
    {
        if (_identifiedBodies.Any(x => x.ObjectId == objectId))
        {
            return;
        }

        _identifiedBodies.Add(new IdentifiedBody()
        {
            ObjectId = objectId,
            Name = lootBody
        });
    }

    public void SetCurrentItemContainer(ItemContainerObject itemContainerObject)
    {
        _currentItemContainer = itemContainerObject;
    }

    public void AddDiscoveredItem(DiscoveredItem discoveredItem)
    {
        if (_discoveredLoot.Any(x => x?.ObjectId == discoveredItem?.ObjectId))
        {
            return;
        }

        _discoveredLoot.Add(discoveredItem);
    }

    public async Task AddNewLocalPlayerLootAsync(int containerSlot, Guid containerGuid, Guid userInteractGuid)
    {
        if (!TryGetLocalPlayerCurrentBodyLoot(containerGuid, userInteractGuid, out var identifiedBody))
        {
            return;
        }

        if (string.IsNullOrEmpty(_trackingController?.EntityController?.LocalUserData?.Username) || string.IsNullOrEmpty(identifiedBody.Name))
        {
            return;
        }

        var itemObjectId = GetItemObjectIdFromContainer(containerSlot);
        var lootedItem = _discoveredLoot.FirstOrDefault(x => x.ObjectId == itemObjectId);

        if (lootedItem == null)
        {
            return;
        }

        await AddLootAsync(new Loot()
        {
            IsSilver = false,
            IsTrash = false,
            ItemIndex = lootedItem.ItemIndex,
            LootedByName = _trackingController?.EntityController?.LocalUserData?.Username,
            LootedFromName = MobController.IsMob(identifiedBody.Name) ? LocalizationController.Translation("MOB") : identifiedBody.Name,
            Quantity = lootedItem.Quantity,
        });
    }

    public async Task AddNewLocalPlayerLootAsync(IReadOnlyCollection<long> itemObjectIds, Guid containerGuid, Guid userInteractGuid)
    {
        if (itemObjectIds?.Count <= 0 || !TryGetLocalPlayerCurrentBodyLoot(containerGuid, userInteractGuid, out var identifiedBody))
        {
            return;
        }

        if (string.IsNullOrEmpty(_trackingController?.EntityController?.LocalUserData?.Username) || string.IsNullOrEmpty(identifiedBody.Name))
        {
            return;
        }

        var currentContainerItemIds = GetCurrentContainerItemObjectIds();
        foreach (var itemObjectId in itemObjectIds.Distinct())
        {
            if (!currentContainerItemIds.Contains(itemObjectId))
            {
                continue;
            }

            var lootedItem = _discoveredLoot.FirstOrDefault(x => x.ObjectId == itemObjectId);
            if (lootedItem == null)
            {
                continue;
            }

            await AddLootAsync(new Loot()
            {
                IsSilver = false,
                IsTrash = false,
                ItemIndex = lootedItem.ItemIndex,
                LootedByName = _trackingController?.EntityController?.LocalUserData?.Username,
                LootedFromName = MobController.IsMob(identifiedBody.Name) ? LocalizationController.Translation("MOB") : identifiedBody.Name,
                Quantity = lootedItem.Quantity,
            });
        }
    }

    private bool TryGetLocalPlayerCurrentBodyLoot(Guid containerGuid, Guid userInteractGuid, out IdentifiedBody identifiedBody)
    {
        identifiedBody = default;

        if (_trackingController.EntityController.LocalUserData.InteractGuid != userInteractGuid)
        {
            return false;
        }

        identifiedBody = _identifiedBodies.FirstOrDefault(x => x.ObjectId == _currentItemContainer?.ObjectId);
        if (_currentItemContainer?.ContainerGuid != containerGuid || _currentItemContainer?.ObjectId != identifiedBody.ObjectId)
        {
            return false;
        }

        return true;
    }

    private HashSet<long> GetCurrentContainerItemObjectIds()
    {
        if (_currentItemContainer?.SlotItemIds?.Count is null or <= 0)
        {
            return [];
        }

        return _currentItemContainer.SlotItemIds.ToHashSet();
    }

    private long GetItemObjectIdFromContainer(int containerSlot)
    {
        if (_currentItemContainer == null || _currentItemContainer?.SlotItemIds?.Count is null or <= 0 || _currentItemContainer?.SlotItemIds?.Count <= containerSlot)
        {
            return 0;
        }

        return _currentItemContainer!.SlotItemIds![containerSlot];
    }

    public void ResetLocalPlayerDiscoveredLoot()
    {
        _discoveredLoot.Clear();
    }

    public void ResetIdentifiedBodies()
    {
        _identifiedBodies.Clear();
    }

    public Item GetItemFromDiscoveredLoot(long objectId)
    {
        var item = _discoveredLoot?.FirstOrDefault(x => x.ObjectId == objectId);
        return item?.ItemIndex > -1 ? ItemController.GetItemByIndex(item.ItemIndex) : null;
    }

    private static void UpdateLootingPlayerAffiliations(LootingPlayer lootingPlayer, PlayerGameObject playerGameObject)
    {
        if (lootingPlayer == null || playerGameObject == null)
        {
            return;
        }

        if (!string.IsNullOrWhiteSpace(playerGameObject.Guild))
        {
            lootingPlayer.PlayerGuild = playerGameObject.Guild;
        }

        if (!string.IsNullOrWhiteSpace(playerGameObject.Alliance))
        {
            lootingPlayer.PlayerAlliance = playerGameObject.Alliance;
        }
    }

    #endregion

    #region Top looters

    private void AddTopLooter(string name, int quantity)
    {
        var looter = _mainWindowViewModel?.LoggingBindings?.TopLooters?.ToList().FirstOrDefault(x => string.Equals(x?.PlayerName, name, StringComparison.CurrentCultureIgnoreCase));
        if (looter != null)
        {
            looter.Quantity += quantity;
            looter.LootActions++;
            return;
        }

        Application.Current.Dispatcher.Invoke(() =>
        {
            _mainWindowViewModel?.LoggingBindings?.TopLooters?.Add(new TopLooterObject(name, quantity, 1));
        });
    }

    #endregion

    #region Debug methods

    private static readonly Random Random = new(DateTime.Now.Millisecond);

    private async Task AddTestLootNotificationsAsync(int notificationCounter, int delay = 5000)
    {
        await Task.Delay(delay);
        var testPlayers = CreateTestLootPlayers();
        RegisterTestLootPlayers(testPlayers);

        for (var i = 0; i < notificationCounter; i++)
        {
            var randomItem = ItemController.GetItemByIndex(Random.Next(1, 7000));

            if (randomItem == null)
            {
                continue;
            }

            var lootedByPlayer = GetRandomTestLootPlayer(testPlayers);
            var lootedFromPlayer = GetRandomTestLootPlayer(testPlayers);
            await AddLootAsync(new Loot()
            {
                LootedFromName = lootedFromPlayer.Name,
                IsTrash = ItemController.IsTrash(randomItem.Index),
                ItemIndex = randomItem.Index,
                LootedByName = lootedByPlayer.Name,
                IsSilver = false,
                Quantity = Random.Next(1, 250)
            });

            lootedByPlayer = GetRandomTestLootPlayer(testPlayers);
            lootedFromPlayer = GetRandomTestLootPlayer(testPlayers);
            await AddLootedItemAsync(new Loot()
            {
                LootedFromName = lootedFromPlayer.Name,
                IsTrash = ItemController.IsTrash(randomItem.Index),
                ItemIndex = randomItem.Index,
                LootedByName = lootedByPlayer.Name,
                IsSilver = false,
                Quantity = Random.Next(1, 250)
            });
            await Task.Delay(100);
        }
    }

    private static IReadOnlyList<TestLootPlayer> CreateTestLootPlayers()
    {
        return
        [
            new TestLootPlayer("DebugLooterOne", "Crimson Market", "CM"),
            new TestLootPlayer("DebugLooterTwo", "Crimson Market", "CM"),
            new TestLootPlayer("DebugLooterThree", "Azure Vault", "AV"),
            new TestLootPlayer("DebugLooterFour", string.Empty, string.Empty),
            new TestLootPlayer("DebugLooterFive", "Iron Ledger", string.Empty),
            new TestLootPlayer("DebugLooterSix", string.Empty, string.Empty)
        ];
    }

    private void RegisterTestLootPlayers(IReadOnlyList<TestLootPlayer> testPlayers)
    {
        for (var i = 0; i < testPlayers.Count; i++)
        {
            var testPlayer = testPlayers[i];
            _trackingController.EntityController.AddEntity(new Entity
            {
                ObjectId = 900000 + i,
                UserGuid = Guid.NewGuid(),
                InteractGuid = Guid.NewGuid(),
                Name = testPlayer.Name,
                Guild = testPlayer.Guild,
                Alliance = testPlayer.Alliance,
                ObjectType = GameObjectType.Player,
                ObjectSubType = GameObjectSubType.Player
            });
        }
    }

    private static TestLootPlayer GetRandomTestLootPlayer(IReadOnlyList<TestLootPlayer> testPlayers)
    {
        return testPlayers[Random.Next(testPlayers.Count)];
    }

    private sealed class TestLootPlayer
    {
        public TestLootPlayer(string name, string guild, string alliance)
        {
            Name = name;
            Guild = guild;
            Alliance = alliance;
        }

        public string Name { get; }
        public string Guild { get; }
        public string Alliance { get; }
    }

    #endregion
}
