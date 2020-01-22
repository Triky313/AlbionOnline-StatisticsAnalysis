using AutoUpdaterDotNET;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using System.Windows;
using StatisticsAnalysisTool.Properties;

namespace StatisticsAnalysisTool.Common
{
    public static class Utilities
    {
        public static string DateFormat(string format, double hourModify)
        {
            if (string.IsNullOrWhiteSpace(format))
                return new DateTime(1, 1, 1).ToString(CultureInfo.InvariantCulture);

            try
            {
                var value = DateTime.Parse(format, LanguageController.DefaultCultureInfo);
                value = value.AddHours(hourModify);
                return value.ToString(new CultureInfo(LanguageController.CurrentLanguage));
            }
            catch (FormatException ex)
            {
                Debug.Print(ex.ToString());
                return new DateTime(0).ToString(CultureInfo.InvariantCulture);
            }
        }

        public static string DateFormat(DateTime format, double hourModify) => DateFormat(format.ToString(CultureInfo.InvariantCulture), hourModify);

        public static bool IsDateTimeOutdated(string format, double hourModify)
        {
            if (string.IsNullOrWhiteSpace(format))
                return true;

            if (!DateTime.TryParse(format, out DateTime dateTime))
                return true;

            return IsDateTimeOutdated(dateTime, hourModify);
        }

        public static bool IsDateTimeOutdated(DateTime dateTime, double hourModify)
        {
            dateTime = dateTime.AddHours(hourModify);
            return dateTime < DateTime.Now.ToUniversalTime();
        }

        public static long MapUlongToLong(ulong ulongValue) => Convert.ToInt64(ulongValue);

        public static void AddRange<T>(this ConcurrentBag<T> @this, IEnumerable<T> toAdd)
        {
            foreach (var element in toAdd)
            {
                @this.Add(element);
            }
        }

        public static void AutoUpdate()
        {
            AutoUpdater.Start(Settings.Default.AutoUpdateConfigUrl);
            AutoUpdater.DownloadPath = Environment.CurrentDirectory;
            AutoUpdater.RunUpdateAsAdmin = false;
            AutoUpdater.ApplicationExitEvent += AutoUpdater_ApplicationExitEvent;
        }

        private static void AutoUpdater_ApplicationExitEvent()
        {
            Thread.Sleep(5000);
            Application.Current.Shutdown();
        }
    }
}