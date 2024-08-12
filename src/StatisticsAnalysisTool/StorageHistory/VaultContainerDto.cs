using System;
using System.Collections.Generic;

namespace StatisticsAnalysisTool.StorageHistory;

public class VaultContainerDto
{
    public DateTime LastUpdate { get; set; }
    public Guid Guid { get; set; }
    public string Name { get; set; }
    public string Icon { get; set; }
    public List<ContainerItemDto> Items { get; set; } = new();
}