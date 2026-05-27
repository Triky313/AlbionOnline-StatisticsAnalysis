using Serilog;
using StatisticsAnalysisTool.Common.UserSettings;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Localization;
using StatisticsAnalysisTool.Network.Manager;
using StatisticsAnalysisTool.Notification;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace StatisticsAnalysisTool.Common;

public class CriticalData
{
    private static SaveOnClosing _saveOnClosing;

    public static void Save()
    {
        Task.Run(SaveAsync).GetAwaiter().GetResult();
    }

    public static async Task SaveAsync()
    {
        if (_saveOnClosing is SaveOnClosing.IsRunning)
        {
            return;
        }

        _saveOnClosing = SaveOnClosing.IsRunning;

        try
        {
            var tasks = new List<Task>
            {
                Task.Run(SettingsController.SaveSettings),
                SaveUserDataAsync()
            };

            await Task.WhenAll(tasks);
            _ = ServiceLocator.Resolve<SatNotificationManager>().ShowTrackingStatusAsync(LocalizationController.Translation("DATA_SAVED"), 
                LocalizationController.Translation("ALL_TOOL_DATA_HAS_BEEN_SAVED"));
        }
        catch (KeyNotFoundException e)
        {
            Log.Error(e, "{message}", MethodBase.GetCurrentMethod()?.DeclaringType);
        }
        finally
        {
            _saveOnClosing = SaveOnClosing.Done;
        }
    }

    public static async Task SaveUserDataAsync()
    {
        if (!AppDataPaths.IsUserDataAvailable)
        {
            Log.Information("Skipped user data save because no Albion server is active.");
            return;
        }

        var trackingController = ServiceLocator.Resolve<TrackingController>();
        await Task.WhenAll(
            trackingController.SaveDataAsync(),
            Task.Run(ItemController.SaveFavoriteItemsToLocalFile));
    }

    public static SaveOnClosing GetStatus()
    {
        return _saveOnClosing;
    }
}
