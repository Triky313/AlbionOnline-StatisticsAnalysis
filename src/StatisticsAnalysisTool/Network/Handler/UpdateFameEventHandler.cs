using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.EventLogging;
using StatisticsAnalysisTool.EventLogging.Notification;
using StatisticsAnalysisTool.Localization;
using StatisticsAnalysisTool.Network.Events;
using StatisticsAnalysisTool.Network.Manager;
using StatisticsAnalysisTool.Network.Notification;
using System;
using System.Threading.Tasks;
using ValueType = StatisticsAnalysisTool.Enumerations.ValueType;

namespace StatisticsAnalysisTool.Network.Handler;

public class UpdateFameEventHandler : EventPacketHandler<UpdateFameEvent>
{
    private readonly LiveStatsTracker _liveStatsTracker;
    private readonly TrackingController _trackingController;

    public UpdateFameEventHandler(TrackingController trackingController) : base((int) EventCodes.UpdateFame)
    {
        _trackingController = trackingController;
        _liveStatsTracker = _trackingController?.LiveStatsTracker;
    }

    protected override async Task OnActionAsync(UpdateFameEvent value)
    {
        await _trackingController.AddNotificationAsync(SetPveFameNotification(value.TotalPlayerFame.DoubleValue, value.TotalGainedFame,
            value.ZoneFame.DoubleValue, value.PremiumFame, value.SatchelFame.DoubleValue, value.IsBonusFactorActive, value.BonusFactorInPercent));
        _liveStatsTracker.Add(ValueType.Fame, value.TotalGainedFame);
        _trackingController.DungeonController?.AddValueToDungeon(value.TotalGainedFame, ValueType.Fame);
        _trackingController.StatisticController?.AddValue(ValueType.Fame, value.TotalGainedFame);
    }

    private TrackingNotification SetPveFameNotification(double totalPlayerFame, double totalGainedFame, double zoneFame, double premiumFame,
        double satchelFame, bool isBonusFactorActive, double bonusFactorInPercent)
    {
        return new TrackingNotification(DateTime.Now, new FameNotificationFragment(LocalizationController.Translation("YOU_HAVE"), AttributeStatOperator.Plus, totalPlayerFame, totalGainedFame,
            LocalizationController.Translation("FAME"), PvpPveType.Pve, zoneFame, premiumFame, satchelFame, isBonusFactorActive, bonusFactorInPercent,
            LocalizationController.Translation("GAINED")), LoggingFilterType.Fame);
    }
}