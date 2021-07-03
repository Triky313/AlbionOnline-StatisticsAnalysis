using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Models.NetworkModel;
using StatisticsAnalysisTool.ViewModels;
using StatisticsAnalysisTool.Views;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace StatisticsAnalysisTool.Network.Controller
{
    public class LootController : ILootController
    {
        private readonly MainWindow _mainWindow;
        private readonly MainWindowViewModel _mainWindowViewModel;
        private readonly TrackingController _trackingController;

        private readonly Dictionary<long, Guid> _putLoot = new Dictionary<long, Guid>();
        private readonly List<DiscoveredLoot> _discoveredLoot = new List<DiscoveredLoot>();
        private readonly List<Loot> LootedItems = new List<Loot>();

        public LootController(TrackingController trackingController, MainWindow mainWindow, MainWindowViewModel mainWindowViewModel)
        {
            _trackingController = trackingController;
            _mainWindow = mainWindow;
            _mainWindowViewModel = mainWindowViewModel;
        }

        public void AddDiscoveredLoot(DiscoveredLoot loot)
        {
            _discoveredLoot.Add(loot);
        }

        public void AddPutLoot(long? objectId, Guid? playerGuid)
        {
            if (objectId != null && playerGuid != null)
            {
                _putLoot.Add((long) objectId, (Guid) playerGuid);
            }

            LootMerge();
        }

        public void ResetViewedLootLists()
        {
            _putLoot.Clear();
            _discoveredLoot.Clear();
        }

        private void LootMerge()
        {
            foreach (var lootedObject in _putLoot)
            {
                if (!_discoveredLoot.Exists(x => x.ItemId == lootedObject.Key))
                {
                    continue;
                }

                var discoveredLoot = _discoveredLoot.FirstOrDefault(x => x.ItemId == lootedObject.Key);
                if (discoveredLoot != null)
                {

                    var loot = new Loot()
                    {
                        BodyName = discoveredLoot.BodyName,
                        IsTrash = ItemController.IsTrash(discoveredLoot.ItemId),
                        Item = ItemController.GetItemByIndex(discoveredLoot.ItemId),
                        ItemId = discoveredLoot.ItemId,
                        LooterName = discoveredLoot.LooterName,
                        ObjectId = discoveredLoot.ObjectId,
                        Quantity = discoveredLoot.Quantity
                    };

                    LootedItems.Add(loot);

                    Debug.Print($"{loot.Item.LocalizedName} | Quantity: {loot.Quantity} | ObjId: {loot.ObjectId}");

                    _discoveredLoot.Remove(discoveredLoot);
                }
                
            }
        }
    }
}