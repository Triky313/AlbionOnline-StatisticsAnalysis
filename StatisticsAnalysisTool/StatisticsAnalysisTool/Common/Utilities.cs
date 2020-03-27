using AutoUpdaterDotNET;
using StatisticsAnalysisTool.Properties;
using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows;

namespace StatisticsAnalysisTool.Common
{
    public static class Utilities
    {
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

        public static bool IsWindowOpen<T>(string name = "") where T : Window
        {
            return string.IsNullOrEmpty(name)
                ? Application.Current.Windows.OfType<T>().Any()
                : Application.Current.Windows.OfType<T>().Any(w => w.Name.Equals(name));
        }

        public static string UlongMarketPriceToString(ulong value) => value.ToString("N0", new CultureInfo(LanguageController.CurrentCultureInfo.TextInfo.CultureName));

        public static string MarketPriceDateToString(DateTime value) => DateTime.SpecifyKind(value, DateTimeKind.Utc).ToLocalTime().ToString("G", new CultureInfo(LanguageController.CurrentCultureInfo.TextInfo.CultureName));
    }
}