using StatisticsAnalysisTool.EventLogging;
using StatisticsAnalysisTool.EventLogging.Notification;
using StatisticsAnalysisTool.Localization;
using StatisticsAnalysisTool.Models.NetworkModel;
using StatisticsAnalysisTool.Network.Events;
using StatisticsAnalysisTool.Network.Manager;
using System;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler;

public class DiedEventHandler : EventPacketHandler<DiedEvent>
{
    private readonly IGameEventWrapper _gameEventWrapper;

    public DiedEventHandler(IGameEventWrapper gameEventWrapper) : base((int) EventCodes.Died)
    {
        _gameEventWrapper = gameEventWrapper;
    }

    protected override async Task OnActionAsync(DiedEvent value)
    {
        _gameEventWrapper.DungeonController?.SetDiedIfInDungeon(new DiedObject(value.Died, value.KilledBy, value.KilledByGuild));
        await _gameEventWrapper.TrackingController.AddNotificationAsync(SetKillNotification(value.Died, value.KilledBy, value.KilledByGuild));
    }

    private static TrackingNotification SetKillNotification(string died, string killedBy, string killedByGuild)
    {
        return new TrackingNotification(DateTime.Now, new KillNotificationFragment(died, killedBy, killedByGuild, LanguageController.Translation("WAS_KILLED_BY")), LoggingFilterType.Kill);
    }
}