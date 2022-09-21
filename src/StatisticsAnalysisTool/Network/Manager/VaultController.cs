using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Models;
using StatisticsAnalysisTool.Models.NetworkModel;
using StatisticsAnalysisTool.Properties;
using StatisticsAnalysisTool.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;

namespace StatisticsAnalysisTool.Network.Manager;

public class VaultController
{
    private readonly MainWindowViewModel _mainWindowViewModel;
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

    public VaultController(MainWindowViewModel mainWindowViewModel)
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

        await foreach (var vault in Vaults.ToAsyncEnumerable())
        {
            vaultSearchItem.AddRange(from vaultContainer in vault.VaultContainer
                                     from item in vaultContainer.Items
                                     where item?.Quantity > 0
                                     select new VaultSearchItem()
                                     {
                                         Item = item.Item,
                                         Location = vault.Location,
                                         MainLocationIndex = vault.MainLocationIndex,
                                         MapType = vault.MapType,
                                         Quantity = item.Quantity,
                                         VaultContainerName = vaultContainer.Name
                                     });
        }

        Application.Current.Dispatcher.Invoke(() =>
        {
            _mainWindowViewModel.VaultBindings.VaultSearchList.Clear();
            _mainWindowViewModel.VaultBindings.VaultSearchList.AddRange(vaultSearchItem);
            _mainWindowViewModel?.VaultBindings?.VaultSearchCollectionView?.Refresh();
        });
    }

    #endregion

    #region Load / Save local file data

    public async Task LoadFromFileAsync()
    {
        Vaults = await FileController.LoadAsync<ObservableCollection<Vault>>($"{AppDomain.CurrentDomain.BaseDirectory}{Settings.Default.VaultsFileName}");
    }

    public async Task SaveInFileAsync()
    {
        await FileController.SaveAsync(Vaults, $"{AppDomain.CurrentDomain.BaseDirectory}{Settings.Default.VaultsFileName}");
    }

    #endregion
}