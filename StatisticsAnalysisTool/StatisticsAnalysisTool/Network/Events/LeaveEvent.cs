using Albion.Network;
using StatisticsAnalysisTool.Common;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace StatisticsAnalysisTool.Network.Events
{
    public class LeaveEvent : BaseEvent
    {
        public LeaveEvent(Dictionary<byte, object> parameters) : base(parameters)
        {
            ConsoleManager.WriteLineForNetworkHandler(GetType().Name, parameters);

            try
            {
                if (parameters.ContainsKey(0)) ObjectId = parameters[0].ObjectToLong();
            }
            catch (Exception e)
            {
                ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod().DeclaringType, e);
            }
        }

        public long? ObjectId { get; }
    }
}