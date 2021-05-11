using Albion.Network;
using log4net;
using StatisticsAnalysisTool.Common;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace StatisticsAnalysisTool.Network.Operations.Responses
{
    public class PartyMakeLeaderResponse : BaseOperation
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public string Username;

        public PartyMakeLeaderResponse(Dictionary<byte, object> parameters) : base(parameters)
        {
            ConsoleManager.WriteLineForNetworkHandler(GetType().Name, parameters);

            try
            {
                if (parameters.ContainsKey(0)) Username = string.IsNullOrEmpty(parameters[0].ToString()) ? string.Empty : parameters[0].ToString();
            }
            catch (Exception e)
            {
                Log.Debug(nameof(PartyMakeLeaderResponse), e);
            }
        }
    }
}