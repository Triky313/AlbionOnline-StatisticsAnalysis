using StatisticsAnalysisTool.Network.Manager;
using StatisticsAnalysisTool.Network.Operations.Responses;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler;

public class GetCharacterEquipmentResponseHandler : ResponsePacketHandler<GetCharacterEquipmentResponse>
{
    private readonly IGameEventWrapper _gameEventWrapper;

    public GetCharacterEquipmentResponseHandler(IGameEventWrapper gameEventWrapper) : base((int) OperationCodes.GetCharacterEquipment)
    {
        _gameEventWrapper = gameEventWrapper;
    }

    protected override async Task OnActionAsync(GetCharacterEquipmentResponse value)
    {
        _gameEventWrapper.EntityController.SetItemPower(value.Guid, value.ItemPower);
        _gameEventWrapper.PartyBuilderController.UpdateInspectedPlayer(value.Guid, value.CharacterEquipment, value.ItemPower);
        await Task.CompletedTask;
    }
}