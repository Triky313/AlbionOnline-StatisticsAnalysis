using StatisticsAnalysisTool.Network.Events;
using StatisticsAnalysisTool.Network.Manager;
using StatisticsAnalysisTool.StorageHistory;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler;

public class GuildVaultInfoEventHandler : EventPacketHandler<GuildVaultInfoEvent>
{
    private readonly TrackingController _trackingController;

    public GuildVaultInfoEventHandler(TrackingController trackingController) : base((int) EventCodes.RecoveryVaultPlayerInfo)
    {
        _trackingController = trackingController;
    }

    protected override async Task OnActionAsync(GuildVaultInfoEvent value)
    {
        if (_trackingController.IsTrackingAllowedByMainCharacter())
        {
            _trackingController.VaultController.SetCurrentGuildVault(new InternalVault(value.ObjectId, value.LocationGuidString, value.VaultGuidList, value.VaultNames, value.IconTags));
        }

        await Task.CompletedTask;
    }
}