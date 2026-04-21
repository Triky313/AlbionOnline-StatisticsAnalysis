using Serilog;
using StatisticsAnalysisTool.Cluster;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Diagnostics;
using StatisticsAnalysisTool.GameFileData;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

namespace StatisticsAnalysisTool.Network.Operations.Responses;

public class JoinResponse
{
    public long? UserObjectId;
    public Guid? UserGuid { get; }
    public string Username { get; }
    public string MapIndex { get; }
    public Guid? MapGuid { get; }
    public MapType MapType { get; }
    public double CurrentFocusPoints { get; }
    public double MaxCurrentFocusPoints { get; }
    public FixPoint LearningPoints { get; }
    public double Reputation { get; }
    public FixPoint ReSpecPoints { get; }
    public FixPoint Silver { get; }
    public FixPoint Gold { get; }
    public Guid? InteractGuid { get; }
    public string GuildName { get; }
    public string MainMapIndex { get; set; }
    public int PlayTimeInSeconds { get; set; }
    public string AllianceName { get; }
    public bool IsReSpecActive { get; }

    public JoinResponse(Dictionary<byte, object> parameters)
    {
        try
        {
            Debug.Print("---------- UserInformation (Response) ----------");
            DebugConsole.WriteInfo(MethodBase.GetCurrentMethod()?.DeclaringType, "---------- UserInformation (Response) ----------", "#0279be");

            UserObjectId = null;
            if (parameters.ContainsKey(0))
            {
                UserObjectId = parameters[0].ObjectToLong();
                Debug.Print($"Local user ObjectId: {UserObjectId}");
                DebugConsole.WriteInfo(MethodBase.GetCurrentMethod()?.DeclaringType, $"Local user ObjectId: {UserObjectId}", "#0279be");
            }

            if (parameters.ContainsKey(1))
            {
                UserGuid = parameters[1].ObjectToGuid();
                Debug.Print($"Local user Guid: {UserGuid}");
                DebugConsole.WriteInfo(MethodBase.GetCurrentMethod()?.DeclaringType, $"Local user Guid: {UserGuid}", "#0279be");
            }

            if (parameters.TryGetValue(2, out object username))
            {
                Username = username.ToString();
                DebugConsole.WriteInfo(MethodBase.GetCurrentMethod()?.DeclaringType, $"Username: {Username}", "#0279be");
            }

            if (parameters.TryGetValue(8, out object mapIndex))
            {
                MapIndex = mapIndex.ToString();
                MapType = WorldData.GetMapType(MapIndex);
                MapGuid = WorldData.GetMapGuid(MapIndex);
                DebugConsole.WriteInfo(MethodBase.GetCurrentMethod()?.DeclaringType, $"MapIndex: {MapIndex} | MapType: {MapType} | MapGuid: {MapGuid}", "#0279be");
            }

            if (parameters.ContainsKey(27))
            {
                CurrentFocusPoints = parameters[27].ObjectToDouble();
            }

            if (parameters.ContainsKey(28))
            {
                MaxCurrentFocusPoints = parameters[28].ObjectToDouble();
            }

            if (parameters.ContainsKey(33))
            {
                Silver = FixPoint.FromInternalValue(parameters[33].ObjectToLong() ?? 0);
            }

            if (parameters.ContainsKey(34))
            {
                Gold = FixPoint.FromInternalValue(parameters[34].ObjectToLong() ?? 0);
            }

            if (parameters.ContainsKey(37))
            {
                LearningPoints = FixPoint.FromInternalValue(parameters[37].ObjectToLong() ?? 0);
            }

            if (parameters.ContainsKey(41))
            {
                Reputation = parameters[41].ObjectToDouble();
            }

            if (parameters.ContainsKey(43) && parameters[43] is long[] { Length: > 1 } reSpecArray)
            {
                ReSpecPoints = FixPoint.FromInternalValue(reSpecArray[1]);
            }

            if (parameters.ContainsKey(54))
            {
                InteractGuid = parameters[54].ObjectToGuid();
                DebugConsole.WriteInfo(MethodBase.GetCurrentMethod()?.DeclaringType, $"Local interact object Guid: {InteractGuid}", "#0279be");
            }

            if (parameters.ContainsKey(58))
            {
                GuildName = string.IsNullOrEmpty(parameters[58].ToString()) ? string.Empty : parameters[58].ToString();
            }

            MainMapIndex = GetMainMapIndex(parameters, MapIndex, MapType);
            DebugConsole.WriteInfo(MethodBase.GetCurrentMethod()?.DeclaringType, $"MainMapIndex: {MainMapIndex}", "#0279be");

            // Temporarily removed until value is found
            PlayTimeInSeconds = 0;

            if (parameters.ContainsKey(79))
            {
                AllianceName = string.IsNullOrEmpty(parameters[79].ToString()) ? string.Empty : parameters[79].ToString();
            }

            if (parameters.ContainsKey(98))
            {
                IsReSpecActive = parameters[98].ObjectToBool();
            }
        }
        catch (Exception e)
        {
            Log.Error(e, "{message}", MethodBase.GetCurrentMethod()?.DeclaringType);
        }
    }

    private static string GetMainMapIndex(Dictionary<byte, object> parameters, string mapIndex, MapType mapType)
    {
        if (IsInstancedMap(mapType))
        {
            var sourceClusterIndex = GetStringParameter(parameters, 65);
            if (!string.IsNullOrWhiteSpace(sourceClusterIndex))
            {
                return sourceClusterIndex;
            }
        }

        return mapIndex ?? string.Empty;
    }

    private static bool IsInstancedMap(MapType mapType)
    {
        return mapType is MapType.RandomDungeon
            or MapType.CorruptedDungeon
            or MapType.HellGate
            or MapType.Expedition
            or MapType.Mists
            or MapType.MistsDungeon
            or MapType.AbyssalDepths;
    }

    private static string GetStringParameter(Dictionary<byte, object> parameters, byte key)
    {
        if (!parameters.TryGetValue(key, out object value))
        {
            return string.Empty;
        }

        return value?.ToString() ?? string.Empty;
    }
}