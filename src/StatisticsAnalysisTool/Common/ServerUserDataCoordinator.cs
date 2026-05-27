using Serilog;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Network;
using StatisticsAnalysisTool.ViewModels;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace StatisticsAnalysisTool.Common;

public sealed class ServerUserDataCoordinator
{
    private readonly AlbionServerDetectionService _albionServerDetectionService;
    private readonly MainWindowViewModel _mainWindowViewModel;
    private readonly SemaphoreSlim _syncLock = new(1, 1);

    public ServerUserDataCoordinator(AlbionServerDetectionService albionServerDetectionService, MainWindowViewModel mainWindowViewModel)
    {
        _mainWindowViewModel = mainWindowViewModel ?? throw new ArgumentNullException(nameof(mainWindowViewModel));

        if (albionServerDetectionService == null)
        {
            throw new ArgumentNullException(nameof(albionServerDetectionService));
        }

        _albionServerDetectionService = albionServerDetectionService;
        albionServerDetectionService.ServerChanged += AlbionServerDetectionService_ServerChanged;
    }

    public async Task SyncCurrentServerAsync()
    {
        if (_albionServerDetectionService.CurrentServerLocation is not (ServerLocation.America or ServerLocation.Asia or ServerLocation.Europe))
        {
            return;
        }

        await ApplyServerChangeAsync(
            AppDataPaths.ActiveUserDataServerLocation,
            _albionServerDetectionService.CurrentServerLocation,
            true).ConfigureAwait(false);
    }

    private async void AlbionServerDetectionService_ServerChanged(object sender, AlbionServerChangedEventArgs e)
    {
        await ApplyServerChangeAsync(AppDataPaths.ActiveUserDataServerLocation, e.CurrentServer.ServerLocation).ConfigureAwait(false);
    }

    private async Task ApplyServerChangeAsync(ServerLocation previousServerLocation, ServerLocation currentServerLocation, bool forceLoadCurrentServer = false)
    {
        await _syncLock.WaitAsync().ConfigureAwait(false);

        try
        {
            if (previousServerLocation is ServerLocation.America or ServerLocation.Asia or ServerLocation.Europe
                && previousServerLocation != currentServerLocation)
            {
                AppDataPaths.SetActiveUserDataServer(previousServerLocation);
                Log.Information("Saving Albion user data before server switch. Server={Server}, Directory={Directory}", previousServerLocation, AppDataPaths.UserDataDirectory);
                await CriticalData.SaveUserDataAsync().ConfigureAwait(false);
            }

            AppDataPaths.SetActiveUserDataServer(currentServerLocation);

            if (currentServerLocation is not (ServerLocation.America or ServerLocation.Asia or ServerLocation.Europe))
            {
                return;
            }

            Directory.CreateDirectory(AppDataPaths.UserDataDirectory);
            if (forceLoadCurrentServer || previousServerLocation != currentServerLocation)
            {
                await LoadCurrentServerUserDataAsync().ConfigureAwait(false);
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Server user data switch failed. Previous={PreviousServer}, Current={CurrentServer}", previousServerLocation, currentServerLocation);
        }
        finally
        {
            _syncLock.Release();
        }
    }

    private async Task LoadCurrentServerUserDataAsync()
    {
        if (Application.Current?.Dispatcher == null)
        {
            await _mainWindowViewModel.LoadUserDataForActiveServerAsync().ConfigureAwait(false);
            return;
        }

        var operation = Application.Current.Dispatcher.InvokeAsync(() => _mainWindowViewModel.LoadUserDataForActiveServerAsync());
        await operation.Task.Unwrap().ConfigureAwait(false);
    }
}
