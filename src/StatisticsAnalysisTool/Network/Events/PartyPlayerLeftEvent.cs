using System;
using System.Collections.Generic;
using System.Reflection;
using StatisticsAnalysisTool.Common;

namespace StatisticsAnalysisTool.Network.Events
{
    public class PartyPlayerLeftEvent
    {
        public Guid? UserGuid;

        public PartyPlayerLeftEvent(Dictionary<byte, object> parameters)
        {
            ConsoleManager.WriteLineForNetworkHandler(GetType().Name, parameters);

            try
            {
                if (parameters.ContainsKey(1))
                {
                    UserGuid = parameters[1].ObjectToGuid();
                }
            }
            catch (Exception e)
            {
                ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
            }
        }
    }
}