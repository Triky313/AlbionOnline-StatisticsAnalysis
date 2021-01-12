using System;
using System.Collections.Generic;

namespace StatisticsAnalysisTool.Network.Notification
{
    public class TrackingNotification
    {
        public TrackingNotification(DateTime dateTime, IEnumerable<LineFragment> fragments)
        {
            DateTime = dateTime;
            Fragments = fragments;
        }

        public DateTime DateTime { get; }

        public IEnumerable<LineFragment> Fragments { get; }
    }
}