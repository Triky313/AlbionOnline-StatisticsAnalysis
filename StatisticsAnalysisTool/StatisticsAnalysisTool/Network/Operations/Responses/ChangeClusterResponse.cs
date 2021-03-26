using System;
using System.Collections.Generic;
using System.Reflection;
using Albion.Network;
using log4net;

namespace StatisticsAnalysisTool.Network.Operations.Responses
{
    public class ChangeClusterResponse : BaseOperation
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public string ClusterMap;

        public string ClusterOwner;

        public string Index;

        public ChangeClusterResponse(Dictionary<byte, object> parameters) : base(parameters)
        {
            try
            {
                if (parameters.ContainsKey(0)) Index = string.IsNullOrEmpty(parameters[0].ToString()) ? string.Empty : parameters[0].ToString();

                if (parameters.ContainsKey(253))
                    ClusterOwner = string.IsNullOrEmpty(parameters[253].ToString()) ? string.Empty : parameters[253].ToString();

                if (parameters.ContainsKey(255))
                    ClusterMap = string.IsNullOrEmpty(parameters[255].ToString()) ? string.Empty : parameters[255].ToString();
            }
            catch (Exception e)
            {
                Log.Debug(nameof(ChangeClusterResponse), e);
            }
        }
    }
}