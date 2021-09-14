using log4net;
using StatisticsAnalysisTool.Properties;
using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Common.UserSettings
{
    public static class SettingsController
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);

        public static SettingsObject CurrentSettings = new();

        public static async Task<bool> LoadAsync()
        {
            var localFilePath = $"{AppDomain.CurrentDomain.BaseDirectory}{Settings.Default.SettingsFileName}";

            if (File.Exists(localFilePath))
            {
                try
                {
                    var settingsString = await File.ReadAllTextAsync(localFilePath, Encoding.UTF8);
                    CurrentSettings = JsonSerializer.Deserialize<SettingsObject>(settingsString);
                    return true;
                }
                catch (Exception e)
                {
                    ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
                    Log.Error(MethodBase.GetCurrentMethod()?.DeclaringType, e);
                    return false;
                }
            }

            return false;
        }

        public static async Task SaveAsync()
        {
            var localFilePath = $"{AppDomain.CurrentDomain.BaseDirectory}{Settings.Default.SettingsFileName}";

            try
            {
                var fileString = JsonSerializer.Serialize(CurrentSettings);
                await File.WriteAllTextAsync(localFilePath, fileString, Encoding.UTF8);
            }
            catch (Exception e)
            {
                ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
                Log.Error(MethodBase.GetCurrentMethod()?.DeclaringType, e);
            }
        }
    }
}