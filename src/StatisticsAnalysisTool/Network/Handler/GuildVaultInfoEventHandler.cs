using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Network.Events;
using StatisticsAnalysisTool.Network.Manager;
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
        await Task.CompletedTask;
    }
}