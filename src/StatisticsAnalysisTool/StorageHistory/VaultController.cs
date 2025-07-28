using Serilog;
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
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;

namespace StatisticsAnalysisTool.StorageHistory;

public class VaultController
{
    private readonly MainWindowViewModel _mainWindowViewModel;
    private InternalVault _currentInternalVault;
    private readonly List<DiscoveredItem> _discoveredItems = new();
    private readonly List<ItemContainerObject> _internalItemContainers = new();
    private readonly VaultBindings _vaultBindings;

    public VaultController(MainWindowViewModel mainWindowViewModel)
    {
        _mainWindowViewModel = mainWindowViewModel;
        _vaultBindings = _mainWindowViewModel.VaultBindings;

        OnVaultsChange += UpdateSearchListUiAsync;
        OnVaultsRemove += UpdateSearchListUiAsync;
    }

    public event Action OnVaultsChange;
    public event Action OnVaultsRemove;

    public void SetOrAddCurrentVault(InternalVault internalVault)
    {
        if (internalVault == null)
        {
            _currentInternalVault = null;
            return;
        }

        internalVault.UniqueClusterName = ClusterController.CurrentCluster.UniqueClusterName;
        internalVault.MainLocationIndex = ClusterController.CurrentCluster.MainClusterIndex;

        if (string.IsNullOrEmpty(internalVault.UniqueClusterName))
        {
            _currentInternalVault = null;
            return;
        }

        if (_currentInternalVault is not null && _currentInternalVault.CompareLocationGuidStringTails(internalVault.LocationGuidString))
        {
            _currentInternalVault.ContainerGuidList = internalVault.ContainerGuidList;
            _currentInternalVault.ContainerIconTags = internalVault.ContainerIconTags;
            _currentInternalVault.ContainerNames = internalVault.ContainerNames;
        }
        else
        {
            _currentInternalVault = internalVault;
        }
    }

    public void SetOrAddCurrentGuildVault(InternalVault internalVault)
    {
        if (internalVault == null)
        {
            _currentInternalVault = null;
            return;
        }

        internalVault.UniqueClusterName = ClusterController.CurrentCluster.UniqueClusterName;
        internalVault.MainLocationIndex = ClusterController.CurrentCluster.MainClusterIndex;

        if (string.IsNullOrEmpty(internalVault.UniqueClusterName))
        {
            _currentInternalVault = null;
            return;
        }

        if (_currentInternalVault is not null && _currentInternalVault.CompareLocationGuidStringTails(internalVault.LocationGuidString))
        {
            _currentInternalVault.GuildContainerGuidList = internalVault.GuildContainerGuidList;
            _currentInternalVault.GuildContainerIconTags = internalVault.GuildContainerIconTags;
            _currentInternalVault.GuildContainerNames = internalVault.GuildContainerNames;
        }
        else
        {
            _currentInternalVault = internalVault;
        }
    }

    public void AddContainer(ItemContainerObject newContainerObject)
    {
        if (newContainerObject?.PrivateContainerGuid == default)
        {
            return;
        }

        var container = _internalItemContainers.FirstOrDefault(x => x.PrivateContainerGuid == newContainerObject.PrivateContainerGuid);
        if (container != null)
        {
            _internalItemContainers.Remove(container);
        }

        SetRepairCostsOfContainer(newContainerObject);

        _internalItemContainers.Add(newContainerObject);
        UpdateUi();
    }

    public void AddDiscoveredItem(DiscoveredItem item)
    {
        if (_discoveredItems.Exists(x => x.ObjectId == item.ObjectId))
        {
            return;
        }

        _discoveredItems.Add(item);
    }

    public void ResetInternalVaultContainer()
    {
        _internalItemContainers.Clear();
    }

    public void ResetDiscoveredItems()
    {
        _discoveredItems.Clear();
    }

    public void ResetCurrentInternalVault()
    {
        _currentInternalVault = null;
    }

    public void RemoveVault(Vault vault)
    {
        if (vault == null)
        {
            return;
        }

        foreach (var removableContainer in vault.VaultContainer.Select(container => _internalItemContainers?.FirstOrDefault(x => x?.PrivateContainerGuid == container?.Guid)))
        {
            _internalItemContainers?.Remove(removableContainer);
        }

        Application.Current.Dispatcher.Invoke(() =>
        {
            _vaultBindings.Vaults.Remove(vault);
        });

        OnVaultsRemove?.Invoke();
    }

    #region Ui

    private void UpdateUi()
    {
        if (_currentInternalVault == null)
        {
            return;
        }

        var vaultSelected = _vaultBindings.VaultSelected;
        var vaultContainerSelectedGuid = _vaultBindings.VaultContainerSelected?.Guid;

        var currentVault = _vaultBindings.Vaults.FirstOrDefault(x => x.Location == _currentInternalVault.UniqueClusterName);
        if (currentVault is not null)
        {
            var vaultContainers = PreparationVaultContainerForUi(_internalItemContainers);
            var containers = vaultContainers.ToList();
            if (containers.Any())
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    currentVault.VaultContainer.Clear();
                    currentVault.VaultContainer.AddRange(containers);
                });
            }
        }
        else
        {
            var vault = new Vault()
            {
                Location = _currentInternalVault.UniqueClusterName,
                MainLocationIndex = _currentInternalVault.MainLocationIndex,
                MapType = _currentInternalVault.MapType
            };

            try
            {
                var vaultContainers = PreparationVaultContainerForUi(_internalItemContainers);
                var containers = vaultContainers.ToList();
                if (containers.Any())
                {
                    vault.VaultContainer.AddRange(containers);
                }

                Application.Current.Dispatcher.Invoke(() =>
                {
                    int insertIndex = 0;
                    for (int i = 0; i < _vaultBindings.Vaults.Count; i++)
                    {
                        if (string.Compare(_vaultBindings.Vaults[i].LocationDisplayString, vault.LocationDisplayString, StringComparison.Ordinal) > 0)
                        {
                            insertIndex = i;
                            break;
                        }
                        insertIndex = i + 1;
                    }

                    _vaultBindings.Vaults.Insert(insertIndex, vault);
                });
            }
            catch (Exception e)
            {
                ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
            }
        }

        _vaultBindings.VaultSelected = _vaultBindings.Vaults.Contains(vaultSelected) ? vaultSelected : default;
        _vaultBindings.VaultContainerSelected = _vaultBindings.VaultContainer.FirstOrDefault(x => x?.Guid == vaultContainerSelectedGuid);

        OnVaultsChange?.Invoke();
    }

    private IEnumerable<VaultContainer> PreparationVaultContainerForUi(IReadOnlyCollection<ItemContainerObject> itemContainers)
    {
        var vaultContainers = new List<VaultContainer>();

        for (var i = 0; i < _currentInternalVault?.ContainerGuidList?.Count; i++)
        {
            var vaultContainer = new VaultContainer()
            {
                Guid = _currentInternalVault.ContainerGuidList[i],
                Icon = _currentInternalVault.ContainerIconTags[i],
                Name = _currentInternalVault.ContainerNames[i] == "@BUILDINGS_T1_BANK" ? LocalizationController.Translation("BANK") : _currentInternalVault.ContainerNames[i]
            };

            var itemContainer = itemContainers.FirstOrDefault(x => x.PrivateContainerGuid == vaultContainer.Guid);
            if (itemContainer != null)
            {
                vaultContainer.LastUpdate = itemContainer.LastUpdate;
                SetItemsToVaultContainerForUi(itemContainer, vaultContainer, _discoveredItems);
                vaultContainer.RepairCosts = itemContainer.RepairCosts;
            }

            vaultContainers.Add(vaultContainer);
        }

        for (var i = 0; i < _currentInternalVault?.GuildContainerGuidList?.Count; i++)
        {
            var vaultContainer = new VaultContainer()
            {
                Guid = _currentInternalVault.GuildContainerGuidList[i],
                Icon = _currentInternalVault.GuildContainerIconTags[i],
                Name = _currentInternalVault.GuildContainerNames[i] == "@BUILDINGS_T1_BANK" ? LocalizationController.Translation("BANK") : _currentInternalVault.GuildContainerNames[i],
                IsGuildContainer = true
            };

            var itemContainer = itemContainers.FirstOrDefault(x => x.PrivateContainerGuid == vaultContainer.Guid);
            if (itemContainer != null)
            {
                vaultContainer.LastUpdate = itemContainer.LastUpdate;
                SetItemsToVaultContainerForUi(itemContainer, vaultContainer, _discoveredItems);
                vaultContainer.RepairCosts = itemContainer.RepairCosts;
            }

            vaultContainers.Add(vaultContainer);
        }

        return vaultContainers;
    }

    private void SetItemsToVaultContainerForUi(ItemContainerObject containerObject, VaultContainer vaultContainer, IReadOnlyCollection<DiscoveredItem> discoveredItems)
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

    private async void UpdateSearchListUiAsync()
    {
        var vaultSearchItem = new List<VaultSearchItem>();

        await foreach (var vault in _vaultBindings.Vaults.ToList().ToAsyncEnumerable())
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
            _vaultBindings.VaultSearchList.Clear();
            _vaultBindings.VaultSearchList.AddRange(vaultSearchItem);
            _mainWindowViewModel?.VaultBindings?.VaultSearchCollectionView?.Refresh();
        });
    }

    #endregion

    #region Repair costs calculation

    private const double RepairCostModifier = 6.0606d;

    private void SetRepairCostsOfContainer(ItemContainerObject newContainerObject)
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

            var repairCosts = Math.Round(itemValue / RepairCostModifier * lostDurability, MidpointRounding.ToPositiveInfinity);

            totalCosts += repairCosts * discoveredItem.Quantity;
        }

        try
        {
            newContainerObject.RepairCosts = totalCosts;
        }
        catch (Exception e)
        {
            ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
        }
    }

    #endregion
    
    #region Load / Save local file data

    public async Task LoadFromFileAsync()
    {
        FileController.TransferFileIfExistFromOldPathToUserDataDirectory(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Settings.Default.VaultsFileName));

        var vaultDtos = await FileController.LoadAsync<List<VaultDto>>(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Settings.Default.UserDataDirectoryName, Settings.Default.VaultsFileName));
        _vaultBindings.Vaults = new ObservableCollection<Vault>(vaultDtos.Select(StorageHistoryMapping.Mapping));
    }

    public async Task SaveInFileAsync()
    {
        DirectoryController.CreateDirectoryWhenNotExists(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Settings.Default.UserDataDirectoryName));

        var vaultDtosToSave = _vaultBindings.Vaults
            ?.ToList()
            .Select(StorageHistoryMapping.Mapping) ?? Enumerable.Empty<VaultDto>();

        await FileController.SaveAsync(vaultDtosToSave,
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Settings.Default.UserDataDirectoryName, Settings.Default.VaultsFileName));
        Log.Information("Vault saved");
    }

    #endregion
}