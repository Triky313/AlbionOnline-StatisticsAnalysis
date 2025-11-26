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

            if (parameters.ContainsKey(2))
            {
                Username = parameters[2].ToString();
                DebugConsole.WriteInfo(MethodBase.GetCurrentMethod()?.DeclaringType, $"Username: {Username}", "#0279be");
            }

            if (parameters.ContainsKey(8))
            {
                MapIndex = parameters[8].ToString();
                MapType = WorldData.GetMapType(MapIndex);
                MapGuid = WorldData.GetMapGuid(MapIndex);
                DebugConsole.WriteInfo(MethodBase.GetCurrentMethod()?.DeclaringType, $"MapIndex: {MapIndex} | MapType: {MapType} | MapGuid: {MapGuid}", "#0279be");
            }

            if (parameters.ContainsKey(26))
            {
                CurrentFocusPoints = parameters[26].ObjectToDouble();
            }

            if (parameters.ContainsKey(27))
            {
                MaxCurrentFocusPoints = parameters[27].ObjectToDouble();
            }

            if (parameters.ContainsKey(32))
            {
                Silver = FixPoint.FromInternalValue(parameters[32].ObjectToLong() ?? 0);
            }

            if (parameters.ContainsKey(33))
            {
                Gold = FixPoint.FromInternalValue(parameters[33].ObjectToLong() ?? 0);
            }

            if (parameters.ContainsKey(36))
            {
                LearningPoints = FixPoint.FromInternalValue(parameters[36].ObjectToLong() ?? 0);
            }

            if (parameters.ContainsKey(40))
            {
                Reputation = parameters[40].ObjectToDouble();
            }

            if (parameters.ContainsKey(42) && parameters[42] is long[] { Length: > 1 } reSpecArray)
            {
                ReSpecPoints = FixPoint.FromInternalValue(reSpecArray[1]);
            }

            if (parameters.ContainsKey(53))
            {
                InteractGuid = parameters[53].ObjectToGuid();
                DebugConsole.WriteInfo(MethodBase.GetCurrentMethod()?.DeclaringType, $"Local interact object Guid: {InteractGuid}", "#0279be");
            }

            if (parameters.ContainsKey(57))
            {
                GuildName = string.IsNullOrEmpty(parameters[57].ToString()) ? string.Empty : parameters[57].ToString();
            }

            if (parameters.ContainsKey(65))
            {
                MainMapIndex = string.IsNullOrEmpty(parameters[65].ToString()) ? string.Empty : parameters[65].ToString();
            }

            // Temporarily removed until value is found
            PlayTimeInSeconds = 0;

            if (parameters.ContainsKey(78))
            {
                AllianceName = string.IsNullOrEmpty(parameters[78].ToString()) ? string.Empty : parameters[78].ToString();
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
}