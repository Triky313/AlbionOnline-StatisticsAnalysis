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
            catch(InvalidCastException e)
            {
                Log.Error(nameof(UpdateReSpecPointsEvent), e);
                Debug.Print(e.Message);
            }
        }

        public double CurrentReSpecPoints { get; }
    }
}