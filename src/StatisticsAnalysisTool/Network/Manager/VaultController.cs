using StatisticsAnalysisTool.Cluster;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Localization;
using StatisticsAnalysisTool.Models;
using StatisticsAnalysisTool.Models.NetworkModel;
using StatisticsAnalysisTool.Properties;
using StatisticsAnalysisTool.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;

namespace StatisticsAnalysisTool.Network.Manager;

public class VaultController : IVaultController
{
    private readonly MainWindowViewModelOld _mainWindowViewModel;
    private VaultInfo _currentVaultInfo;
    private readonly List<DiscoveredItem> _discoveredItems = new();
    private readonly List<ItemContainerObject> _vaultContainer = new();
    private ObservableCollection<Vault> _vault = new();

    private ObservableCollection<Vault> Vaults
    {
        get => _vault;
        set
        {
            _vault = value;
            OnVaultsChange?.Invoke();
        }
    }

    public VaultController(MainWindowViewModelOld mainWindowViewModel)
    {
        _mainWindowViewModel = mainWindowViewModel;

        OnVaultsChange += UpdateUi;
        OnVaultsChange += UpdateSearchListUiAsync;
        OnVaultsRemove += UpdateSearchListUiAsync;
    }

    public event Action OnVaultsChange;
    public event Action OnVaultsRemove;

    public void SetCurrentVault(VaultInfo vaultInfo)
    {
        if (vaultInfo == null)
        {
            _currentVaultInfo = null;
            return;
        }

        vaultInfo.UniqueClusterName = ClusterController.CurrentCluster.UniqueClusterName;
        vaultInfo.MainLocationIndex = ClusterController.CurrentCluster.MainClusterIndex;

        if (string.IsNullOrEmpty(vaultInfo.UniqueClusterName))
        {
            _currentVaultInfo = null;
            return;
        }

        _currentVaultInfo = vaultInfo;
    }

    public void AddContainer(ItemContainerObject newContainerObject)
    {
        if (newContainerObject?.PrivateContainerGuid == default)
        {
            return;
        }

        var container = _vaultContainer.FirstOrDefault(x => x.PrivateContainerGuid == newContainerObject.PrivateContainerGuid);
        if (container != null)
        {
            _vaultContainer.Remove(container);
        }

        _vaultContainer.Add(newContainerObject);
        ParseVault();
        GetRepairCostsOfContainer(newContainerObject);
    }

    public void Add(DiscoveredItem item)
    {
        if (_discoveredItems.Exists(x => x.ObjectId == item.ObjectId))
        {
            return;
        }

        _discoveredItems.Add(item);
    }

    public void ResetVaultContainer()
    {
        _vaultContainer.Clear();
    }

    public void ResetDiscoveredItems()
    {
        _discoveredItems.Clear();
    }

    public void ResetCurrentVaultInfo()
    {
        _currentVaultInfo = null;
    }

    public void RemoveVault(Vault vault)
    {
        if (vault == null)
        {
            return;
        }

        foreach (var removableContainer in vault.VaultContainer.Select(container => _vaultContainer?.FirstOrDefault(x => x?.PrivateContainerGuid == container?.Guid)))
        {
            _vaultContainer?.Remove(removableContainer);
        }

        _vault.Remove(vault);
        OnVaultsRemove?.Invoke();

        UpdateUi();
    }

    private void ParseVault()
    {
        if (_currentVaultInfo == null)
        {
            return;
        }

        var removableVaultInfo = Vaults.FirstOrDefault(x => x.Location == _currentVaultInfo.UniqueClusterName);

        if (removableVaultInfo != null)
        {
            Vaults.Remove(removableVaultInfo);
        }

        var vault = new Vault()
        {
            Location = _currentVaultInfo.UniqueClusterName,
            MainLocationIndex = _currentVaultInfo.MainLocationIndex,
            MapType = _currentVaultInfo.MapType
        };

        try
        {
            for (var i = 0; i < _currentVaultInfo.ContainerGuidList.Count; i++)
            {
                var vaultContainer = new VaultContainer()
                {
                    Guid = _currentVaultInfo.ContainerGuidList[i],
                    Icon = _currentVaultInfo.ContainerIconTags[i],
                    Name = (_currentVaultInfo.ContainerNames[i] == "@BUILDINGS_T1_BANK") ? LanguageController.Translation("BANK") : _currentVaultInfo.ContainerNames[i]
                };

                var itemContainer = _vaultContainer.FirstOrDefault(x => x.PrivateContainerGuid == vaultContainer.Guid);
                if (itemContainer != null)
                {
                    vaultContainer.LastUpdate = itemContainer.LastUpdate;
                    SetItemsToVaultContainer(itemContainer, vaultContainer, _discoveredItems);
                }

                vault.VaultContainer.Add(vaultContainer);
            }

            Vaults.Add(vault);
            OnVaultsChange?.Invoke();
        }
        catch (Exception e)
        {
            ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
        }
    }

    private void SetItemsToVaultContainer(ItemContainerObject containerObject, VaultContainer vaultContainer, List<DiscoveredItem> discoveredItems)
    {
        foreach (var slotItemId in containerObject?.SlotItemIds ?? new List<long>())
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

    #region Ui

    private void UpdateUi()
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            _mainWindowViewModel.VaultBindings.Vaults = Vaults.ToList();
        });
    }

    private async void UpdateSearchListUiAsync()
    {
        var vaultSearchItem = new List<VaultSearchItem>();

        await foreach (var vault in Vaults.ToList().ToAsyncEnumerable())
        {
            var tempItems = new List<VaultSearchItem>();

            foreach (var vaultContainer in vault.VaultContainer)
            {
                foreach (var item in vaultContainer.Items)
                {
                    if (!(item?.Quantity > 0))
                    {
                        continue;
                    }

                    var searchItem = new VaultSearchItem()
                    {
                        Item = item.Item,
                        Location = vault.Location,
                        MainLocationIndex = vault.MainLocationIndex,
                        MapType = vault.MapType,
                        Quantity = item.Quantity,
                        VaultContainerName = vaultContainer.Name
                    };

                    tempItems.Add(searchItem);
                }
            }

            vaultSearchItem.AddRange(tempItems);
        }

        Application.Current.Dispatcher.Invoke(() =>
        {
            _mainWindowViewModel.VaultBindings.VaultSearchList.Clear();
            _mainWindowViewModel.VaultBindings.VaultSearchList.AddRange(vaultSearchItem);
            _mainWindowViewModel?.VaultBindings?.VaultSearchCollectionView?.Refresh();
        });
    }

    #endregion

    #region Repair costs calculation

    private const double RepairCostModifier = 6.0606d;

    private void GetRepairCostsOfContainer(ItemContainerObject newContainerObject)
    {
        var discoveredItems = new List<DiscoveredItem>();
        foreach (var slotItemId in newContainerObject.SlotItemIds)
        {
            var slotItem = _discoveredItems.FirstOrDefault(x => x.ObjectId == slotItemId);
            if (slotItem == null)
            {
                continue;
            }
            discoveredItems.Add(slotItem);
        }

        var totalCosts = 0d;

        foreach (var discoveredItem in discoveredItems)
        {
            var item = ItemController.GetItemByIndex(discoveredItem.ItemIndex);

            if (item == null)
            {
                return;
            }

            var itemValue = ItemController.GetItemValue(item.FullItemInformation, item.Level);

            double lostDurability;
            if (discoveredItem.CurrentDurability.IntegerValue <= 0)
            {
                lostDurability = 0;
            }
            else
            {
                var fullDurability = ItemController.GetDurability(item.FullItemInformation, item.Level);
                lostDurability = (fullDurability - discoveredItem.CurrentDurability.IntegerValue) / fullDurability * 100;
            }

            if (lostDurability <= 1)
            {
                continue;
            }

            var repairCosts = Math.Round((itemValue / RepairCostModifier * lostDurability), MidpointRounding.ToPositiveInfinity);

            totalCosts += repairCosts * discoveredItem.Quantity;
        }

        try
        {
            _mainWindowViewModel.DashboardBindings.RepairCostsChest = Convert.ToInt32(totalCosts);
        }
        catch
        {
            // ignore
        }
    }

    #endregion

    #region Load / Save local file data

    public async Task LoadFromFileAsync()
    {
        FileController.TransferFileIfExistFromOldPathToUserDataDirectory(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Settings.Default.VaultsFileName));
        Vaults = await FileController.LoadAsync<ObservableRangeCollection<Vault>>(
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Settings.Default.UserDataDirectoryName, Settings.Default.VaultsFileName));
    }

    public async Task SaveInFileAsync()
    {
        DirectoryController.CreateDirectoryWhenNotExists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Settings.Default.UserDataDirectoryName));
        await FileController.SaveAsync(Vaults, Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Settings.Default.UserDataDirectoryName, Settings.Default.VaultsFileName));
        Debug.Print("Vault saved");
    }

    #endregion
}