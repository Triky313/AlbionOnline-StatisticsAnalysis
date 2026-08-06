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
        Title = _alertInfos.AlertType switch
        {
            Alert.ItemAlertType.PriceThreshold => Translation.PriceAlertTitle,
            Alert.ItemAlertType.MarketAvailability => Translation.AvailabilityAlertTitle,
            Alert.ItemAlertType.BlackMarketBuyOrder => Translation.BlackMarketBuyOrderAlertTitle,
            _ => string.Empty
        };
        LeadText = _alertInfos.AlertType switch
        {
            Alert.ItemAlertType.PriceThreshold => Translation.ThePriceOf,
            Alert.ItemAlertType.MarketAvailability => Translation.AMarketOfferFor,
            Alert.ItemAlertType.BlackMarketBuyOrder => Translation.HighestBlackMarketBuyOrderFor,
            _ => string.Empty
        };
        ResultText = _alertInfos.AlertType switch
        {
            Alert.ItemAlertType.PriceThreshold => Translation.HasBeenUndercut,
            Alert.ItemAlertType.MarketAvailability => Translation.WasFound,
            Alert.ItemAlertType.BlackMarketBuyOrder => Translation.ReachedMinimumPrice,
            _ => string.Empty
        };
        ItemName = _alertInfos.Item.LocalizedName;
        CityName = _alertInfos.MarketResponse.City;
        Price = _alertInfos.AlertType == Alert.ItemAlertType.BlackMarketBuyOrder
            ? _alertInfos.MarketResponse.BuyPriceMax
            : _alertInfos.MarketResponse.SellPriceMin;
        Icon = _alertInfos.Item.Icon;
        CityColor = Locations.GetLocationColor(_alertInfos.MarketResponse.City.GetMarketLocationByLocationNameOrId());
    }

    #region Bindings

    public string Title
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public string LeadText
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public string ResultText
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

    public ulong Price
    {
        get;
        set
        {
            field = value;
            OnPropertyChanged();
        }
    }

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