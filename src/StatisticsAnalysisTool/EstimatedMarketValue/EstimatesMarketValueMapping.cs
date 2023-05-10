using System;
using System.Linq;
using StatisticsAnalysisTool.Common;

namespace StatisticsAnalysisTool.EstimatedMarketValue;

public static class EstimatesMarketValueMapping
{
    public static EstimatedMarketValueDto Mapping(EstimatedMarketValueObject estMarketValueObject)
    {
        return new EstimatedMarketValueDto
        {
            UniqueItemName = estMarketValueObject.UniqueItemName,
            EstimatedMarketValueDtos = estMarketValueObject.EstimatedMarketValues.Select(EstQualityValueMapping).ToList()
        };
    }

    public static EstimatedMarketValueObject Mapping(EstimatedMarketValueDto estMarketValueDto)
    {
        return new EstimatedMarketValueObject
        {
            UniqueItemName = estMarketValueDto.UniqueItemName,
            EstimatedMarketValues = estMarketValueDto.EstimatedMarketValueDtos.Select(EstQualityValueDtoMapping).ToList()
        };
    }

    public static EstQualityValueDto EstQualityValueMapping(EstQualityValue estQualityValue)
    {
        return new EstQualityValueDto
        {
            Ticks = estQualityValue.Timestamp.Ticks,
            MarketValueInternal = estQualityValue.MarketValue.InternalValue,
            Quality = estQualityValue.Quality
        };
    }

    public static EstQualityValue EstQualityValueDtoMapping(EstQualityValueDto estQualityValueDto)
    {
        return new EstQualityValue
        {
            Timestamp = new DateTime(estQualityValueDto.Ticks),
            MarketValue = FixPoint.FromInternalValue(estQualityValueDto.MarketValueInternal),
            Quality = estQualityValueDto.Quality
        };
    }
}