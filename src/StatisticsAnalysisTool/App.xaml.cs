using Notification.Wpf;
using Serilog;
using Serilog.Events;
using StatisticsAnalysisTool.Backup;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Common.UserSettings;
using StatisticsAnalysisTool.Diagnostics;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.GameFileData;
using StatisticsAnalysisTool.Localization;
using StatisticsAnalysisTool.Network;
using StatisticsAnalysisTool.Network.Manager;
using StatisticsAnalysisTool.Notification;
using StatisticsAnalysisTool.ViewModels;
using StatisticsAnalysisTool.Views;
using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace StatisticsAnalysisTool;

public partial class App
{
    private MainWindowViewModel _mainWindowViewModel;
    private TrackingController _trackingController;
    private bool _isEarlyShutdown;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        try
        {
            Current.ShutdownMode = ShutdownMode.OnExplicitShutdown;

            AppDataPaths.EnsureBaseDirectory();
            var migrationMessages = AppDataMigration.MigrateLegacyRuntimeData();
            AppDataPaths.EnsureRuntimeDirectories();
            InitLogger();
            AppDataMigration.LogMessages(migrationMessages);
            Log.Information("Tool started with v{Version}", Assembly.GetExecutingAssembly().GetName().Version);

            SystemInfo.LogSystemInfo();
            DebugConsole.UseEnums(typeof(EventCodes), typeof(OperationCodes));

            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;

            await SettingsController.LoadSettingsAsync();

            if (SettingsController.CurrentSettings.IsOpenDebugConsoleWhenStartingTheToolChecked)
            {
                DebugConsole.Attach("SAT Debug Console");
                DebugConsole.Configure(SettingsController.CurrentSettings.DebugConsoleFilter);
            }

            Culture.SetCulture(Culture.GetCultureByIetfLanguageTag(SettingsController.CurrentSettings.CurrentCultureIetfLanguageTag));
            if (!LocalizationController.Init())
            {
                _isEarlyShutdown = true;
                Current.Shutdown();
                return;
            }

            if (SettingsController.CurrentSettings.ServerLocation != ServerLocation.America
                && SettingsController.CurrentSettings.ServerLocation != ServerLocation.Asia
                && SettingsController.CurrentSettings.ServerLocation != ServerLocation.Europe)
            {
                Server.SetServerLocationWithDialogAsync();
            }

            if (!await GameData.InitializeMainGameDataFilesAsync(SettingsController.CurrentSettings.ServerType))
            {
                _isEarlyShutdown = true;
                Current.Shutdown();
                return;
            }

            ShowNpcapInfoDialogOnFirstStart();

            await BackupController.DeleteOldestBackupsIfNeededAsync();

            Current.ShutdownMode = ShutdownMode.OnMainWindowClose;

            RegisterServicesEarly();
            Current.MainWindow = new MainWindow(_mainWindowViewModel);
            RegisterServicesLate();

            await _mainWindowViewModel.InitMainWindowDataAsync();
            Current.MainWindow.Show();

            await AutoUpdateController.StartBackgroundUpdateLoopAsync();

            Utilities.AnotherAppToStart(SettingsController.CurrentSettings.AnotherAppToStartPath);
        }
        catch (Exception ex)
        {
            _isEarlyShutdown = true;
            try
            {
                Log.Fatal(ex, "An unexpected fatal error has occurred.");
                MessageBox.Show("An unexpected error has occurred.",
                    "Statistics Analysis Tool",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            finally
            {
                await Log.CloseAndFlushAsync();
            }

            Current.Shutdown();
        }
    }

    private static void ShowNpcapInfoDialogOnFirstStart()
    {
        if (!SettingsController.CurrentSettings.IsNpcapInfoDialogShownOnStart)
        {
            return;
        }

        var dialog = new DialogWindow(
            LocalizationController.Translation("NPCAP_INFO_DIALOG_TITLE"),
            LocalizationController.Translation("NPCAP_INFO_DIALOG_MESSAGE"),
            DialogType.Ok,
            "https://npcap.com/",
            LocalizationController.Translation("NPCAP_INFO_DIALOG_LINK_TEXT"),
            300d);

        dialog.ShowDialog();

        SettingsController.CurrentSettings.IsNpcapInfoDialogShownOnStart = false;
    }


    private static void InitLogger()
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .WriteTo.File(
                AppDataPaths.LogFilePattern,
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

    private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        try
        {
            if (e.ExceptionObject is Exception exception)
            {
                Log.Fatal(exception, "Unhandled exception in {Source}", MethodBase.GetCurrentMethod()?.DeclaringType);
            }
            else
            {
                Log.Fatal("Unhandled exception object: {ExceptionObject} in {Source}", e.ExceptionObject, MethodBase.GetCurrentMethod()?.DeclaringType);
            }
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Error during unhandled exception logging in {Source}", MethodBase.GetCurrentMethod()?.DeclaringType);
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    private void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
    {
        Log.Fatal(e.Exception, "Unhandled exception in TaskScheduler");
        e.SetObserved();
    }

    private void Application_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        // Fixes a issue in the WPF clipboard handler.
        // It is necessary to handle the unhandled exception in the Application.DispatcherUnhandledException event.
        if (e.Exception is COMException comException
            && comException.ErrorCode == -2147221040)
        {
            e.Handled = true;
            return;
        }

        if (IsRecoverableLiveChartsTickerException(e.Exception))
        {
            Log.Debug(e.Exception, "Ignored a recoverable LiveCharts ticker disposal exception.");
            e.Handled = true;
            return;
        }

        try
        {
            Log.Fatal(e.Exception, "Unhandled exception in UI thread");
        }
        finally
        {
            Log.CloseAndFlush();
            e.Handled = false;
        }
    }

    private static bool IsRecoverableLiveChartsTickerException(Exception exception)
    {
        if (exception is not NullReferenceException)
        {
            return false;
        }

        var stackTrace = new StackTrace(exception, false);
        return ContainsStackFrame(stackTrace, "LiveChartsCore.SkiaSharpView.WPF.Rendering.CompositionTargetTicker", "DisposeTicker")
            && ContainsStackFrame(stackTrace, "LiveChartsCore.Motion.MotionCanvasComposer", "Dispose");
    }

    private static bool ContainsStackFrame(StackTrace stackTrace, string typeName, string methodName)
    {
        foreach (var frame in stackTrace.GetFrames())
        {
            var method = frame.GetMethod();
            if (method?.Name == methodName
                && method != null
                && method.DeclaringType?.FullName == typeName)
            {
                return true;
            }
        }

        return false;
    }

    protected override void OnExit(ExitEventArgs e)
    {
        if (_isEarlyShutdown || _trackingController is null)
        {
            return;
        }

        try
        {
            ServiceLocator.Resolve<SatNotificationManager>().StopShowingNotifications();
            AutoUpdateController.Dispose();
            _trackingController?.StopTracking();
            CriticalData.Save();

            if (!BackupController.ExistBackupOnSettingConditions())
            {
                BackupController.Save();
            }
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    private void OnSessionEnding(object sender, SessionEndingCancelEventArgs e)
    {
        if (_isEarlyShutdown || _trackingController is null)
        {
            return;
        }

        try
        {
            ServiceLocator.Resolve<SatNotificationManager>().StopShowingNotifications();
            AutoUpdateController.Dispose();
            _trackingController?.StopTracking();
            CriticalData.Save();

            if (!BackupController.ExistBackupOnSettingConditions())
            {
                BackupController.Save();
            }
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }
}
