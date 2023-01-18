using log4net;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.GameData;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using StatisticsAnalysisTool.Enumerations;

namespace StatisticsAnalysisTool.Network.Operations.Responses;

public class JoinResponse
{
    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);
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

            if (parameters.ContainsKey(23)) CurrentFocusPoints = parameters[23].ObjectToDouble();

            if (parameters.ContainsKey(24)) MaxCurrentFocusPoints = parameters[24].ObjectToDouble();

            if (parameters.ContainsKey(28)) Silver = FixPoint.FromInternalValue(parameters[28].ObjectToLong() ?? 0);

            if (parameters.ContainsKey(29)) Gold = FixPoint.FromInternalValue(parameters[29].ObjectToLong() ?? 0);

            if (parameters.ContainsKey(32)) LearningPoints = FixPoint.FromInternalValue(parameters[32].ObjectToLong() ?? 0);

            if (parameters.ContainsKey(36)) Reputation = parameters[36].ObjectToDouble();

            if (parameters.ContainsKey(38) && parameters[38] is long[] { Length: > 1 } reSpecArray)
            {
                ReSpecPoints = FixPoint.FromInternalValue(reSpecArray[1]);
            }

            if (parameters.ContainsKey(47))
            {
                InteractGuid = parameters[47].ObjectToGuid();
                ConsoleManager.WriteLineForMessage(MethodBase.GetCurrentMethod()?.DeclaringType, $"Local interact object Guid: {InteractGuid}", ConsoleColorType.EventMapChangeColor);
            }

            if (parameters.ContainsKey(51))
            {
                GuildName = string.IsNullOrEmpty(parameters[51].ToString()) ? string.Empty : parameters[51].ToString();
            }

            if (parameters.ContainsKey(58))
            {
                MainMapIndex = string.IsNullOrEmpty(parameters[58].ToString()) ? string.Empty : parameters[58].ToString();
            }

            if (parameters.ContainsKey(61))
            {
                PlayTimeInSeconds = parameters[61].ObjectToInt();
            }

            if (parameters.ContainsKey(71))
            {
                AllianceName = string.IsNullOrEmpty(parameters[71].ToString()) ? string.Empty : parameters[71].ToString();
            }

            if (parameters.ContainsKey(88))
            {
                IsReSpecActive = parameters[88].ObjectToBool();
            }
        }
        catch (Exception e)
        {
            Log.Debug(nameof(JoinResponse), e);
        }
    }
}