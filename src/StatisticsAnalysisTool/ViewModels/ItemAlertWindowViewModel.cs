using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Models;
using StatisticsAnalysisTool.Models.TranslationModel;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace StatisticsAnalysisTool.ViewModels;

public class ItemAlertWindowViewModel : BaseViewModel
{
    private readonly AlertInfos _alertInfos;
    private Color _cityColor;
    private string _cityName;
    private BitmapImage _icon;
    private string _itemName;
    private ItemAlertWindowTranslation _translation;

    public ItemAlertWindowViewModel(AlertInfos alertInfos)
    {
        _alertInfos = alertInfos;
        Init();
    }

    private void Init()
    {
        Translation = new ItemAlertWindowTranslation();
        ItemName = _alertInfos.Item.LocalizedName;
        CityName = _alertInfos.MarketResponse.City;
        Icon = _alertInfos.Item.Icon;
        CityColor = Locations.GetLocationColor(_alertInfos.MarketResponse.City.GetMarketLocationByLocationNameOrId());
    }

    #region Bindings

    public string ItemName
    {
        get => _itemName;
        set
        {
            _itemName = value;
            OnPropertyChanged();
        }
    }

    public string CityName
    {
        get => _cityName;
        set
        {
            _cityName = value;
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

    public Color CityColor
    {
        get => _cityColor;
        set
        {
            _cityColor = value;
            OnPropertyChanged();
        }
    }

    public ItemAlertWindowTranslation Translation
    {
        get => _translation;
        set
        {
            _translation = value;
            OnPropertyChanged();
        }
    }

    #endregion
}