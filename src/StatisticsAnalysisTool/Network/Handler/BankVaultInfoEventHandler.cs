using StatisticsAnalysisTool.Network.Events;
using StatisticsAnalysisTool.Network.Manager;
using StatisticsAnalysisTool.StorageHistory;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler;

public class BankVaultInfoEventHandler : EventPacketHandler<BankVaultInfoEvent>
{
    private readonly TrackingController _trackingController;

    public BankVaultInfoEventHandler(TrackingController trackingController) : base((int) EventCodes.BankVaultInfo)
    {
        _trackingController = trackingController;
    }

    protected override async Task OnActionAsync(BankVaultInfoEvent value)
    {
        if (_trackingController.IsTrackingAllowedByMainCharacter())
        {
            _trackingController.VaultController.SetOrAddCurrentVault(new InternalVault(value.ObjectId, value.LocationGuidString, value.VaultGuidList, value.VaultNames, value.IconTags));
        }

        await Task.CompletedTask;
    }
}