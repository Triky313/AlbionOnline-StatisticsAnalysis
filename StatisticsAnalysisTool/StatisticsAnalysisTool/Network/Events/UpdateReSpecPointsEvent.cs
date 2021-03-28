using Albion.Network;
using log4net;
using PcapDotNet.Base;
using StatisticsAnalysisTool.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

namespace StatisticsAnalysisTool.Network.Events
{
    public class UpdateReSpecPointsEvent : BaseEvent
    {
        public FixPoint? CurrentReSpecPoints { get; }

        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public UpdateReSpecPointsEvent(Dictionary<byte, object> parameters) : base(parameters)
        {
            try
            {
                if (parameters.ContainsKey(0) && parameters[0] != null)
                {
                    var parameterType = parameters[0].GetType();

                    if(parameterType.Name == "Int32[]") 
                    {
                        var reSpecPointsArray = ((int[])parameters[0]).ToDictionary();

                        if (!reSpecPointsArray.IsNullOrEmpty() && reSpecPointsArray.ContainsKey(1))
                        {
                            CurrentReSpecPoints = FixPoint.FromInternalValue(reSpecPointsArray[1].ObjectToLong() ?? 0);
                        }
                    } 
                    else if(parameterType.Name == "Int64[]") 
                    {
                        var reSpecPointsArray = ((long[])parameters[0]).ToDictionary();

                        if (!reSpecPointsArray.IsNullOrEmpty() && reSpecPointsArray.ContainsKey(1))
                        {
                            CurrentReSpecPoints = FixPoint.FromInternalValue(reSpecPointsArray[1].ObjectToLong() ?? 0);
                        }
                    }
                }
            }
            catch (ArgumentNullException e)
            {
                Log.Error(nameof(UpdateReSpecPointsEvent), e);
                Debug.Print(e.Message);
            }
            catch (InvalidCastException e)
            {
                Log.Error(nameof(UpdateReSpecPointsEvent), e);
                Debug.Print(e.Message);
            }
        }
    }
}