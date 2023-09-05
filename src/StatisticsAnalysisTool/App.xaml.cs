using Notification.Wpf;
using Serilog;
using Serilog.Events;
using StatisticsAnalysisTool.Backup;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Common.UserSettings;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.GameFileData;
using StatisticsAnalysisTool.Network.Manager;
using StatisticsAnalysisTool.Notification;
using StatisticsAnalysisTool.ViewModels;
using StatisticsAnalysisTool.Views;
using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Threading;

namespace StatisticsAnalysisTool;

public partial class App
{
    private MainWindowViewModel _mainWindowViewModel;
    private TrackingController _trackingController;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        InitLogger();
        Log.Information($"Tool started with v{Assembly.GetExecutingAssembly().GetName().Version}");

        AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

        SettingsController.LoadSettings();
        InitializeLanguage();

        AutoUpdateController.RemoveUpdateFiles();
        await AutoUpdateController.AutoUpdateAsync();
        await BackupController.DeleteOldestBackupsIfNeededAsync();

        RegisterServicesEarly();

        Current.MainWindow = new MainWindow(_mainWindowViewModel);
        await GameData.InitializeMainGameDataFilesAsync();

        RegisterServicesLate();

        await _mainWindowViewModel.InitMainWindowDataAsync();
        Current.MainWindow.Show();


        Utilities.AnotherAppToStart(SettingsController.CurrentSettings.AnotherAppToStartPath);
    }

    private static void InitLogger()
    {
        const string logFolderName = "logs";
        DirectoryController.CreateDirectoryWhenNotExists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, logFolderName));

        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .WriteTo.File(
                Path.Combine(logFolderName, "sat-.logs"),
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 7,
                restrictedToMinimumLevel: LogEventLevel.Information)
            .MinimumLevel.Verbose()
            .WriteTo.Debug(
                restrictedToMinimumLevel: LogEventLevel.Verbose)
            .CreateLogger();
    }

    private void RegisterServicesEarly()
    {
        _mainWindowViewModel = new MainWindowViewModel();
        ServiceLocator.Register<MainWindowViewModel>(_mainWindowViewModel);

        var satNotifications = new SatNotificationManager(new NotificationManager(Current.Dispatcher));
        ServiceLocator.Register<SatNotificationManager>(satNotifications);
    }

    private void RegisterServicesLate()
    {
        _trackingController = new TrackingController(_mainWindowViewModel);
        ServiceLocator.Register<TrackingController>(_trackingController);
    }

    private static void InitializeLanguage()
    {
        if (LanguageController.InitializeLanguage())
        {
            return;
        }

        var dialogWindow = new DialogWindow(
            "LANGUAGE FILE NOT FOUND",
            "No language file was found, please add one and restart the tool!",
            DialogType.Error);
        var dialogResult = dialogWindow.ShowDialog();

        if (dialogResult is not true)
        {
            Current.Shutdown();
        }
    }

    private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        try
        {
            Log.Fatal(e.ExceptionObject as Exception, "{message}", MethodBase.GetCurrentMethod()?.DeclaringType);
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "{message}", MethodBase.GetCurrentMethod()?.DeclaringType);
        }
    }

    // Fixes a issue in the WPF clipboard handler.
    // It is necessary to handle the unhandled exception in the Application.DispatcherUnhandledException event.
    private void Application_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        if (e.Exception is COMException { ErrorCode: -2147221040 })
        {
            e.Handled = true;
        }
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _trackingController?.StopTracking();
        CriticalData.Save();
        if (!BackupController.ExistBackupOnSettingConditions())
        {
            BackupController.Save();
        }
    }

    private void OnSessionEnding(object sender, SessionEndingCancelEventArgs e)
    {
        _trackingController?.StopTracking();
        CriticalData.Save();
        if (!BackupController.ExistBackupOnSettingConditions())
        {
            BackupController.Save();
        }
    }
}