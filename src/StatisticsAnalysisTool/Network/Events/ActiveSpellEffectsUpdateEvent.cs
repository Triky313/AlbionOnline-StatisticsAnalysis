using StatisticsAnalysisTool.Common;
using System;
using System.Collections.Generic;
using System.Reflection;
using StatisticsAnalysisTool.Diagnostics;

namespace StatisticsAnalysisTool.Network.Events;

public class ActiveSpellEffectsUpdateEvent
{
    public ActiveSpellEffectsUpdateEvent(Dictionary<byte, object> parameters)
    {
        try
        {
            if (parameters.ContainsKey(0))
            {
                CauserId = parameters[0].ObjectToLong();
            }

            if (parameters.ContainsKey(1))
            {
                var valueType = parameters[1].GetType();
                if (valueType.IsArray && typeof(short[]).Name == valueType.Name)
                {
                    var spells = ((short[]) parameters[1]).ToDictionary();
                    SpellIndex = spells[0].ObjectToInt();
                }
            }
        }
        catch (Exception e)
        {
            DebugConsole.WriteError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
        }
    }

    public long? CauserId { get; }
    public int SpellIndex { get; }
}