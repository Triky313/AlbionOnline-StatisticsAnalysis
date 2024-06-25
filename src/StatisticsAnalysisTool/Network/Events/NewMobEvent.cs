using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.EventValidations;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace StatisticsAnalysisTool.Network.Events;

public class NewMobEvent
{
    public NewMobEvent(Dictionary<byte, object> parameters)
    {
        EventValidator.IsEventValid(EventCodes.NewMob, parameters);
        ConsoleManager.WriteLineForNetworkHandler(GetType().Name, parameters);

        try
        {
            if (parameters.TryGetValue(0, out object objectId))
            {
                ObjectId = objectId.ObjectToLong();
            }

            if (parameters.TryGetValue(1, out object mobIndex))
            {
                MobIndex = mobIndex.ObjectToInt();
            }

            if (parameters.TryGetValue(11, out object moveSpeed))
            {
                MoveSpeed = moveSpeed.ObjectToDouble();
            }

            if (parameters.TryGetValue(13, out object hitPoints))
            {
                HitPoints = hitPoints.ObjectToDouble();
            }

            if (parameters.TryGetValue(14, out object hitPointsMax))
            {
                HitPointsMax = hitPointsMax.ObjectToDouble();
            }

            if (parameters.TryGetValue(17, out object energy))
            {
                Energy = energy.ObjectToDouble();
            }

            if (parameters.TryGetValue(18, out object energyMax))
            {
                EnergyMax = energyMax.ObjectToDouble();
            }

            if (parameters.TryGetValue(19, out object energyRegeneration))
            {
                EnergyRegeneration = energyRegeneration.ObjectToDouble();
            }
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