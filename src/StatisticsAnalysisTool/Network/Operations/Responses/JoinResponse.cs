using Serilog;
using StatisticsAnalysisTool.Cluster;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Enumerations;
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

    public JoinResponse(IReadOnlyDictionary<byte, object> parameters)
    {
        try
        {
            Debug.Print("---------- UserInformation (Response) ----------");
            ConsoleManager.WriteLineForMessage(MethodBase.GetCurrentMethod()?.DeclaringType, "---------- UserInformation (Response) ----------", ConsoleColorType.EventMapChangeColor);

            UserObjectId = null;
            if (parameters.ContainsKey(0))
            {
                UserObjectId = parameters[0].ObjectToLong();
                Debug.Print($"Local user ObjectId: {UserObjectId}");
                ConsoleManager.WriteLineForMessage(MethodBase.GetCurrentMethod()?.DeclaringType, $"Local user ObjectId: {UserObjectId}", ConsoleColorType.EventMapChangeColor);
            }

            if (parameters.ContainsKey(1))
            {
                UserGuid = parameters[1].ObjectToGuid();
                Debug.Print($"Local user Guid: {UserGuid}");
                ConsoleManager.WriteLineForMessage(MethodBase.GetCurrentMethod()?.DeclaringType, $"Local user Guid: {UserGuid}", ConsoleColorType.EventMapChangeColor);
            }

            if (parameters.ContainsKey(2))
            {
                Username = parameters[2].ToString();
            }

            if (parameters.ContainsKey(8))
            {
                MapIndex = parameters[8].ToString();
                MapType = WorldData.GetMapType(MapIndex);
                MapGuid = WorldData.GetMapGuid(MapIndex);
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

            if (parameters.ContainsKey(52))
            {
                InteractGuid = parameters[52].ObjectToGuid();
                ConsoleManager.WriteLineForMessage(MethodBase.GetCurrentMethod()?.DeclaringType, $"Local interact object Guid: {InteractGuid}", ConsoleColorType.EventMapChangeColor);
            }

            if (parameters.ContainsKey(56))
            {
                GuildName = string.IsNullOrEmpty(parameters[56].ToString()) ? string.Empty : parameters[56].ToString();
            }

            if (parameters.ContainsKey(63))
            {
                MainMapIndex = string.IsNullOrEmpty(parameters[63].ToString()) ? string.Empty : parameters[63].ToString();
            }

            // Temporarily removed until value is found
            PlayTimeInSeconds = 0;

            if (parameters.ContainsKey(76))
            {
                AllianceName = string.IsNullOrEmpty(parameters[76].ToString()) ? string.Empty : parameters[76].ToString();
            }

            if (parameters.ContainsKey(93))
            {
                IsReSpecActive = parameters[93].ObjectToBool();
            }
        }
        catch (Exception e)
        {
            Log.Error(e, "{message}", MethodBase.GetCurrentMethod()?.DeclaringType);
        }
    }
}