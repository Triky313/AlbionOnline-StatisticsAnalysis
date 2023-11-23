using StatisticsAnalysisTool.Network.Events;
using StatisticsAnalysisTool.Network.Manager;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler;

public class CharacterEquipmentChangedEventHandler : EventPacketHandler<CharacterEquipmentChangedEvent>
{
    private readonly IEntityController _entityController;

    public CharacterEquipmentChangedEventHandler(IEntityController entityController) : base((int) EventCodes.CharacterEquipmentChanged)
    {
        _entityController = entityController;
    }

    protected override async Task OnActionAsync(CharacterEquipmentChangedEvent value)
    {
        if (value.ObjectId != null)
        {
            await _entityController.SetCharacterEquipmentAsync((long) value.ObjectId, value.CharacterEquipment);
        }
    }
}