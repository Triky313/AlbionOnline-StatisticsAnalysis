using System;
using System.Collections.Generic;

namespace StatisticsAnalysisTool.EstimatedMarketValue;

public class EstimatedMarketValueObject
{
    public string UniqueItemName { get; set; }
    public List<EstQualityValue> EstimatedMarketValues { get; set; }
}