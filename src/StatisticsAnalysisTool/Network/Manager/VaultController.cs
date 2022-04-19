using log4net;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Models.NetworkModel;
using StatisticsAnalysisTool.ViewModels;
using System;
using System.Linq;
using System.Reflection;

namespace StatisticsAnalysisTool.Network.Manager;

public class VaultController
{
    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);

    private readonly MainWindowViewModel _mainWindowViewModel;
    private Vault _currentVault;

    public VaultController(MainWindowViewModel mainWindowViewModel)
    {
        _mainWindowViewModel = mainWindowViewModel;
    }

    public void SetCurrentVault(Vault vault)
    {
        _currentVault = vault;
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