using StatisticsAnalysisTool.Enumerations;
using System;
using System.Collections.Generic;

namespace StatisticsAnalysisTool.Network.Notification
{
    public class TrackingNotification
    {
        public TrackingNotification(DateTime dateTime, IEnumerable<LineFragment> fragments, NotificationType type)
        {
            DateTime = dateTime;
            Fragments = fragments;
            Type = type;
        }

        public DateTime DateTime { get; }
        public IEnumerable<LineFragment> Fragments { get; }
        public NotificationType Type { get; }
    }
}