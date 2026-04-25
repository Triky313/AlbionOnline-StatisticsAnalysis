using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Serilog;
using Serilog.Events;
using StatisticsAnalysisTool.Avalonia.Services;
using StatisticsAnalysisTool.Avalonia.ViewModels;
using StatisticsAnalysisTool.Avalonia.Views;
using System;
using System.IO;
using System.Runtime.Versioning;

namespace StatisticsAnalysisTool.Avalonia;

[SupportedOSPlatform("windows")]
public partial class App : Application
{
    private NetworkTrackingController? _networkTrackingController;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            if (BindingPlugins.DataValidators.Count > 0)
            {
                BindingPlugins.DataValidators.RemoveAt(0);
            }

            UiLogSink uiLogSink = new();
            InitLogger(uiLogSink);

            TrackingSettingsStore trackingSettingsStore = new();
            _networkTrackingController = new NetworkTrackingController();
            MainWindowViewModel mainWindowViewModel = new(_networkTrackingController, trackingSettingsStore, uiLogSink);

            desktop.MainWindow = new MainWindow
            {
                DataContext = mainWindowViewModel,
            };

            desktop.Exit += (_, _) =>
            {
                _networkTrackingController?.Dispose();
                Log.CloseAndFlush();
            };
        }

        base.OnFrameworkInitializationCompleted();
    }

    private static void InitLogger(UiLogSink uiLogSink)
    {
        const string logFolderName = "logs";
        Directory.CreateDirectory(Path.Combine(AppContext.BaseDirectory, logFolderName));

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console(restrictedToMinimumLevel: LogEventLevel.Information)
            .WriteTo.Debug(restrictedToMinimumLevel: LogEventLevel.Debug)
            .WriteTo.File(
                Path.Combine(AppContext.BaseDirectory, logFolderName, "sat-avalonia-.log"),
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 7,
                restrictedToMinimumLevel: LogEventLevel.Information)
            .WriteTo.Sink(uiLogSink, restrictedToMinimumLevel: LogEventLevel.Information)
            .CreateLogger();
    }
}
