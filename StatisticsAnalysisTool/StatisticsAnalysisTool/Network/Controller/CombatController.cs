using log4net;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Network.Notification;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace StatisticsAnalysisTool.Network.Controller
{
    public class CombatController
    {
        private readonly TrackingController _trackingController;
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public CombatController(TrackingController trackingController)
        {
            _trackingController = trackingController;
        }

        public void AddDamage(long? causerId, long? targetId, double damage)
        {
            if (causerId == null || (_trackingController.UserObjectId != causerId && _trackingController.UserObjectId != targetId))
            {
                return;
            }

            if (_trackingController.UserObjectId == causerId)
            {
                var causerName = (string.IsNullOrEmpty(_trackingController.Username)) ? LanguageController.Translation("UNKNOWN") : _trackingController.Username;
                _trackingController.AddNotification(new TrackingNotification(DateTime.Now, new List<LineFragment>
                {
                    new CombatNotificationFragment(causerName, targetId.ToString(), damage)
                }));
            }

            if (_trackingController.UserObjectId == targetId)
            {
                var targetName = (string.IsNullOrEmpty(_trackingController.Username)) ? LanguageController.Translation("UNKNOWN") : _trackingController.Username;
                _trackingController.AddNotification(new TrackingNotification(DateTime.Now, new List<LineFragment>
                {
                    new CombatNotificationFragment(causerId.ToString(), targetName, damage)
                }));
            }
        }
    }
}