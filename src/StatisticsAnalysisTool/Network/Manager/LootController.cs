using Serilog;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.EventLogging;
using StatisticsAnalysisTool.EventLogging.Notification;
using StatisticsAnalysisTool.Models;
using StatisticsAnalysisTool.Models.NetworkModel;
using StatisticsAnalysisTool.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using Loot = StatisticsAnalysisTool.Models.NetworkModel.Loot;

namespace StatisticsAnalysisTool.Network.Manager;

public class LootController : ILootController
{
    private readonly TrackingController _trackingController;
    private readonly MainWindowViewModel _mainWindowViewModel;
    private readonly List<LootLoggerObject> _lootLoggerObjects = new();
    private ItemContainerObject _currentItemContainer;
    private readonly List<DiscoveredItem> _discoveredLoot = new();
    private Loot _lastLootedItem;

    private const int MaxLoot = 5000;

    public bool IsPartyLootOnly;

    public LootController(TrackingController trackingController, MainWindowViewModel mainWindowViewModel)
    {
        _trackingController = trackingController;
        _mainWindowViewModel = mainWindowViewModel;

#if DEBUG
        _ = AddTestLootNotificationsAsync(30);
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

    public async Task AddLootAsync(Loot loot)
    {
        if (loot == null || loot.IsSilver || loot.IsTrash)
        {
            return;
        }

        if (IsPartyLootOnly && !_trackingController.EntityController.IsEntityInParty(loot.LootedByName) && !_trackingController.EntityController.IsEntityInParty(loot.LootedFromName))
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

        var notification = SetNotificationAsync(loot.LootedByName, loot.LootedFromName, lootedByUser?.Value?.Guild, lootedFromUser?.Value?.Guild, item, loot.Quantity);
        await _trackingController.AddNotificationAsync(notification);

        _lootLoggerObjects.Add(new LootLoggerObject
        {
            LootedFromName = loot.LootedFromName,
            LootedFromGuild = lootedFromUser?.Value?.Guild,
            LootedByName = loot.LootedByName,
            LootedByGuild = lootedByUser?.Value?.Guild,
            Quantity = loot.Quantity,
            ItemId = item.Index,
            UniqueItemName = item.UniqueName,
        });

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
            ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
            Log.Error(e, "{message}", MethodBase.GetCurrentMethod()?.DeclaringType);
        }
    }

    private bool IsLastLootedItem(Loot loot)
    {
        var lastItem = _lastLootedItem;
        return lastItem?.ItemIndex == loot?.ItemIndex
               && lastItem?.Quantity == loot?.Quantity
               && lastItem?.LootedByName == loot?.LootedByName
               && lastItem?.LootedFromName == loot?.LootedFromName;
    }

    public void ClearLootLogger()
    {
        _lootLoggerObjects.Clear();
        Application.Current.Dispatcher.Invoke(() =>
        {
            _mainWindowViewModel?.LoggingBindings?.TopLooters?.Clear();
        });
    }

    public string GetLootLoggerObjectsAsCsv()
    {
        try
        {
            const string csvHeader = "timestamp_utc;looted_by__alliance;looted_by__guild;looted_by__name;item_id;item_name;quantity;looted_from__alliance;looted_from__guild;looted_from__name\n";
            return csvHeader + string.Join(Environment.NewLine, _lootLoggerObjects.Select(loot => loot.CsvOutput).ToArray());
        }
        catch (Exception e)
        {
            ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
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
        if (_trackingController.EntityController.LocalUserData.InteractGuid != userInteractGuid)
        {
            return;
        }

        var identifiedBody = _identifiedBodies.FirstOrDefault(x => x.ObjectId == _currentItemContainer?.ObjectId);
        if (_currentItemContainer?.ContainerGuid != containerGuid || _currentItemContainer?.ObjectId != identifiedBody.ObjectId)
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
            LootedFromName = MobController.IsMob(identifiedBody.Name) ? LanguageController.Translation("MOB") : identifiedBody.Name,
            Quantity = lootedItem.Quantity
        });
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
            _mainWindowViewModel?.LoggingBindings?.TopLootersCollectionView?.Refresh();
        });
    }

    #endregion

    #region Debug methods

    private static readonly Random Random = new(DateTime.Now.Millisecond);

    private async Task AddTestLootNotificationsAsync(int notificationCounter, int delay = 10000)
    {
        await Task.Delay(delay);
        for (var i = 0; i < notificationCounter; i++)
        {
            var randomItem = ItemController.GetItemByIndex(Random.Next(1, 7000));

            if (randomItem == null)
            {
                continue;
            }

            await AddLootAsync(new Loot()
            {
                LootedFromName = TestMethods.GenerateName(4),
                IsTrash = ItemController.IsTrash(randomItem.Index),
                ItemIndex = randomItem.Index,
                LootedByName = TestMethods.GenerateName(3),
                IsSilver = false,
                Quantity = Random.Next(1, 250)
            });
            await Task.Delay(100);
        }
    }

    #endregion
}