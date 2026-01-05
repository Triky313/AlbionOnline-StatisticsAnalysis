using Notification.Wpf;
using Serilog;
using Serilog.Events;
using StatisticsAnalysisTool.Backup;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Common.UserSettings;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.GameFileData;
using StatisticsAnalysisTool.Localization;
using StatisticsAnalysisTool.Network.Manager;
using StatisticsAnalysisTool.Notification;
using StatisticsAnalysisTool.ViewModels;
using StatisticsAnalysisTool.Views;
using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using StatisticsAnalysisTool.Diagnostics;
using StatisticsAnalysisTool.Network;
using StatisticsAnalysisTool.UserControls;

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

            InitLogger();
            Log.Information("Tool started with v{Version}", Assembly.GetExecutingAssembly().GetName().Version);

            SystemInfo.LogSystemInfo();
            DebugConsole.UseEnums(typeof(EventCodes), typeof(OperationCodes));

            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
            DispatcherUnhandledException += Application_DispatcherUnhandledException;

            await SettingsController.LoadSettingsAsync();

            if (SettingsController.CurrentSettings.IsOpenDebugConsoleWhenStartingTheToolChecked)
            {
                DebugConsole.Attach("SAT Debug Console");
                DebugConsole.Configure(SettingsController.CurrentSettings.DebugConsoleFilter);
            }

            await AutoUpdateController.AutoUpdateAsync();

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

            await BackupController.DeleteOldestBackupsIfNeededAsync();

            Current.ShutdownMode = ShutdownMode.OnMainWindowClose;

            RegisterServicesEarly();
            Current.MainWindow = new MainWindow(_mainWindowViewModel);
            RegisterServicesLate();

            await _mainWindowViewModel.InitMainWindowDataAsync();
            Current.MainWindow.Show();

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
        if (e.Exception is COMException { ErrorCode: -2147221040 })
        {
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

    protected override void OnExit(ExitEventArgs e)
    {
        if (_isEarlyShutdown || _trackingController is null)
        {
            return;
        }

        try
        {
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