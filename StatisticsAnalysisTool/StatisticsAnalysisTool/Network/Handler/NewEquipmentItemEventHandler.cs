using Albion.Network;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Models;
using StatisticsAnalysisTool.Network.Controller;
using StatisticsAnalysisTool.Network.Events;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler
{
    public class NewEquipmentItemEventHandler : EventPacketHandler<NewEquipmentItemEvent>
    {
        private readonly TrackingController _trackingController;

        public NewEquipmentItemEventHandler(TrackingController trackingController) : base((int) EventCodes.NewEquipmentItem)
        {
            _trackingController = trackingController;
        }

        protected override async Task OnActionAsync(NewEquipmentItemEvent value)
        {
            _trackingController.EntityController.AddEquipmentItem(new EquipmentItem
            {
                ItemIndex = value.ItemIndex,
                SpellDictionary = value.SpellDictionary
            });
            await Task.CompletedTask;
        }
    }
}