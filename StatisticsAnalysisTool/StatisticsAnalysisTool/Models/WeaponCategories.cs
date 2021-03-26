using Newtonsoft.Json;

namespace StatisticsAnalysisTool.Models
{
    public class WeaponCategories
    {
        [JsonProperty("id")] public string Id { get; set; }

        [JsonProperty("name")] public string Name { get; set; }
    }
}