using StatisticsAnalysisTool.Network.Events;
using StatisticsAnalysisTool.Network.Manager;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler;

public sealed class KilledPlayerEventHandler(TrackingController trackingController)
    : EventPacketHandler<KilledPlayerEvent>((int) EventCodes.KilledPlayer)
{
    protected override Task OnActionAsync(KilledPlayerEvent value)
    {
        var localUserObjectId = trackingController.EntityController.LocalUserData.UserObjectId;
        if (!localUserObjectId.HasValue
            || localUserObjectId.Value != value.KillerObjectId)
        {
            return Task.CompletedTask;
        }

        trackingController.StatisticController.AddPlayerKill(
            value.KilledPlayerName,
            value.KilledPlayerObjectId);
        return Task.CompletedTask;
    }
}