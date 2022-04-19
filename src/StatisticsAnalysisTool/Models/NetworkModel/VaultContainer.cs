using System;
using System.Collections.Generic;

namespace StatisticsAnalysisTool.Models.NetworkModel;

public class VaultContainer
{
    public Guid Guid { get; set; }
    public string Name { get; set; }
    public string Icon { get; set; }
    public List<ContainerItem> Items { get; set; } = new ();
}