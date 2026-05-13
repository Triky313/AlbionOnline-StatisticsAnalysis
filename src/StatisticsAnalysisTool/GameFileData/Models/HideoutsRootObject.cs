using StatisticsAnalysisTool.Common.Converters;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace StatisticsAnalysisTool.GameFileData.Models;

public class HideoutsRootObject
{
    [JsonPropertyName("hideouts")]
    public HideoutsObject Hideouts
    {
        get;
        set;
    }
}

public class HideoutsObject
{
    [JsonPropertyName("hideout")]
    public HideoutObject Hideout
    {
        get;
        set;
    }
}

public class HideoutObject
{
    [JsonPropertyName("powerlevels")]
    public HideoutPowerLevelsObject PowerLevels
    {
        get;
        set;
    }
}

public class HideoutPowerLevelsObject
{
    [JsonPropertyName("powerlevel")]
    [JsonConverter(typeof(SingleOrArrayConverter<HideoutPowerLevelObject>))]
    public List<HideoutPowerLevelObject> PowerLevel
    {
        get;
        set;
    }
    = [];
}

public class HideoutPowerLevelObject
{
    [JsonPropertyName("@level")]
    public string Level
    {
        get;
        set;
    }

    [JsonPropertyName("@generalistcraftingbonus")]
    public string GeneralistCraftingBonus
    {
        get;
        set;
    }

    [JsonPropertyName("@specialistcraftingbonus")]
    public string SpecialistCraftingBonus
    {
        get;
        set;
    }
}