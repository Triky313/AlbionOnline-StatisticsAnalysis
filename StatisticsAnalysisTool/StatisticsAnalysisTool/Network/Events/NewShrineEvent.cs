using Albion.Network;
using log4net;
using StatisticsAnalysisTool.Common;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace StatisticsAnalysisTool.Network.Handler
{
    public class NewShrineEvent : BaseEvent
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);
        
        public NewShrineEvent(Dictionary<byte, object> parameters) : base(parameters)
        {
            ConsoleManager.WriteLineForNetworkHandler(GetType().Name, parameters);

            try
            {
                if (parameters.ContainsKey(0) && int.TryParse(parameters[0].ToString(), out var id))
                {
                    Id = id;
                }

                if (parameters.ContainsKey(3))
                {
                    UniqueName = string.IsNullOrEmpty(parameters[3].ToString()) ? string.Empty : parameters[3].ToString();
                }

                if (parameters.ContainsKey(4))
                {
                    ObjectName = string.IsNullOrEmpty(parameters[4].ToString()) ? string.Empty : parameters[4].ToString();
                }
            }
            catch (Exception e)
            {
                Log.Debug(nameof(NewShrineEvent), e);
            }
        }

        public int Id { get; set; }
        public string UniqueName { get; set; }
        public string ObjectName { get; set; }
    }
}