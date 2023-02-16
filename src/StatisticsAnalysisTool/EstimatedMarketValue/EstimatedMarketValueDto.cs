using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace StatisticsAnalysisTool.EstimatedMarketValue;

public class EstimatedMarketValueDto
{
    [JsonPropertyName("Name")]
    public string UniqueItemName { get; set; }
    [JsonPropertyName("EstValues")]
    public List<EstQualityValueDto> EstimatedMarketValueDtos { get; set; }
}