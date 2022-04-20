using log4net;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Models.NetworkModel;
using StatisticsAnalysisTool.Properties;
using StatisticsAnalysisTool.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Windows;
using StatisticsAnalysisTool.Models.BindingModel;

namespace StatisticsAnalysisTool.Network.Manager;

public class VaultController
{
    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);

    private readonly TrackingController _trackingController;
    private readonly MainWindowViewModel _mainWindowViewModel;
    private VaultInfo _currentVaultInfo;
    private readonly List<DiscoveredItem> _discoveredItems = new();
    private readonly List<ItemContainerObject> _vaultContainer = new();
    private ObservableCollection<Vault> _vaults = new();

    public VaultController(TrackingController trackingController, MainWindowViewModel mainWindowViewModel)
    {
        _trackingController = trackingController;
        _mainWindowViewModel = mainWindowViewModel;

        OnVaultsChange += UpdateUi;
    }

    public event Action OnVaultsChange;

    public void SetCurrentVault(VaultInfo vaultInfo)
    {
        if (vaultInfo == null || vaultInfo.VaultLocation == VaultLocation.Unknown)
        {
            return;
        }

        _currentVaultInfo = vaultInfo;
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

    public void AddContainer(ItemContainerObject newContainerObject)
    {
        if (newContainerObject?.ContainerGuid == default)
        {
            return;
        }

        var container = _vaultContainer.FirstOrDefault(x => x.ContainerGuid == newContainerObject.ContainerGuid);
        if (container != null)
        {
            _vaultContainer.Remove(container);
        }

        _vaultContainer.Add(newContainerObject);

        ParseVault();
    }

    public void ResetVaultContainer()
    {
        _vaultContainer.Clear();
    }

    private void ParseVault()
    {
        if (_currentVaultInfo == null || GetVaultLocation(_currentVaultInfo?.Location) == VaultLocation.Unknown)
        {
            return;
        }

        var removableVaultInfo = _vaults.FirstOrDefault(x => x.Location == _currentVaultInfo.Location);

        if (removableVaultInfo != null)
        {
            _vaults.Remove(removableVaultInfo);
        }

        var vault = new Vault()
        {
            Location = _currentVaultInfo.Location
        };

        try
        {
            for (var i = 0; i < _currentVaultInfo.ContainerGuidList.Count; i++)
            {
                var vaultContainer = new VaultContainer()
                {
                    Guid = _currentVaultInfo.ContainerGuidList[i],
                    Icon = _currentVaultInfo.ContainerIconTags[i],
                    Name = _currentVaultInfo.ContainerNames[i]
                };

                var itemContainer = _vaultContainer.FirstOrDefault(x => x.ContainerGuid == vaultContainer.Guid);

                SetItemsToVaultContainer(itemContainer, vaultContainer, _discoveredItems);

                vault.VaultContainer.Add(vaultContainer);
            }

            _vaults.Add(vault);
            OnVaultsChange?.Invoke();
        }
        catch (Exception e)
        {
            ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
        }
    }

    private void SetItemsToVaultContainer(ItemContainerObject containerObject, VaultContainer vaultContainer, List<DiscoveredItem> discoveredItems)
    {
        foreach (var slotItemId in containerObject?.SlotItemId ?? new List<int>())
        {
            var slotItem = discoveredItems.FirstOrDefault(x => x.ObjectId == slotItemId);

            if (slotItem == null || slotItem.ItemIndex == 0)
            {
                vaultContainer.Items.Add(new ContainerItem()
                {
                    ItemIndex = 0,
                    Quantity = 0
                });

                continue;
            }

            vaultContainer.Items.Add(new ContainerItem()
            {
                ItemIndex = slotItem.ItemIndex,
                Quantity = slotItem.Quantity
            });
        }
    }

    public static VaultLocation GetVaultLocation(string value)
    {
        var clusterInfoArray = value.Split("@");
        return clusterInfoArray.ElementAtOrDefault(1) != null ? GetVaultLocationByIndex(clusterInfoArray[1]) : VaultLocation.Unknown;
    }

    public static string GetVaultLocationIndex(string value)
    {
        var clusterInfoArray = value.Split("@");
        return clusterInfoArray.ElementAtOrDefault(1) != null ? clusterInfoArray[1] : "UNKNOWN";
    }

    #region Ui

    private void UpdateUi()
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            var unknownVaultSelection = new Vault() { Location = "UNKNOWN" };
            var list = _vaults.ToList();
            list.Insert(0, unknownVaultSelection);

            _mainWindowViewModel.VaultBindings.Vaults = list;
            _mainWindowViewModel.VaultBindings.VaultSelected = unknownVaultSelection;
        });
    }

    #endregion

    #region Load / Save local file data

    public void LoadFromFile()
    {
        var localFilePath = $"{AppDomain.CurrentDomain.BaseDirectory}{Settings.Default.VaultsFileName}";

        if (File.Exists(localFilePath))
        {
            try
            {
                var localFileString = File.ReadAllText(localFilePath, Encoding.UTF8);
                var vaults = JsonSerializer.Deserialize<List<Vault>>(localFileString) ?? new List<Vault>();
                _vaults = new ObservableCollection<Vault>(vaults);
                return;
            }
            catch (Exception e)
            {
                ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
                Log.Error(MethodBase.GetCurrentMethod()?.DeclaringType, e);
                _vaults = new ObservableCollection<Vault>();
                return;
            }
        }

        _vaults = new ObservableCollection<Vault>();
    }

    public void SaveInFile()
    {
        var localFilePath = $"{AppDomain.CurrentDomain.BaseDirectory}{Settings.Default.VaultsFileName}";

        try
        {
            var fileString = JsonSerializer.Serialize(_vaults);
            File.WriteAllText(localFilePath, fileString, Encoding.UTF8);
        }
        catch (Exception e)
        {
            ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
            Log.Error(MethodBase.GetCurrentMethod()?.DeclaringType, e);
        }
    }

    #endregion

    #region Helper methods

    private static VaultLocation GetVaultLocationByIndex(string index)
    {
        return Enum.TryParse(index, true, out VaultLocation location) ? location : VaultLocation.Unknown;
    }

    #endregion
}