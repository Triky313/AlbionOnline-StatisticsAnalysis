using StatisticsAnalysisTool.Models;
using StatisticsAnalysisTool.Network.Events;
using StatisticsAnalysisTool.Network.Manager;
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
            if (_trackingController.IsTrackingAllowedByMainCharacter())
            {
                _trackingController.VaultController.Add(value.Item);
            }

            _trackingController.EntityController.AddEquipmentItem(new EquipmentItemInternal
            {
                ItemIndex = value.Item.ItemIndex,
                SpellDictionary = value.Item.SpellDictionary
            });
            await Task.CompletedTask;
        }
    }
}