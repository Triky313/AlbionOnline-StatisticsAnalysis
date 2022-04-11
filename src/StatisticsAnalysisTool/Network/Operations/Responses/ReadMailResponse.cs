using log4net;
using StatisticsAnalysisTool.Common;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace StatisticsAnalysisTool.Network.Operations.Responses
{
    public class ReadMailResponse
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);

        public long MailId;
        public string Content;

        public ReadMailResponse(Dictionary<byte, object> parameters)
        {
            ConsoleManager.WriteLineForNetworkHandler(GetType().Name, parameters);

            try
            {
                if (parameters.ContainsKey(0))
                {
                    MailId = parameters[0].ObjectToLong() ?? -1;
                }

                if (parameters.ContainsKey(1))
                {
                    Content = string.IsNullOrEmpty(parameters[1].ToString()) ? string.Empty : parameters[1].ToString();
                }
            }
            catch (Exception e)
            {
                Log.Error(nameof(ReadMailResponse), e);
            }
        }
    }
}