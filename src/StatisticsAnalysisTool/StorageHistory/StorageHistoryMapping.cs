using System.Collections.ObjectModel;
using System.Linq;

namespace StatisticsAnalysisTool.StorageHistory;

public static class StorageHistoryMapping
{
    public static VaultDto Mapping(Vault vault)
    {
        return new VaultDto()
        {
            Location = vault.Location,
            MainLocationIndex = vault.MainLocationIndex,
            MapType = vault.MapType,
            VaultContainer = vault.VaultContainer.Select(Mapping).ToList()
        };
    }

    public static Vault Mapping(VaultDto vaultDto)
    {
        return new Vault()
        {
            Location = vaultDto.Location,
            MainLocationIndex = vaultDto.MainLocationIndex,
            MapType = vaultDto.MapType,
            VaultContainer = vaultDto.VaultContainer.Select(Mapping).ToList()
        };
    }

    public static VaultContainerDto Mapping(VaultContainer vaultContainer)
    {
        return new VaultContainerDto()
        {
            LastUpdate = vaultContainer.LastUpdate,
            Guid = vaultContainer.Guid,
            Name = vaultContainer.Name,
            Icon = vaultContainer.Icon,
            Items = vaultContainer.Items.Select(Mapping).ToList()
        };
    }

    public static VaultContainer Mapping(VaultContainerDto vaultContainerDto)
    {
        return new VaultContainer()
        {
            LastUpdate = vaultContainerDto.LastUpdate,
            Guid = vaultContainerDto.Guid,
            Name = vaultContainerDto.Name,
            Icon = vaultContainerDto.Icon,
            Items = new ObservableCollection<ContainerItem>(vaultContainerDto.Items.Select(Mapping))
        };
    }

    public static ContainerItemDto Mapping(ContainerItem containerItem)
    {
        return new ContainerItemDto()
        {
            ItemIndex = containerItem.ItemIndex,
            Quantity = containerItem.Quantity
        };
    }

    public static ContainerItem Mapping(ContainerItemDto containerItemDto)
    {
        return new ContainerItem()
        {
            ItemIndex = containerItemDto.ItemIndex,
            Quantity = containerItemDto.Quantity
        };
    }
}