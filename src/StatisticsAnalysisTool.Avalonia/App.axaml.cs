using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;
using StatisticsAnalysisTool.Avalonia.Http;
using StatisticsAnalysisTool.Avalonia.ToolSettings;
using StatisticsAnalysisTool.Avalonia.ViewModels;
using StatisticsAnalysisTool.Avalonia.Views;
using System;
using System.IO;
using System.Reflection;

namespace StatisticsAnalysisTool.Avalonia;

public partial class App : Application
{
    private bool _isEarlyShutdown;

    public static IServiceProvider? ServiceProvider { get; private set; }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        // Line below is needed to remove Avalonia data validation.
        // Without this line you will get duplicate validations from both Avalonia and CT
        BindingPlugins.DataValidators.RemoveAt(0);

        AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
        InitLogger();

        var serviceCollection = new ServiceCollection();
        ConfigureServices(serviceCollection);
        ServiceProvider = serviceCollection.BuildServiceProvider();

        // TODO: Add tool Updater, maybe: https://github.com/NetSparkleUpdater/NetSparkle
        ServiceProvider.GetRequiredService<ISettingsController>().LoadSettings();

        var mainWindowViewModel = ServiceProvider.GetRequiredService<MainWindowViewModel>();
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = mainWindowViewModel
            };
            desktop.Exit += OnExit;
            desktop.MainWindow.Closed += OnClosing;
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            singleViewPlatform.MainView = new MainWindow
            {
                DataContext = mainWindowViewModel
            };
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<IHttpClientUtils, HttpClientUtils>();
        services.AddSingleton<ISettingsController, SettingsController>();

        services.AddSingleton<MainWindowViewModel>();

        services.AddSingleton<MainViewModel>();
        services.AddSingleton<ErrorBarViewModel>();
        services.AddSingleton<FooterViewModel>();

        //services.AddSingleton<GuildViewModel>();

        //services.AddSingleton<SettingsWindowViewModel>();
        //services.AddSingleton<SettingsControl>();

        //var satNotifications = new SatNotificationManager(new NotificationManager(Current.Dispatcher));
        //services.AddSingleton<ISatNotificationManager>(satNotifications);

        //services.AddSingleton<IBackupController, BackupController>();

        //services.AddSingleton<INetworkManager, NetworkManager>();
        //services.AddSingleton<ITrackingController, TrackingController>();
        //services.AddSingleton<ILootController, LootController>();
        //services.AddSingleton<IEntityController, EntityController>();
        //services.AddSingleton<IPartyBuilderController, PartyBuilderController>();
        //services.AddSingleton<IClusterController, ClusterController>();
        //services.AddSingleton<IDungeonController, DungeonController>();
        //services.AddSingleton<ICombatController, CombatController>();
        //services.AddSingleton<IStatisticController, StatisticController>();
        //services.AddSingleton<ITreasureController, TreasureController>();
        //services.AddSingleton<IMailController, MailController>();
        //services.AddSingleton<IMarketController, MarketController>();
        //services.AddSingleton<ITradeController, TradeController>();
        //services.AddSingleton<IVaultController, VaultController>();
        //services.AddSingleton<IGatheringController, GatheringController>();
        //services.AddSingleton<IGuildController, GuildController>();
        //services.AddSingleton<ILiveStatsTracker, LiveStatsTracker>();
        //services.AddSingleton<IGameEventWrapper, GameEventWrapper>();
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

    private static void InitLogger()
    {
        const string logFolderName = "logs";
        //DirectoryController.CreateDirectoryWhenNotExists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, logFolderName));

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

    private void OnExit(object? sender, ControlledApplicationLifetimeExitEventArgs e)
    {
        AppClosesProcess();
    }

    private void OnClosing(object? sender, EventArgs e)
    {
        AppClosesProcess();
    }

    private void AppClosesProcess()
    {
        //if (_isEarlyShutdown || ServiceProvider.GetService(typeof(ITrackingController)) is null)
        //{
        //    return;
        //}

        //var backupController = ServiceProvider.GetRequiredService<IBackupController>();
        //var trackingController = ServiceProvider.GetRequiredService<ITrackingController>();
        //var networkManager = ServiceProvider.GetRequiredService<INetworkManager>();
        var settingsController = ServiceProvider?.GetRequiredService<ISettingsController>();

        //networkManager.Stop();
        //trackingController.StopTracking();
        //trackingController.SaveDataAsync();
        settingsController?.SaveSettings();
        //if (!backupController.ExistBackupOnSettingConditions())
        //{
        //    backupController.Save();
        //}
    }
}
