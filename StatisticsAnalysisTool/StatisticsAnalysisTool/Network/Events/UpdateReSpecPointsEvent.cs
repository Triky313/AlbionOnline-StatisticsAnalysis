using Albion.Network;
using log4net;
using PcapDotNet.Base;
using StatisticsAnalysisTool.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace StatisticsAnalysisTool.Network.Events
{
    public class UpdateReSpecPointsEvent : BaseEvent
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public UpdateReSpecPointsEvent(Dictionary<byte, object> parameters) : base(parameters)
        {
            try
            {
                if (parameters.ContainsKey(0))
                {
                    // Todo: Exception thrown: 'System.InvalidCastException' in StatisticsAnalysisTool.exe
                    // An exception of type 'System.InvalidCastException' occurred in StatisticsAnalysisTool.exe but was not handled in user code
                    // Das Objekt des Typs "System.Int32[]" kann nicht in Typ "System.Int64[]" umgewandelt werden.
                    var reSpecPointsArray = ((long[])parameters[0]).ToDictionary();

                    if (!reSpecPointsArray.IsNullOrEmpty() && reSpecPointsArray.ContainsKey(1) && long.TryParse(reSpecPointsArray[1].ToString(), out long currentReSpecPoints))
                    {
                        CurrentReSpecPoints = currentReSpecPoints / 10000d;
                    }
                }
            }
            catch(ArgumentNullException e)
            {
                Log.Error(nameof(UpdateReSpecPointsEvent), e);
                Debug.Print(e.Message);
            }
        }

        public double CurrentReSpecPoints { get; }
    }
}