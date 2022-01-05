using log4net;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Models;
using StatisticsAnalysisTool.Models.NetworkModel;
using StatisticsAnalysisTool.Network.Notification;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Manager
{
    public class LootController : ILootController
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);
        private readonly TrackingController _trackingController;

        private readonly Dictionary<long, Guid> _putLoot = new();
        private readonly List<DiscoveredLoot> _discoveredLoot = new();
        private readonly List<LootLoggerObject> _lootLoggerObjects = new();

        private const int _maxLoot = 5000;

        public bool IsPartyLootOnly;

        public LootController(TrackingController trackingController)
        {
            _trackingController = trackingController;

#if DEBUG
            _ = AddTestLootNotificationsAsync(30);
#endif
        }

        public async Task AddLootAsync(Loot loot)
        {
            if (IsPartyLootOnly && !_trackingController.EntityController.IsEntityInParty(loot.LooterName) && !_trackingController.EntityController.IsEntityInParty(loot.LootedBody))
            {
                return;
            }

            if (loot == null || loot.IsSilver || loot.IsTrash)
            {
                return;
            }

            var item = ItemController.GetItemByIndex(loot.ItemIndex);

            await _trackingController.AddNotificationAsync(SetNotificationAsync(loot.LooterName, loot.LootedBody, item, loot.Quantity));

            _lootLoggerObjects.Add(new LootLoggerObject()
            {
                BodyName = loot.LootedBody,
                LooterName = loot.LooterName,
                Quantity = loot.Quantity,
                UniqueName = item.UniqueName
            });

            await RemoveLootIfMoreThanLimitAsync(_maxLoot);
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
        }

        public void AddDiscoveredLoot(DiscoveredLoot loot)
        {
            if (_discoveredLoot.Exists(x => x.ObjectId == loot.ObjectId))
            {
                return;
            }

            _discoveredLoot.Add(loot);
        }
        
        public async Task AddPutLootAsync(long? objectId, Guid? interactGuid)
        {
            if (_trackingController.EntityController.GetLocalEntity()?.Value?.InteractGuid != interactGuid)
            {
                return;
            }

            if (objectId != null && interactGuid != null && !_putLoot.ContainsKey((long)objectId))
            {
                _putLoot.Add((long) objectId, (Guid)interactGuid);
            }

            await LootMergeAsync();
        }

        public void ResetViewedLootLists()
        {
            _putLoot.Clear();
            _discoveredLoot.Clear();
        }

        private async Task LootMergeAsync()
        {
            foreach (var lootedObject in _putLoot)
            {
                if (!_discoveredLoot.Exists(x => x.ObjectId == lootedObject.Key))
                {
                    continue;
                }

                var discoveredLoot = _discoveredLoot.FirstOrDefault(x => x.ObjectId == lootedObject.Key);
                if (discoveredLoot != null)
                {

                    var loot = new Loot()
                    {
                        LootedBody = discoveredLoot.BodyName,
                        IsTrash = ItemController.IsTrash(discoveredLoot.ItemId),
                        ItemIndex = discoveredLoot.ItemId,
                        LooterName = discoveredLoot.LooterName,
                        Quantity = discoveredLoot.Quantity
                    };

                    await AddLootAsync(loot);
                    _discoveredLoot.Remove(discoveredLoot);
                }
            }
        }

        public string GetLootLoggerObjectsAsCsv()
        {
            try
            {
                return string.Join(Environment.NewLine, _lootLoggerObjects.Select(loot => loot.CsvOutput).ToArray());
            }
            catch (Exception e)
            {
                ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
                Log.Error(MethodBase.GetCurrentMethod()?.DeclaringType, e);
                return string.Empty;
            }
        }

        private static TrackingNotification SetNotificationAsync(string looter, string lootedPlayer, Item item, int quantity)
        {
            return new TrackingNotification(DateTime.Now, new OtherGrabbedLootNotificationFragment(looter, lootedPlayer, item, quantity), item.Index);
        }
        
        #region Debug methods

        private static readonly Random _random = new(DateTime.Now.Millisecond);

        private async Task AddTestLootNotificationsAsync(int notificationCounter)
        {
            for (var i = 0; i < notificationCounter; i++)
            {
                var randomItem = ItemController.GetItemByIndex(_random.Next(1, 7000));

                if (randomItem == null)
                {
                    continue;
                }

                await AddLootAsync(new Loot()
                {
                    LootedBody = TestMethods.GenerateName(6),
                    IsTrash = ItemController.IsTrash(randomItem.Index),
                    ItemIndex = randomItem.Index,
                    LooterName = TestMethods.GenerateName(8),
                    IsSilver = false,
                    Quantity = 1
                });
                await Task.Delay(100);
            }
        }

        #endregion
    }
}