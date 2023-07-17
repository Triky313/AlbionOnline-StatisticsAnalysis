using StatisticsAnalysisTool.Network.Events;
using StatisticsAnalysisTool.Network.Manager;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler;

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
            await _trackingController.EntityController.SetCharacterEquipmentAsync((long) value.ObjectId, value.CharacterEquipment);
        }
    }
}