using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Localization;
using StatisticsAnalysisTool.Network.Manager;
using System.Reflection;
using System;
using System.IO;
using System.Windows.Input;
using Microsoft.Win32;
using Serilog;

namespace StatisticsAnalysisTool.Trade;

public class TradeExportTemplateObject
{

    #region Commands

    public void TradeExport(object obj)
    {
        var dialog = new SaveFileDialog
        {
            FileName = $"trades-{DateTime.UtcNow:yyyy-MM-dd-hh-mm-ss}utc",
            DefaultExt = ".csv",
            Filter = "CSV documents (.csv)|*.csv"
        };

        var result = dialog.ShowDialog();
        if (result == true)
        {
            try
            {
                var trackingController = ServiceLocator.Resolve<TrackingController>();
                File.WriteAllText(dialog.FileName, trackingController?.TradeController?.GetTradesAsCsv());
            }
            catch (Exception e)
            {
                ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
                Log.Error(e, "{message}", MethodBase.GetCurrentMethod()?.DeclaringType);
            }
        }
    }

    private ICommand _tradeExportCommand;

    public ICommand TradeExportCommand => _tradeExportCommand ??= new CommandHandler(TradeExport, true);

    #endregion

    public static string TranslationExport => LanguageController.Translation("EXPORT");
    public static string TranslationExportTradesAsCsv => LanguageController.Translation("EXPORT_TRADES_AS_CSV");
}