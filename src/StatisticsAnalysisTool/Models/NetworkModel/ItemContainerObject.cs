using System;
using System.Collections.Generic;

namespace StatisticsAnalysisTool.Models.NetworkModel;

public class ItemContainerObject
{
    public ItemContainerObject(long? objectId, Guid containerGuid, List<long> slotItemIds)
    {
        ObjectId = objectId;
        ContainerGuid = containerGuid;
        SlotItemIds = slotItemIds;

        LastUpdate = DateTime.UtcNow;
    }

    public DateTime LastUpdate { get; }
    public long? ObjectId { get; set; }
    public Guid ContainerGuid { get; set; }
    public List<long> SlotItemIds { get; set; }
}