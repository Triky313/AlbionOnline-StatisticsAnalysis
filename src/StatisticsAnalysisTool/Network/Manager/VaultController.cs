using log4net;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Models.NetworkModel;
using StatisticsAnalysisTool.Properties;
using StatisticsAnalysisTool.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Windows;

namespace StatisticsAnalysisTool.Network.Manager;

public class VaultController
{
    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);
    
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
    }

    public event Action OnVaultsChange;

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

                var itemContainer = _vaultContainer.FirstOrDefault(x => x.ContainerGuid == vaultContainer.Guid);
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

    #region Ui

    private void UpdateUi()
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            _mainWindowViewModel.VaultBindings.Vaults = Vaults.ToList();
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
                Vaults = new ObservableCollection<Vault>(vaults);
                return;
            }
            catch (Exception e)
            {
                ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
                Log.Error(MethodBase.GetCurrentMethod()?.DeclaringType, e);
                Vaults = new ObservableCollection<Vault>();
                return;
            }
        }

        Vaults = new ObservableCollection<Vault>();
    }

    public void SaveInFile()
    {
        var localFilePath = $"{AppDomain.CurrentDomain.BaseDirectory}{Settings.Default.VaultsFileName}";

        try
        {
            var fileString = JsonSerializer.Serialize(Vaults);
            File.WriteAllText(localFilePath, fileString, Encoding.UTF8);
        }
        catch (Exception e)
        {
            ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
            Log.Error(MethodBase.GetCurrentMethod()?.DeclaringType, e);
        }
    }

    #endregion
}