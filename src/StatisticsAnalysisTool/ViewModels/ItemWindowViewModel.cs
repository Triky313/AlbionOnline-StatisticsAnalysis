using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Models;
using StatisticsAnalysisTool.Models.TranslationModel;
using StatisticsAnalysisTool.Views;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Timers;
using System.Windows;
using System.Windows.Media.Imaging;

namespace StatisticsAnalysisTool.ViewModels;

public class ItemWindowViewModel : INotifyPropertyChanged
{
    private Visibility _errorBarVisibility;
    private Item _item;
    private string _titleName;
    private string _itemTierLevel;
    private BitmapImage _icon;
    private ItemWindowTranslation _translation = new();
    private bool _refreshSpin;
    private bool _isAutoUpdateActive;
    private readonly Timer _timer = new();

    public enum Error
    {
        NoPrices,
        NoItemInfo,
        GeneralError,
        ToManyRequests
    }

    public void Init(ItemWindow itemWindow, Item item)
    {
        Icon = null;
        TitleName = "-";
        ItemTierLevel = string.Empty;

        Item = item;

        ItemTierLevel = Item?.Tier != -1 && Item?.Level != -1 ? $"T{Item?.Tier}.{Item?.Level}" : string.Empty;

        var localizedName = ItemController.LocalizedName(Item?.LocalizedNames, null, Item?.UniqueName);

        itemWindow.Icon = Item?.Icon;
        TitleName = localizedName;
        
        IsAutoUpdateActive = true;
    }

    //private async void InitializeItemData(Item item)
    //{
    //    //InformationLoadingImageVisibility = Visibility.Visible;

    //    Icon = null;
    //    ItemName = "-";
    //    ItemTierLevel = string.Empty;

    //    if (item == null)
    //    {
    //        SetErrorValues(Error.NoItemInfo);
    //        return;
    //    }

    //    ItemTierLevel = Item?.Tier != -1 && Item?.Level != -1 ? $"T{Item?.Tier}.{Item?.Level}" : string.Empty;
    //    //InitExtraItemInformation();
    //    //await InitCraftingTabAsync();

    //    await Application.Current.Dispatcher.InvokeAsync(() =>
    //    {
    //        _itemWindow.Icon = null;
    //        _itemWindow.Title = "-";
    //    });

    //    if (Application.Current.Dispatcher == null)
    //    {
    //        SetErrorValues(Error.GeneralError);
    //        return;
    //    }

    //    var localizedName = ItemController.LocalizedName(Item?.LocalizedNames, null, Item?.UniqueName);

    //    Icon = item.Icon;
    //    ItemName = localizedName;

    //    await _itemWindow.Dispatcher.InvokeAsync(() =>
    //    {
    //        _itemWindow.Icon = item.Icon;
    //        _itemWindow.Title = $"{localizedName} (T{item.Tier})";
    //    });

    //    InitTimer();
    //    IsAutoUpdateActive = true;
    //    UpdateValues(null, null);

    //    //InformationLoadingImageVisibility = Visibility.Hidden;
    //}

    public void AutoUpdateSwitcher()
    {
        IsAutoUpdateActive = !IsAutoUpdateActive;
        RefreshSpin = IsAutoUpdateActive;
    }

    #region Bindings

    public Item Item
    {
        get => _item;
        set
        {
            _item = value;
            OnPropertyChanged();
        }
    }

    public string TitleName
    {
        get => _titleName;
        set
        {
            _titleName = value;
            OnPropertyChanged();
        }
    }

    public string ItemTierLevel
    {
        get => _itemTierLevel;
        set
        {
            _itemTierLevel = value;
            OnPropertyChanged();
        }
    }

    public BitmapImage Icon
    {
        get => _icon;
        set
        {
            _icon = value;
            OnPropertyChanged();
        }
    }

    public bool RefreshSpin
    {
        get => _refreshSpin;
        set
        {
            _refreshSpin = value;
            OnPropertyChanged();
        }
    }

    public Visibility ErrorBarVisibility
    {
        get => _errorBarVisibility;
        set
        {
            _errorBarVisibility = value;
            OnPropertyChanged();
        }
    }

    public bool IsAutoUpdateActive
    {
        get => _isAutoUpdateActive;
        set
        {
            _isAutoUpdateActive = value;

            _timer.Enabled = _isAutoUpdateActive;
            RefreshSpin = IsAutoUpdateActive;
            OnPropertyChanged();
        }
    }

    public ItemWindowTranslation Translation
    {
        get => _translation;
        set
        {
            _translation = value;
            OnPropertyChanged();
        }
    }

    #endregion

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}