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
            var trackingController = ServiceLocator.Resolve<TrackingController>();

            var tasks = new List<Task>
            {
                Task.Run(SettingsController.SaveSettings),
                Task.Run(async () => { await trackingController?.SaveDataAsync()!; }),
                Task.Run(ItemController.SaveFavoriteItemsToLocalFile)
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

    public static SaveOnClosing GetStatus()
    {
        return _saveOnClosing;
    }
}