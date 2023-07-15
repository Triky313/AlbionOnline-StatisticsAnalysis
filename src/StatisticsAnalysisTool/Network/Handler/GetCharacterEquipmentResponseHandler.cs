using StatisticsAnalysisTool.Network.Manager;
using StatisticsAnalysisTool.Network.Operations.Responses;
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
        await _trackingController.EntityController.SetItemPowerAsync(value.Guid, value.ItemPower);
    }
}