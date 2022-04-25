using log4net;
using StatisticsAnalysisTool.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using StatisticsAnalysisTool.GameData;

namespace StatisticsAnalysisTool.Network.Operations.Responses
{
    public class ChangeClusterResponse
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);

        public string Index;
        public Guid? Guid;
        public MapType MapType = MapType.Unknown;
        public string WorldMapDataType;
        public string IslandName;
        public byte[] DungeonInformation;
        public string MainClusterIndex;

        public ChangeClusterResponse(Dictionary<byte, object> parameters)
        {
            ConsoleManager.WriteLineForNetworkHandler(GetType().Name, parameters);

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

                if (parameters.ContainsKey(3))
                {
                    DungeonInformation = ((byte[])parameters[3]).ToArray();
                }
            }
            catch (Exception e)
            {
                Log.Debug(nameof(ChangeClusterResponse), e);
            }
        }
    }
}