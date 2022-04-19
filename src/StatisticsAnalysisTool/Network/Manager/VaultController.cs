using log4net;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Models.NetworkModel;
using StatisticsAnalysisTool.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace StatisticsAnalysisTool.Network.Manager;

public class VaultController
{
    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);

    private readonly TrackingController _trackingController;
    private readonly MainWindowViewModel _mainWindowViewModel;
    private Vault _currentVault;
    private List<DiscoveredItem> _discoveredItems = new ();

    public VaultController(TrackingController trackingController, MainWindowViewModel mainWindowViewModel)
    {
        _trackingController = trackingController;
        _mainWindowViewModel = mainWindowViewModel;
    }

    public void SetCurrentVault(Vault vault)
    {
        _currentVault = vault;
    }

    public void Add(DiscoveredItem item)
    {
        if (_discoveredItems.Exists(x => x.ObjectId == item.ObjectId))
        {
            return;
        }

        _discoveredItems.Add(item);
    }

    public void ResetDiscoveredItems()
    {
        _discoveredItems.Clear();
    }

    public static VaultLocation GetVaultLocation(string value)
    {
        var clusterInfoArray = value.Split("@");
        return clusterInfoArray.ElementAtOrDefault(1) != null ? GetVaultLocationByIndex(clusterInfoArray[1]) : VaultLocation.Unknown;
    }

    #region Helper methods

    private static VaultLocation GetVaultLocationByIndex(string index)
    {
        return Enum.TryParse(index, true, out VaultLocation location) ? location : VaultLocation.Unknown;
    }

    #endregion
}