using StatisticsAnalysisTool.Enumerations;
using System.Collections.Generic;

namespace StatisticsAnalysisTool.Models.NetworkModel;

public class Vault
{
    public VaultLocation Location { get; set; }
    public List<VaultContainer> VaultContainer { get; set; } = new ();
}