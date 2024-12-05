using StatisticsAnalysisTool.Cluster;
using StatisticsAnalysisTool.GameFileData;
using System;
using System.Collections.Generic;

namespace StatisticsAnalysisTool.StorageHistory;

public class InternalVault
{
    public InternalVault(long? objectId, string locationGuidString, List<Guid> containerGuidList, List<string> containerNames, List<string> containerIconTags, bool isGuildVault = false)
    {
        ObjectId = objectId;
        LocationGuidString = locationGuidString;

        if (isGuildVault)
        {
            GuildContainerGuidList = containerGuidList;
            GuildContainerNames = containerNames;
            GuildContainerIconTags = containerIconTags;
        }
        else
        {
            ContainerGuidList = containerGuidList;
            ContainerNames = containerNames;
            ContainerIconTags = containerIconTags;
        }
    }

    public long? ObjectId { get; set; }
    public string LocationGuidString { get; set; }
    public List<Guid> ContainerGuidList { get; set; }
    public List<string> ContainerNames { get; set; }
    public List<string> ContainerIconTags { get; set; }
    public List<Guid> GuildContainerGuidList { get; set; }
    public List<string> GuildContainerNames { get; set; }
    public List<string> GuildContainerIconTags { get; set; }
    public MapType MapType => WorldData.GetMapType(LocationGuidString);
    public string MainLocationIndex { get; set; }
    public string UniqueClusterName { get; set; }

    public bool CompareLocationGuidStringTails(string guidToCompare)
    {
        string[] array1 = GetTails(guidToCompare);
        string[] array2 = GetTails(LocationGuidString);

        for (int i = 0; i <= 3; i++)
        {
            if (array1[i] != array2[i])
            {
                return false;
            }
        }

        return true;
    }

    private static string[] GetTails(string locationGuidString)
    {
        string[] parts = locationGuidString.Split('@');
        string[] result = new string[7];

        int index = 0;
        for (int i = parts.Length - 1; i >= 0 && index < 7; i--, index++)
        {
            result[index] = parts[i];
        }
        
        for (; index < 7; index++)
        {
            result[index] = string.Empty;
        }

        return result;
    }
}