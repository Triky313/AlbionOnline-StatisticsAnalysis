using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Enumerations;
using System;
using System.Globalization;

namespace StatisticsAnalysisTool.Crafting;

public class CraftingSellPriceOption
{
    public MarketLocation Location
    {
        get;
        init;
    }

    public string LocationName
    {
        get;
        init;
    }

    public decimal Price
    {
        get;
        init;
    }

    public DateTime PriceDate
    {
        get;
        init;
    }

    public ValueTimeStatus PriceDateStatus
    {
        get;
        init;
    }
    = ValueTimeStatus.NoValue;

    public string DisplayText
    {
        get
        {
            return $"{LocationName}: {Price.ToString("N0", CultureInfo.CurrentCulture)}";
        }
    }

    public string UpdateDateTimeText
    {
        get
        {
            return PriceDateStatus == ValueTimeStatus.NoValue
                ? string.Empty
                : PriceDate.ToString("G", CultureInfo.CurrentCulture);
        }
    }
}