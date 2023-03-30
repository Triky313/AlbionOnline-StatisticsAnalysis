using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.EventLogging;
using StatisticsAnalysisTool.EventLogging.Notification;
using StatisticsAnalysisTool.Models.NetworkModel;
using StatisticsAnalysisTool.Network.Events;
using StatisticsAnalysisTool.Network.Manager;
using System;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler;

public class DiedEventHandler : EventPacketHandler<DiedEvent>
{
    private readonly TrackingController _trackingController;

    public DiedEventHandler(TrackingController trackingController) : base((int) EventCodes.Died)
    {
        _trackingController = trackingController;
    }

    protected override async Task OnActionAsync(DiedEvent value)
    {
        _trackingController.DungeonController?.SetDiedIfInDungeon(new DiedObject(value.Died, value.KilledBy, value.KilledByGuild));
        await _trackingController.AddNotificationAsync(SetKillNotification(value.Died, value.KilledBy, value.KilledByGuild));
    }

    private static TrackingNotification SetKillNotification(string died, string killedBy, string killedByGuild)
    {
        return new TrackingNotification(DateTime.Now, new KillNotificationFragment(died, killedBy, killedByGuild, LanguageController.Translation("WAS_KILLED_BY")), NotificationType.Kill);
    }
}