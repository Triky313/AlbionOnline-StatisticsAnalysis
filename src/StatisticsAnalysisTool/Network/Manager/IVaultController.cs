using System;
using System.Threading.Tasks;
using StatisticsAnalysisTool.Models.NetworkModel;

namespace StatisticsAnalysisTool.Network.Manager;

public interface IVaultController
{
    event Action OnVaultsChange;
    event Action OnVaultsRemove;
    void SetCurrentVault(VaultInfo vaultInfo);
    void AddContainer(ItemContainerObject newContainerObject);
    void Add(DiscoveredItem item);
    void ResetVaultContainer();
    void ResetDiscoveredItems();
    void ResetCurrentVaultInfo();
    void RemoveVault(Vault vault);
    Task LoadFromFileAsync();
    Task SaveInFileAsync();

}