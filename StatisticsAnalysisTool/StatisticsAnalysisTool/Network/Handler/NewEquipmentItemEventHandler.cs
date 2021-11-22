using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Models;
using StatisticsAnalysisTool.Network.Manager;
using StatisticsAnalysisTool.Network.Events;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler
{
    public class NewEquipmentItemEventHandler
    {
        private readonly TrackingController _trackingController;

        public NewEquipmentItemEventHandler(TrackingController trackingController)
        {
            _trackingController = trackingController;
        }

        public async Task OnActionAsync(NewEquipmentItemEvent value)
        {
            //_trackingController.LootController.AddDiscoveredLoot(value.Loot);

            _trackingController.EntityController.AddEquipmentItem(new EquipmentItem
            {
                ItemIndex = value.Loot.ItemId,
                SpellDictionary = value.Loot.SpellDictionary
            });
            await Task.CompletedTask;
        }
    }
}