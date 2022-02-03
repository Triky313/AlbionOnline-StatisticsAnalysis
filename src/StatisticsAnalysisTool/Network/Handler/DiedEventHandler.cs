using System;
using StatisticsAnalysisTool.Models.NetworkModel;
using StatisticsAnalysisTool.Network.Manager;
using StatisticsAnalysisTool.Network.Events;
using System.Threading.Tasks;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Network.Notification;

namespace StatisticsAnalysisTool.Network.Handler
{
    public class DiedEventHandler
    {
        private readonly TrackingController _trackingController;

        public DiedEventHandler(TrackingController trackingController)
        {
            _trackingController = trackingController;

            //_trackingController.AddNotificationAsync(SetKillNotification("Dicky", "Aaron"));
        }

        public async Task OnActionAsync(DiedEvent value)
        {
            _trackingController.DungeonController?.SetDiedIfInDungeon(new DiedObject(value.Died, value.KilledBy, value.KilledByGuild));
            await _trackingController.AddNotificationAsync(SetKillNotification(value.Died, value.KilledBy));
        }

        private static TrackingNotification SetKillNotification(string died, string killedBy)
        {
            return new TrackingNotification(DateTime.Now, new KillNotificationFragment(died, killedBy, LanguageController.Translation("WAS_KILLED_BY")), NotificationType.Kill);
        }
    }
}