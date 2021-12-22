using log4net;
using StatisticsAnalysisTool.Properties;
using System;
using System.DirectoryServices.ActiveDirectory;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.Json;

namespace StatisticsAnalysisTool.Common.UserSettings
{
    public static class SettingsController
    {
        public static SettingsObject CurrentSettings = new();

        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);
        private static bool _haveSettingsAlreadyBeenLoaded = false;

        public static void LoadSettings()
        {
            if (_haveSettingsAlreadyBeenLoaded)
            {
                return;
            }

            var localFilePath = $"{AppDomain.CurrentDomain.BaseDirectory}{Settings.Default.SettingsFileName}";

            if (File.Exists(localFilePath))
            {
                try
                {
                    var settingsString = File.ReadAllText(localFilePath, Encoding.UTF8);
                    CurrentSettings = JsonSerializer.Deserialize<SettingsObject>(settingsString);
                    _haveSettingsAlreadyBeenLoaded = true;
                }
                catch (Exception e)
                {
                    ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
                    Log.Error(MethodBase.GetCurrentMethod()?.DeclaringType, e);
                }
            }
        }

        public static void Save()
        {
            var localFilePath = $"{AppDomain.CurrentDomain.BaseDirectory}{Settings.Default.SettingsFileName}";

            try
            {
                var fileString = JsonSerializer.Serialize(CurrentSettings);
                File.WriteAllText(localFilePath, fileString, Encoding.UTF8);
            }
            catch (Exception e)
            {
                ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
                Log.Error(MethodBase.GetCurrentMethod()?.DeclaringType, e);
            }
        }
    }
}