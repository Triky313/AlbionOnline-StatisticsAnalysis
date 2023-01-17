using StatisticsAnalysisTool.Network.Events;
using StatisticsAnalysisTool.Network.Manager;
using System.Threading.Tasks;
using StatisticsAnalysisTool.Models.NetworkModel;

namespace StatisticsAnalysisTool.Network.Handler;

public class BaseVaultInfoEventHandler
{
    private readonly TrackingController _trackingController;

    public BaseVaultInfoEventHandler(TrackingController trackingController)
    {
        _trackingController = trackingController;
    }

    public async Task OnActionAsync(BaseVaultInfoEvent value)
    {
        if (_trackingController.IsTrackingAllowedByMainCharacter())
        {
            _trackingController.VaultController.SetCurrentVault(new VaultInfo(value.ObjectId, value.LocationGuidString, value.VaultGuidList, value.VaultNames, value.IconTags));
        }

        await Task.CompletedTask;
    }
}