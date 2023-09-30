using StatisticsAnalysisTool.Cluster;
using StatisticsAnalysisTool.GameFileData;
using StatisticsAnalysisTool.Localization;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace StatisticsAnalysisTool.Models.NetworkModel;

public class Vault
{
    public string Location { get; set; }
    public string MainLocationIndex { get; set; }
    public MapType MapType { get; set; }
    public List<VaultContainer> VaultContainer { get; set; } = new();

    [JsonIgnore]
    public string MainLocation => WorldData.GetUniqueNameOrDefault(MainLocationIndex);
    [JsonIgnore]
    public string LocationDisplayString
    {
        get
        {
            if (MapType is MapType.Hideout)
            {
                return $"{Location} ({LanguageController.Translation("HIDEOUT")}) - {MainLocation}";
            }

            if (MapType is MapType.Island)
            {
                return $"{Location} ({LanguageController.Translation("ISLAND")})";
            }

            return Location;
        }
    }
}