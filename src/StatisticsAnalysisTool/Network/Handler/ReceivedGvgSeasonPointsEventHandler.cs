using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.EventLogging;
using StatisticsAnalysisTool.EventLogging.Notification;
using StatisticsAnalysisTool.Localization;
using StatisticsAnalysisTool.Network.Events;
using StatisticsAnalysisTool.Network.Manager;
using System;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler;

public class ReceivedGvgSeasonPointsEventHandler : EventPacketHandler<ReceivedGvgSeasonPointsEvent>
{
    private readonly TrackingController _trackingController;

    public ReceivedGvgSeasonPointsEventHandler(TrackingController trackingController) : base((int) EventCodes.ReceivedGvgSeasonPoints)
    {
        _trackingController = trackingController;
    }

    protected override async Task OnActionAsync(ReceivedGvgSeasonPointsEvent value)
    {
        await Task.CompletedTask;
    }

    private TrackingNotification SetSeasonPointsNotification(int seasonPoints)
    {
        return new TrackingNotification(DateTime.Now, new SeasonPointsNotificationFragment(LocalizationController.Translation("YOU_HAVE"), AttributeStatOperator.Plus, seasonPoints,
            LocalizationController.Translation("SEASON_POINTS"), LocalizationController.Translation("GAINED")), LoggingFilterType.SeasonPoints);
    }
}