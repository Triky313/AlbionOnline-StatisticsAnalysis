using Albion.Network;
using StatisticsAnalysisTool.Common;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace StatisticsAnalysisTool.Network.Handler
{
    public class PartyChangedOrderEvent : BaseEvent
    {
        public Guid? UserGuid;
        public string Username;

        public PartyChangedOrderEvent(Dictionary<byte, object> parameters) : base(parameters)
        {
            ConsoleManager.WriteLineForNetworkHandler(GetType().Name, parameters);

            try
            {
                if (parameters.ContainsKey(1)) UserGuid = parameters[1].ObjectToGuid();

                if (parameters.ContainsKey(2)) Username = string.IsNullOrEmpty(parameters[2].ToString()) ? string.Empty : parameters[2].ToString();
            }
            catch (Exception e)
            {
                ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
            }
        }
    }
}