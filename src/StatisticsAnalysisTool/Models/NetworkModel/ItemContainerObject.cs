using System;
using System.Collections.Generic;

namespace StatisticsAnalysisTool.Models.NetworkModel;

public class ItemContainerObject
{
    public ItemContainerObject(long? objectId, Guid containerGuid, List<int> slotItemId)
    {
        ObjectId = objectId;
        ContainerGuid = containerGuid;
        SlotItemId = slotItemId;

        LastUpdate = DateTime.UtcNow;
    }

    public DateTime LastUpdate { get; }
    public long? ObjectId { get; set; }
    public Guid ContainerGuid { get; set; }
    public List<int> SlotItemId { get; set; }
}