using StatisticsAnalysisTool.Common.UserSettings;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Network.Manager;
using StatisticsAnalysisTool.ViewModels;
using System.Collections.Generic;
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
        if (_saveOnClosing is SaveOnClosing.IsRunning or SaveOnClosing.Done)
        {
            return;
        }

        _saveOnClosing = SaveOnClosing.IsRunning;

        var trackingController = ServiceLocator.Resolve<TrackingController>();

        var tasks = new List<Task>
        {
            Task.Run(SettingsController.SaveSettings),
            Task.Run(async () => { await trackingController?.SaveDataAsync()!; })
        };

        await Task.WhenAll(tasks);

        _saveOnClosing = SaveOnClosing.Done;
    }

    public static SaveOnClosing GetStatus()
    {
        return _saveOnClosing;
    }
}