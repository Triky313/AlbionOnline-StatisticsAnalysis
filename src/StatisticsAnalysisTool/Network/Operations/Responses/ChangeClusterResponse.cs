using Serilog;
using StatisticsAnalysisTool.Cluster;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.GameFileData;
using System;
using System.Collections.Generic;
using System.Linq;
using StatisticsAnalysisTool.Dungeon;

namespace StatisticsAnalysisTool.Network.Operations.Responses;

public class ChangeClusterResponse
{
    public string Index;
    public Guid? Guid;
    public MapType MapType = MapType.Unknown;
    public string WorldMapDataType;
    public string IslandName;
    public byte[] DungeonInformation;
    public string MainClusterIndex;
    public Tier MistsDungeonTier;

    public ChangeClusterResponse(Dictionary<byte, object> parameters)
    {
        ConsoleManager.WriteLineForMessage(GetType().Name, parameters, ConsoleColorType.EventMapChangeColor);

        try
        {
            if (parameters.ContainsKey(0))
            {
                var clusterString = string.IsNullOrEmpty(parameters[0].ToString()) ? string.Empty : parameters[0].ToString();
                var splitName = clusterString?.Split(new[] { "@" }, StringSplitOptions.RemoveEmptyEntries);

                if (splitName?.Length > 1 && clusterString.ToLower().Contains('@'))
                {
                    Guid = WorldData.GetMapGuid(clusterString);
                    MapType = WorldData.GetMapType(clusterString);

                    if (MapType is MapType.Hideout && splitName.Length >= 3)
                    {
                        MainClusterIndex = string.IsNullOrEmpty(splitName[1]) ? string.Empty : splitName[1];
                    }
                }
                else
                {
                    Index = clusterString;
                }
            }

            if (parameters.ContainsKey(1))
            {
                WorldMapDataType = string.IsNullOrEmpty(parameters[1].ToString()) ? string.Empty : parameters[1].ToString();
            }

            if (parameters.ContainsKey(2))
            {
                IslandName = string.IsNullOrEmpty(parameters[2].ToString()) ? string.Empty : parameters[2].ToString();
            }

            if (parameters.TryGetValue(3, out object dungeonInfo))
            {
                DungeonInformation = ((byte[]) dungeonInfo).ToArray();
            }

            if (parameters.TryGetValue(5, out object mistsDungeonTier))
            {
                MistsDungeonTier = WorldData.GetTier(mistsDungeonTier.ObjectToInt());
            }
        }
        catch (Exception e)
        {
            Log.Debug(e, nameof(ChangeClusterResponse));
        }
    }
}