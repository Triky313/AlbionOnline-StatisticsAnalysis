using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Models;
using StatisticsAnalysisTool.Models.NetworkModel;
using StatisticsAnalysisTool.Network.Notification;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Manager
{
    public class LootController : ILootController
    {
        private readonly TrackingController _trackingController;

        private readonly Dictionary<long, Guid> _putLoot = new ();
        private readonly List<DiscoveredLoot> _discoveredLoot = new ();

        public LootController(TrackingController trackingController)
        {
            _trackingController = trackingController;
        }

        public async Task AddLootAsync(Loot loot)
        {
            if (loot == null || loot.IsSilver || loot.IsTrash)
            {
                return;
            }

            await _trackingController.AddNotificationAsync(SetNotification(loot.LooterName, loot.LootedBody, loot.Item, loot.Quantity));
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
                        Item = ItemController.GetItemByIndex(discoveredLoot.ItemId),
                        ItemId = discoveredLoot.ItemId,
                        LooterName = discoveredLoot.LooterName,
                        Quantity = discoveredLoot.Quantity
                    };

                    await AddLootAsync(loot);
                    _discoveredLoot.Remove(discoveredLoot);
                }
                
            }
        }

        private TrackingNotification SetNotification(string looter, string lootedPlayer, Item item, int quantity)
        {
            var itemType = ItemController.GetItemType(item.Index);

            return new TrackingNotification(DateTime.Now, new List<LineFragment>
            {
                new OtherGrabbedLootNotificationFragment(looter, lootedPlayer, item, quantity)
            }, GetNotificationType(itemType));
        }

        private NotificationType GetNotificationType(ItemType itemType)
        {
            switch (itemType)
            {
                case ItemType.Weapon:
                    return NotificationType.EquipmentLoot;
                case ItemType.Consumable:
                    return NotificationType.ConsumableLoot;
                case ItemType.Simple:
                    return NotificationType.SimpleLoot;
                default:
                    return NotificationType.UnknownLoot;
            }
        }
    }
}