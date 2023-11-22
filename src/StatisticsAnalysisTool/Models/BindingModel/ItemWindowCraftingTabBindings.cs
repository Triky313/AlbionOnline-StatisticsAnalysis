using StatisticsAnalysisTool.ViewModels;
using System.Windows;

namespace StatisticsAnalysisTool.Models.BindingModel;

public class ItemWindowCraftingTabBindings : BaseViewModel
{
    private Visibility _craftingInfoPopupVisibility = Visibility.Hidden;

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
}