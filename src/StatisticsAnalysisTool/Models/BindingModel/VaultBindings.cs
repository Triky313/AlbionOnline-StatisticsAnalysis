using System;
using StatisticsAnalysisTool.Models.NetworkModel;
using StatisticsAnalysisTool.Properties;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;

namespace StatisticsAnalysisTool.Models.BindingModel;

public class VaultBindings : INotifyPropertyChanged
{
    private List<ContainerItem> _vaultContainerContent;
    private List<Vault> _vaults;
    private Vault _vaultSelected;
    private List<VaultContainer> _vaultContainer;
    private VaultContainer _vaultContainerSelected;
    private Visibility _lastUpdateVisibility = Visibility.Hidden;
    private DateTime _lastUpdate;

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
            VaultContainer = _vaultSelected.VaultContainer.FindAll(x => x.LastUpdate.Ticks > 0).OrderBy(y => y.Name).ToList();
            OnPropertyChanged();
        }
    }

    public List<VaultContainer> VaultContainer
    {
        get => _vaultContainer;
        set
        {
            _vaultContainer = value;
            OnPropertyChanged();
        }
    }

    public VaultContainer VaultContainerSelected
    {
        get => _vaultContainerSelected;
        set
        {
            _vaultContainerSelected = value;
            VaultContainerContent = _vaultContainer?.FirstOrDefault(x => x.Guid == _vaultContainerSelected.Guid)?.Items ?? new List<ContainerItem>();
            LastUpdate = _vaultContainerSelected?.LastUpdate ?? new DateTime(0);
            LastUpdateVisibility = _vaultContainerSelected?.LastUpdate.Ticks <= 1 ? Visibility.Hidden : Visibility.Visible;
            OnPropertyChanged();
        }
    }

    public List<ContainerItem> VaultContainerContent
    {
        get => _vaultContainerContent;
        set
        {
            _vaultContainerContent = value;
            OnPropertyChanged();
        }
    }

    public Visibility LastUpdateVisibility
    {
        get => _lastUpdateVisibility;
        set
        {
            _lastUpdateVisibility = value;
            OnPropertyChanged();
        }
    }

    public DateTime LastUpdate
    {
        get => _lastUpdate;
        set
        {
            _lastUpdate = value;
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