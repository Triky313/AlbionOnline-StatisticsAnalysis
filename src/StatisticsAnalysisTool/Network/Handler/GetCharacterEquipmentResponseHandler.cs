using StatisticsAnalysisTool.Models.NetworkModel;
using StatisticsAnalysisTool.Network.Manager;
using StatisticsAnalysisTool.Network.Operations.Responses;
using System;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler;

public class GetCharacterEquipmentResponseHandler : ResponsePacketHandler<GetCharacterEquipmentResponse>
{
    private readonly TrackingController _trackingController;

    public GetCharacterEquipmentResponseHandler(TrackingController trackingController) : base((int) OperationCodes.GetCharacterEquipment)
    {
        _trackingController = trackingController;
    }

    protected override async Task OnActionAsync(GetCharacterEquipmentResponse value)
    {
        _trackingController.EntityController.SetItemPower(value.Guid, value.ItemPower);
        _trackingController.PartyController.UpdateInspectedPlayer(value.Guid, value.CharacterEquipment, value.ItemPower);
        await Task.CompletedTask;
    }
}