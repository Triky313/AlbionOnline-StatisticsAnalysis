using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.EstimatedMarketValue;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Windows;
using System.Windows.Media.Imaging;
using ShopCategory = StatisticsAnalysisTool.Common.ShopCategory;

namespace StatisticsAnalysisTool.Models;

public class Item
{
    private BitmapImage _icon;
    public string LocalizationNameVariable { get; set; }
    public string LocalizationDescriptionVariable { get; set; }
    public LocalizedNames LocalizedNames { get; set; }
    public int Index { get; set; }
    public string UniqueName { get; set; }
    [JsonIgnore]
    public string LocalizedNameAndEnglish => LanguageController.CurrentCultureInfo.TextInfo.CultureName.ToUpper() == "EN-US"
        ? $"{ItemController.LocalizedName(LocalizedNames, null, UniqueName)}{GetUniqueNameIfDebug()}"
        : $"{ItemController.LocalizedName(LocalizedNames, null, UniqueName)}" +
          $"\n{ItemController.LocalizedName(LocalizedNames, "EN-US", string.Empty)}{GetUniqueNameIfDebug()}";
    public string LocalizedName => ItemController.LocalizedName(LocalizedNames, null, UniqueName);

    public int Level => ItemController.GetItemLevel(UniqueName);
    public int Tier => ItemController.GetItemTier(this);

    public string TierLevelString
    {
        get
        {
            var tier = (Tier is <= 8 and >= 1) ? Tier.ToString() : "?";
            return $"T{tier}.{Level}";
        }
    }

    [JsonIgnore]
    public BitmapImage Icon => Application.Current.Dispatcher.Invoke(() => _icon ??= ImageController.GetItemImage(UniqueName));

    public object FullItemInformation { get; set; }
    public ShopCategory ShopCategory { get; set; }
    public ShopSubCategory ShopShopSubCategory1 { get; set; }
    public int AlertModeMinSellPriceIsUndercutPrice { get; set; }
    public bool IsAlertActive { get; set; }
    public bool IsFavorite { get; set; }

    [JsonIgnore]
    public List<EstQualityValue> EstimatedMarketValues { get; set; }

    [JsonIgnore]
    public string AverageEstQualityValueString => Utilities.LongWithCulture(EstimatedMarketValueController.CalculateNearestToAverage(EstimatedMarketValues).MarketValue.IntegerValue);
    [JsonIgnore]
    public string LastEstimatedUpdateTimeString =>
        $"{LanguageController.Translation("LAST_ESTIMATED_VALUE_UPDATE")}: {EstimatedMarketValueController.CalculateNearestToAverage(EstimatedMarketValues).Timestamp.DateTimeToLastUpdateTime()}";
    [JsonIgnore]
    public PastTime EstimatedMarketValueStatus => EstimatedMarketValueController.CalculateNearestToAverage(EstimatedMarketValues).Timestamp.GetPastTimeEnumByDateTime();
    [JsonIgnore]
    public string TranslationEstMarketValue => LanguageController.Translation("EST_MARKET_VALUE");
    private string GetUniqueNameIfDebug()
    {
#if DEBUG
        return $"\n{UniqueName}";
#else
            return string.Empty;
#endif
    }
}