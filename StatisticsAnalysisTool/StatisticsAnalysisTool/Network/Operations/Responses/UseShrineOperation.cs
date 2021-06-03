using Albion.Network;
using log4net;
using StatisticsAnalysisTool.Common;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace StatisticsAnalysisTool.Network.Handler
{
    public class UseShrineOperation : BaseOperation
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public UseShrineOperation(Dictionary<byte, object> parameters) : base(parameters)
        {
            ConsoleManager.WriteLineForNetworkHandler(GetType().Name, parameters);

            try
            {
                //Debug.Print("----- UseShrine (Operation) -----");

                //foreach (var parameter in parameters)
                //{
                //    Debug.Print($"{parameter}");
                //}
            }
            catch (Exception e)
            {
                ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod().DeclaringType, e);
                Log.Error(MethodBase.GetCurrentMethod().DeclaringType, e);
            }
        }
    }
}