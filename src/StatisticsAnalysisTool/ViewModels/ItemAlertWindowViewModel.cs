using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Models;
using StatisticsAnalysisTool.Models.TranslationModel;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace StatisticsAnalysisTool.ViewModels;

public class ItemAlertWindowViewModel : BaseViewModel
{
    private readonly AlertInfos _alertInfos;

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
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public string CityName
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public BitmapImage Icon
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public Color CityColor
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public ItemAlertWindowTranslation Translation
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    #endregion
}