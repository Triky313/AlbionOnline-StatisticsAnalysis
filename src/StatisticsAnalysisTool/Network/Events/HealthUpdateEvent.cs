using System;
using System.Collections.Generic;
using System.Reflection;
using log4net;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Network.Time;

namespace StatisticsAnalysisTool.Network.Events;

public class HealthUpdateEvent
{
    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);
    public long CauserId;
    public int CausingSpellType;
    public EffectOrigin EffectOrigin;
    public EffectType EffectType;
    public double HealthChange;
    public double NewHealthValue;

    public long AffectedObjectId;
    public GameTimeStamp TimeStamp;

    public HealthUpdateEvent(Dictionary<byte, object> parameters)
    {
        ConsoleManager.WriteLineForNetworkHandler(GetType().Name, parameters);

        try
        {
            if (parameters.TryGetValue(0, out object affectedObjectId))
            {
                AffectedObjectId = affectedObjectId.ObjectToLong() ?? throw new ArgumentNullException();
            }

            if (parameters.TryGetValue(1, out object timestamp))
            {
                TimeStamp = new GameTimeStamp(timestamp.ObjectToLong() ?? 0);
            }

            if (parameters.TryGetValue(2, out object healthChange))
            {
                HealthChange = healthChange.ObjectToDouble();
            }

            if (parameters.TryGetValue(3, out object newHealthValue))
            {
                NewHealthValue = newHealthValue.ObjectToDouble();
            }

            if (parameters.TryGetValue(4, out object effectType))
            {
                EffectType = (EffectType) (effectType as byte? ?? 0);
            }

            if (parameters.TryGetValue(5, out object effectOrigin))
            {
                EffectOrigin = (EffectOrigin) (effectOrigin as byte? ?? 0);
            }

            if (parameters.TryGetValue(6, out object causerId))
            {
                CauserId = causerId.ObjectToLong() ?? throw new ArgumentNullException();
            }

            if (parameters.TryGetValue(7, out object causingSpellType))
            {
                CausingSpellType = causingSpellType.ObjectToShort();
            }
        }
        catch (ArgumentNullException ex)
        {
            ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, ex);
            Log.Error(MethodBase.GetCurrentMethod()?.DeclaringType, ex);
        }
        catch (Exception e)
        {
            ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
            Log.Error(MethodBase.GetCurrentMethod()?.DeclaringType, e);
        }
    }
}