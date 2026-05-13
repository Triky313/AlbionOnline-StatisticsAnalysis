using StatisticsAnalysisTool.Common.Converters;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace StatisticsAnalysisTool.GameFileData.Models;

public class CraftingModifiersRootObject
{
    [JsonPropertyName("craftingmodifiers")]
    public CraftingModifiersObject CraftingModifiers
    {
        get;
        set;
    }
}

public class CraftingModifiersObject
{
    [JsonPropertyName("craftinglocation")]
    [JsonConverter(typeof(SingleOrArrayConverter<CraftingModifierLocationObject>))]
    public List<CraftingModifierLocationObject> CraftingLocation
    {
        get;
        set;
    }
    = [];
}

public class CraftingModifierLocationObject
{
    [JsonPropertyName("@clusterid")]
    public string ClusterId
    {
        get;
        set;
    }

    [JsonPropertyName("@continent")]
    public string Continent
    {
        get;
        set;
    }

    [JsonPropertyName("@biome")]
    public string Biome
    {
        get;
        set;
    }

    [JsonPropertyName("@clusterquality")]
    public string ClusterQuality
    {
        get;
        set;
    }

    [JsonPropertyName("craftingbonus")]
    public CraftingModifierBonusObject CraftingBonus
    {
        get;
        set;
    }

    [JsonPropertyName("refiningbonus")]
    public CraftingModifierBonusObject RefiningBonus
    {
        get;
        set;
    }

    [JsonPropertyName("craftingmodifier")]
    [JsonConverter(typeof(SingleOrArrayConverter<CraftingModifierObject>))]
    public List<CraftingModifierObject> CraftingModifier
    {
        get;
        set;
    }
    = [];
}

public class CraftingModifierBonusObject
{
    [JsonPropertyName("@value")]
    public string Value
    {
        get;
        set;
    }

    [JsonPropertyName("@islandvalue")]
    public string IslandValue
    {
        get;
        set;
    }
}

public class CraftingModifierObject
{
    [JsonPropertyName("@name")]
    public string Name
    {
        get;
        set;
    }

    [JsonPropertyName("@value")]
    public string Value
    {
        get;
        set;
    }
}
