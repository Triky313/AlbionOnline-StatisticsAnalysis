using StatisticsAnalysisTool.GameFileData.Converter;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace StatisticsAnalysisTool.GameFileData.Models;

public class MiniMapMarkers
{
    [JsonPropertyName("marker")]
    [JsonConverter(typeof(MarkerToMarkersListConverter))]
    public List<Marker> Marker { get; set; }
}