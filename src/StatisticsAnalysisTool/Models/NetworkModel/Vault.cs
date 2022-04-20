using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.GameData;
using StatisticsAnalysisTool.Network.Manager;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace StatisticsAnalysisTool.Models.NetworkModel;

public class Vault
{
    public string Location { get; set; }
    [JsonIgnore]
    public VaultLocation VaultLocation => VaultController.GetVaultLocation(Location);
    [JsonIgnore]
    public string LocationName => WorldData.GetUniqueNameOrDefault(VaultController.GetVaultLocationIndex(Location));
    public List<VaultContainer> VaultContainer { get; set; } = new();
}