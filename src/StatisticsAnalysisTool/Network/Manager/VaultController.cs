using log4net;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Models.NetworkModel;
using StatisticsAnalysisTool.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using StatisticsAnalysisTool.Common;

namespace StatisticsAnalysisTool.Network.Manager;

public class VaultController
{
    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);

    private readonly TrackingController _trackingController;
    private readonly MainWindowViewModel _mainWindowViewModel;
    private VaultInfo _currentVaultInfo;
    private readonly List<DiscoveredItem> _discoveredItems = new ();
    private readonly List<ItemContainerObject> _vaultContainer = new ();
    private readonly List<Vault> _vaults = new ();

    public VaultController(TrackingController trackingController, MainWindowViewModel mainWindowViewModel)
    {
        _trackingController = trackingController;
        _mainWindowViewModel = mainWindowViewModel;
    }

    public void SetCurrentVault(VaultInfo vaultInfo)
    {
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
        if (_currentVaultInfo == null)
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
                    Item = null,
                    Quantity = 0
                });

                continue;
            }

            var item = ItemController.GetItemByIndex(slotItem.ItemIndex);
            vaultContainer.Items.Add(new ContainerItem()
            {
                Item = item,
                Quantity = slotItem.Quantity
            });
        }
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