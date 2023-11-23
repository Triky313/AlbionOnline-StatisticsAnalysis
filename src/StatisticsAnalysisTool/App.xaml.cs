using Microsoft.Extensions.DependencyInjection;
using Notification.Wpf;
using Serilog;
using Serilog.Events;
using StatisticsAnalysisTool.Backup;
using StatisticsAnalysisTool.Cluster;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Common.UserSettings;
using StatisticsAnalysisTool.Core;
using StatisticsAnalysisTool.Dungeon;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Exceptions;
using StatisticsAnalysisTool.GameFileData;
using StatisticsAnalysisTool.Gathering;
using StatisticsAnalysisTool.Guild;
using StatisticsAnalysisTool.Localization;
using StatisticsAnalysisTool.Network;
using StatisticsAnalysisTool.Network.Manager;
using StatisticsAnalysisTool.Network.PacketProviders;
using StatisticsAnalysisTool.Notification;
using StatisticsAnalysisTool.PartyBuilder;
using StatisticsAnalysisTool.Trade;
using StatisticsAnalysisTool.Trade.Mails;
using StatisticsAnalysisTool.Trade.Market;
using StatisticsAnalysisTool.UserControls;
using StatisticsAnalysisTool.ViewModels;
using StatisticsAnalysisTool.Views;
using System;
using System.IO;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Threading;

namespace StatisticsAnalysisTool;

public partial class App
{
    private bool _isEarlyShutdown;

    public static IServiceProvider ServiceProvider { get; private set; }

    protected override async void OnStartup(StartupEventArgs e)
    {
        var serviceCollection = new ServiceCollection();
        ConfigureServices(serviceCollection);
        ServiceProvider = serviceCollection.BuildServiceProvider();

        base.OnStartup(e);

        Current.ShutdownMode = ShutdownMode.OnExplicitShutdown;

        AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
        InitLogger();

        await ServiceProvider.GetRequiredService<IAutoUpdateController>().AutoUpdateAsync();
        ServiceProvider.GetRequiredService<IAutoUpdateController>().RemoveUpdateFiles();
        ServiceProvider.GetRequiredService<ISettingsController>().LoadSettings();

        Culture.SetCulture(Culture.GetCulture(SettingsController.CurrentSettings.CurrentCultureIetfLanguageTag));
        if (!LanguageController.Init())
        {
            _isEarlyShutdown = true;
            Current.Shutdown();
            return;
        }

        if (SettingsController.CurrentSettings.ServerLocation != ServerLocation.West
            && SettingsController.CurrentSettings.ServerLocation != ServerLocation.East)
        {
            Server.SetServerLocationWithDialogAsync();
        }

        if (!await GameData.InitializeMainGameDataFilesAsync(SettingsController.CurrentSettings.ServerType))
        {
            _isEarlyShutdown = true;
            Current.Shutdown();
            return;
        }

        await ServiceProvider.GetRequiredService<IBackupController>().DeleteOldestBackupsIfNeededAsync();

        Current.ShutdownMode = ShutdownMode.OnMainWindowClose;

        var mainWindow = new MainWindow(ServiceProvider.GetRequiredService<MainWindowViewModel>());
        //var mainWindow = new MainWindow(ServiceProvider.GetRequiredService<MainWindowViewModel>(), 
        //    ServiceProvider.GetRequiredService<IEntityController>(), 
        //    ServiceProvider.GetRequiredService<ISettingsController>());
        //mainWindow.DataContext = ServiceProvider.GetRequiredService<MainWindowViewModelNew>();
        Current.MainWindow = mainWindow;
        //await ServiceProvider.GetRequiredService<MainWindowViewModelNew>().InitMainWindowDataAsync();

        StartNetworkTracking();

        mainWindow.Show();

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

        Log.Information($"Tool started with v{Assembly.GetExecutingAssembly().GetName().Version}");
        SystemInfo.LogSystemInfo();
    }

    private void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IHttpClientUtils, HttpClientUtils>();
        services.AddSingleton<IAutoUpdateController, AutoUpdateController>();
        services.AddSingleton<ISettingsController, SettingsController>();
        
        services.AddSingleton<MainWindowViewModel>();
        services.AddSingleton<MainWindowViewModelOld>();

        services.AddSingleton<ErrorBarViewModel>();
        services.AddSingleton<GuildViewModel>();

        services.AddSingleton<SettingsWindowViewModel>();
        services.AddSingleton<SettingsControl>();

        var satNotifications = new SatNotificationManager(new NotificationManager(Current.Dispatcher));
        services.AddSingleton<ISatNotificationManager>(satNotifications);

        services.AddSingleton<IBackupController, BackupController>();

        services.AddSingleton<INetworkManager, NetworkManager>();
        services.AddSingleton<ITrackingController, TrackingController>();
        services.AddSingleton<ILootController, LootController>();
        services.AddSingleton<IEntityController, EntityController>();
        services.AddSingleton<IPartyBuilderController, PartyBuilderController>();
        services.AddSingleton<IClusterController, ClusterController>();
        services.AddSingleton<IDungeonController, DungeonController>();
        services.AddSingleton<ICombatController, CombatController>();
        services.AddSingleton<IStatisticController, StatisticController>();
        services.AddSingleton<ITreasureController, TreasureController>();
        services.AddSingleton<IMailController, MailController>();
        services.AddSingleton<IMarketController, MarketController>();
        services.AddSingleton<ITradeController, TradeController>();
        services.AddSingleton<IVaultController, VaultController>();
        services.AddSingleton<IGatheringController, GatheringController>();
        services.AddSingleton<IGuildController, GuildController>();
        services.AddSingleton<ILiveStatsTracker, LiveStatsTracker>();
        services.AddSingleton<IGameEventWrapper, GameEventWrapper>();
    }

    public static void StartNetworkTracking()
    {
        var mainWindowViewModel = ServiceProvider.GetRequiredService<MainWindowViewModelOld>();
        var errorBarViewModel = ServiceProvider.GetRequiredService<ErrorBarViewModel>();

        if (!ApplicationCore.IsAppStartedAsAdministrator() && SettingsController.CurrentSettings.PacketProvider == PacketProviderKind.Sockets)
        {
            errorBarViewModel.Set(Visibility.Visible, LanguageController.Translation("START_APPLICATION_AS_ADMINISTRATOR"));
            return;
        }

        try
        {
            ServiceProvider.GetRequiredService<INetworkManager>().Start();
            mainWindowViewModel.IsTrackingActive = true;
        }
        catch (NoListeningAdaptersException e)
        {
            ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
            Log.Error(e, "{message}", MethodBase.GetCurrentMethod()?.DeclaringType);
            errorBarViewModel.Set(Visibility.Visible, LanguageController.Translation("NO_LISTENING_ADAPTERS"));
        }
        catch (SocketException e)
        {
            ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
            Log.Error(e, "{message}|{socketErrorCode}", MethodBase.GetCurrentMethod()?.DeclaringType, e.SocketErrorCode);
            errorBarViewModel.Set(Visibility.Visible, $"Socket Exception - {e.Message}");
        }
        catch (Exception e)
        {
            ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
            Log.Error(e, "{message}", MethodBase.GetCurrentMethod()?.DeclaringType);
            errorBarViewModel.Set(Visibility.Visible, LanguageController.Translation("PACKET_HANDLER_ERROR_MESSAGE"));

            ServiceProvider.GetRequiredService<INetworkManager>().Stop();
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
        AppClosesProcess();
    }

    private void OnSessionEnding(object sender, SessionEndingCancelEventArgs e)
    {
        AppClosesProcess();
    }

    private void AppClosesProcess()
    {
        if (_isEarlyShutdown || ServiceProvider.GetService(typeof(ITrackingController)) is null)
        {
            return;
        }

        var backupController = ServiceProvider.GetRequiredService<IBackupController>();
        var trackingController = ServiceProvider.GetRequiredService<ITrackingController>();
        var networkManager = ServiceProvider.GetRequiredService<INetworkManager>();
        var settingsController = ServiceProvider.GetRequiredService<ISettingsController>();

        networkManager.Stop();
        trackingController.StopTracking();
        trackingController.SaveDataAsync();
        settingsController.SaveSettings();
        if (!backupController.ExistBackupOnSettingConditions())
        {
            backupController.Save();
        }
    }
}