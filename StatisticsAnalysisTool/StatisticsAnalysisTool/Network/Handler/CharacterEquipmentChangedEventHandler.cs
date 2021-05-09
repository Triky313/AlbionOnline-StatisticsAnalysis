using Albion.Network;
using Newtonsoft.Json;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Network.Controller;
using StatisticsAnalysisTool.Network.Events;
using StatisticsAnalysisTool.Network.Notification;
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
            _trackingController.AddDebugNotification(HandlerType.Event, (int)EventCodes.CharacterEquipmentChanged, JsonConvert.SerializeObject(value));

            if (value.ObjectId != null)
            {
                _trackingController.EntityController.SetCharacterEquipment((long) value.ObjectId, value.CharacterEquipment);
            }
            await Task.CompletedTask;
        }
    }
}