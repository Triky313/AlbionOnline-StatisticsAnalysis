using log4net;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Models;
using StatisticsAnalysisTool.Models.NetworkModel;
using StatisticsAnalysisTool.Network.Notification;
using StatisticsAnalysisTool.Properties;
using StatisticsAnalysisTool.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;

namespace StatisticsAnalysisTool.Network.Manager
{
    public class LootController : ILootController
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);
        private readonly TrackingController _trackingController;
        private readonly MainWindowViewModel _mainWindowViewModel;
        private readonly List<LootLoggerObject> _lootLoggerObjects = new();
        private ObservableCollection<EstimatedMarketValueObject> _estimatedMarketValues = new();
        private ItemContainerObject _currentItemContainer;
        private readonly List<DiscoveredItem> _discoveredLoot = new();

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

                var itemsToBeRemoved = (from loot in _lootLoggerObjects orderby loot.UtcPickupTime select loot).Take(numberOfItemsToBeDeleted);
                await foreach (var item in itemsToBeRemoved.ToAsyncEnumerable())
                {
                    _lootLoggerObjects.Remove(item);
                }
            }
            catch (Exception e)
            {
                ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
                Log.Error(MethodBase.GetCurrentMethod()?.DeclaringType, e);
            }
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
                Log.Error(MethodBase.GetCurrentMethod()?.DeclaringType, e);
                return string.Empty;
            }
        }

        private static TrackingNotification SetNotificationAsync(string lootedByName, string lootedFromName, string lootedByGuild, string lootedFromGuild, Item item, int quantity)
        {
            return new TrackingNotification(DateTime.Now,
                new OtherGrabbedLootNotificationFragment(lootedByName, lootedFromName, lootedByGuild, lootedFromGuild, item, quantity), item.Index);
        }

        #region Lokal player loot tracking

        private IdentifiedBody _currentIdentifiedBody;

        public struct IdentifiedBody
        {
            public long ObjectId { get; set; }
            public string Name { get; set; }
        }

        public void SetIdentifiedBody(long objectId, string lootBody)
        {
            _currentIdentifiedBody = new IdentifiedBody()
            {
                ObjectId = objectId,
                Name = lootBody
            };
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

            if (_currentItemContainer?.ContainerGuid != containerGuid || _currentItemContainer?.ObjectId != _currentIdentifiedBody.ObjectId)
            {
                return;
            }

            if (string.IsNullOrEmpty(_trackingController?.EntityController?.LocalUserData?.Username) || string.IsNullOrEmpty(_currentIdentifiedBody.Name))
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
                LootedFromName = MobController.IsMob(_currentIdentifiedBody.Name) ? LanguageController.Translation("MOB") : _currentIdentifiedBody.Name,
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

        #endregion

        #region Estimated market value

        public void AddEstimatedMarketValue(int itemId, long estimatedMarketValueInternal)
        {
            if (itemId <= 0 || estimatedMarketValueInternal <= 0)
            {
                return;
            }

            var item = ItemController.GetItemByIndex(itemId);

            if (item == null)
            {
                return;
            }

            var estMarketValueObject = _estimatedMarketValues?.FirstOrDefault(x => x.UniqueItemName == item.UniqueName);

            if (estMarketValueObject != null)
            {
                _estimatedMarketValues.Remove(estMarketValueObject);
            }

            var timestamp = DateTime.UtcNow;
            _estimatedMarketValues?.Add(new EstimatedMarketValueObject()
            {
                UniqueItemName = item.UniqueName,
                EstimatedMarketValueInternal = estimatedMarketValueInternal,
                Timestamp = timestamp
            });

            ItemController.SetEstimatedMarketValue(item.UniqueName, estimatedMarketValueInternal, timestamp);
        }

        private async Task SetAllEstimatedMarketValuesToItemsAsync()
        {
            await foreach (var estMarketValue in _estimatedMarketValues.ToAsyncEnumerable())
            {
                ItemController.SetEstimatedMarketValue(estMarketValue.UniqueItemName, estMarketValue.EstimatedMarketValueInternal, estMarketValue.Timestamp);
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

        #region Load / Save local file data

        public async Task LoadFromFileAsync()
        {
            _estimatedMarketValues = await FileController.LoadAsync<ObservableCollection<EstimatedMarketValueObject>>($"{AppDomain.CurrentDomain.BaseDirectory}{Settings.Default.EstimatedMarketValueFileName}");
            await SetAllEstimatedMarketValuesToItemsAsync();
        }

        public async Task SaveInFileAsync()
        {
            await FileController.SaveAsync(_estimatedMarketValues, $"{AppDomain.CurrentDomain.BaseDirectory}{Settings.Default.EstimatedMarketValueFileName}");
        }

        #endregion
    }
}