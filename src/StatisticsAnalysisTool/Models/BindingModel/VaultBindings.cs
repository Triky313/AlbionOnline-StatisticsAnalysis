using StatisticsAnalysisTool.Models.NetworkModel;
using StatisticsAnalysisTool.Properties;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace StatisticsAnalysisTool.Models.BindingModel;

public class VaultBindings : INotifyPropertyChanged
{
    private VaultContainerContent _vaultContainerContent;
    private List<Vault> _vaults;
    private Vault _vaultSelected;

    public List<Vault> Vaults
    {
        get => _vaults;
        set
        {
            _vaults = value;
            OnPropertyChanged();
        }
    }

    public Vault VaultSelected
    {
        get => _vaultSelected;
        set
        {
            _vaultSelected = value;
            OnPropertyChanged();
        }
    }

    public VaultContainerContent VaultContainerContent
    {
        get => _vaultContainerContent;
        set
        {
            _vaultContainerContent = value;
            OnPropertyChanged();
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    [NotifyPropertyChangedInvocator]
    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}