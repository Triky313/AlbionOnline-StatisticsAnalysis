using StatisticsAnalysisTool.Properties;
using StatisticsAnalysisTool.ViewModels;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace StatisticsAnalysisTool.Models.BindingModel;

public class ItemWindowCraftingTabBindings : INotifyPropertyChanged
{
    private readonly ItemWindowViewModel _itemWindowViewModel;
    private Visibility _craftingInfoPopupVisibility = Visibility.Hidden;

    public ItemWindowCraftingTabBindings(ItemWindowViewModel itemWindowViewModel)
    {
        _itemWindowViewModel = itemWindowViewModel;
    }

    public void SetInfoPopupVisibility()
    {
        CraftingInfoPopupVisibility = CraftingInfoPopupVisibility == Visibility.Hidden ? Visibility.Visible : Visibility.Hidden;
    }

    #region Bindings

    public Visibility CraftingInfoPopupVisibility
    {
        get => _craftingInfoPopupVisibility;
        set
        {
            _craftingInfoPopupVisibility = value;
            OnPropertyChanged();
        }
    }

    #endregion

    public event PropertyChangedEventHandler PropertyChanged;

    [NotifyPropertyChangedInvocator]
    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}