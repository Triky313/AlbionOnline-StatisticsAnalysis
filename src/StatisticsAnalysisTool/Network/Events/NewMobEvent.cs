using StatisticsAnalysisTool.Common;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace StatisticsAnalysisTool.Network.Events;

public class NewMobEvent
{
    public NewMobEvent(Dictionary<byte, object> parameters)
    {
        ConsoleManager.WriteLineForNetworkHandler(GetType().Name, parameters);
            
        try
        {
            if (parameters.ContainsKey(0)) 
                ObjectId = parameters[0].ObjectToLong();

            if (parameters.ContainsKey(1)) 
                MobIndex = parameters[1].ObjectToInt();

            if (parameters.ContainsKey(11))
                MoveSpeed = parameters[11].ObjectToDouble();

            if (parameters.ContainsKey(13))
                HitPoints = parameters[13].ObjectToDouble();

            if (parameters.ContainsKey(14))
                HitPointsMax = parameters[14].ObjectToDouble();

            if (parameters.ContainsKey(17))
                Energy = parameters[17].ObjectToDouble();

            if (parameters.ContainsKey(18))
                EnergyMax = parameters[18].ObjectToDouble();

            if (parameters.ContainsKey(19))
                EnergyRegeneration = parameters[19].ObjectToDouble();
        }
        catch (Exception e)
        {
            ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
        }
    }

    public long? ObjectId { get; }
    public int MobIndex { get; }
    public double MoveSpeed { get; }
    public double HitPoints { get; }
    public double HitPointsMax { get; }
    public double Energy { get; }
    public double EnergyMax { get; }
    public double EnergyRegeneration { get; }
}