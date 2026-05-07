using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Models.NetworkModel;
using StatisticsAnalysisTool.Network.Events;
using StatisticsAnalysisTool.Network.Manager;
using System;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler;

public class NewCharacterEventHandler(TrackingController trackingController) : EventPacketHandler<NewCharacterEvent>((int) EventCodes.NewCharacter)
{
    protected override async Task OnActionAsync(NewCharacterEvent value)
    {


        if (value.Guid != null && value.ObjectId != null)
        {
            trackingController.EntityController.AddEntity(new Entity
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