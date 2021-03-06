using Albion.Network;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Network.Controller;
using StatisticsAnalysisTool.Network.Events;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler
{
    public class CharacterEquipmentChangedEventHandler : EventPacketHandler<CharacterEquipmentChangedEvent>
    {
        private readonly TrackingController _trackingController;
        public CharacterEquipmentChangedEventHandler(TrackingController trackingController) : base((int) EventCodes.CharacterEquipmentChanged)
        {
            _trackingController = trackingController;
        }

        protected override async Task OnActionAsync(CharacterEquipmentChangedEvent value)
        {
            if (value.ObjectId != null)
            {
                _trackingController.EntityController.SetCharacterEquipment((long)value.ObjectId, value.CharacterEquipment);
            }
            await Task.CompletedTask;
        }
    }
}