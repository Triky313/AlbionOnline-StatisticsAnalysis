﻿using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Network.Manager;
using StatisticsAnalysisTool.Network.Notification;
using System;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Network.Handler
{
    public class ReceivedSeasonPointsEventHandler
    {
        private readonly TrackingController _trackingController;

        public ReceivedSeasonPointsEventHandler(TrackingController trackingController)
        {
            _trackingController = trackingController;
        }

        public async Task OnActionAsync(ReceivedSeasonPointsEvent value)
        {
            await Task.CompletedTask;
        }

        private TrackingNotification SetSeasonPointsNotification(int seasonPoints)
        {
            return new TrackingNotification(DateTime.Now, new SeasonPointsNotificationFragment(LanguageController.Translation("YOU_HAVE"), AttributeStatOperator.Plus, seasonPoints,
                LanguageController.Translation("SEASON_POINTS"), LanguageController.Translation("GAINED")), NotificationType.SeasonPoints);
        }
    }
}