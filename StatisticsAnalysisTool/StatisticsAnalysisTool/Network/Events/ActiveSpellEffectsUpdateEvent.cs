using Albion.Network;
using StatisticsAnalysisTool.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace StatisticsAnalysisTool.Network.Events
{
    public class ActiveSpellEffectsUpdateEvent : BaseEvent
    {
        public ActiveSpellEffectsUpdateEvent(Dictionary<byte, object> parameters) : base(parameters)
        {
            try
            {
                if (parameters.ContainsKey(0))
                {
                    CauserId = parameters[0].ObjectToLong();
                }

                if (parameters.ContainsKey(1))
                {
                    SpellIndex = parameters[1].ObjectToInt();
                }
            }
            catch (Exception e)
            {
                Debug.Print(e.Message);
            }
        }

        public long? CauserId { get; }
        public int SpellIndex { get; }
    }
}