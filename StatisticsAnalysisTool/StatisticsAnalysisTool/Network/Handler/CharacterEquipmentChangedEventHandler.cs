using StatisticsAnalysisTool.Network.Events;
using StatisticsAnalysisTool.Network.Manager;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler
{
    public class CharacterEquipmentChangedEventHandler
    {
        private readonly TrackingController _trackingController;

        public CharacterEquipmentChangedEventHandler(TrackingController trackingController)
        {
            _trackingController = trackingController;
        }

        public async Task OnActionAsync(CharacterEquipmentChangedEvent value)
        {
            if (value.ObjectId != null)
            {
                _trackingController.EntityController.SetCharacterEquipment((long)value.ObjectId, value.CharacterEquipment);
            }
            await Task.CompletedTask;
        }
    }
}