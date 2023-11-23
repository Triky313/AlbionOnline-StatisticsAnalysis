using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Network.Events;
using StatisticsAnalysisTool.Network.Manager;
using System;
using System.Threading.Tasks;
using StatisticsAnalysisTool.Models.NetworkModel;

namespace StatisticsAnalysisTool.Network.Handler;

public class NewCharacterEventHandler : EventPacketHandler<NewCharacterEvent>
{
    private readonly IEntityController _entityController;

    public NewCharacterEventHandler(IEntityController entityController) : base((int) EventCodes.NewCharacter)
    {
        _entityController = entityController;
    }

    protected override async Task OnActionAsync(NewCharacterEvent value)
    {
        if (value.Guid != null && value.ObjectId != null)
        {
            _trackingController.EntityController.AddEntity(new Entity
            {
                ObjectId = value.ObjectId,
                UserGuid = value.Guid ?? Guid.Empty,
                Name = value.Name,
                Guild = value.GuildName,
                CharacterEquipment = value.CharacterEquipment,
                ObjectType = GameObjectType.Player,
                ObjectSubType = GameObjectSubType.Player
            });
        }

        await Task.CompletedTask;
    }
}