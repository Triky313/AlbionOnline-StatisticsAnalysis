using log4net;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Models.NetworkModel;
using StatisticsAnalysisTool.Network.Manager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace StatisticsAnalysisTool.Network.Operations.Responses
{
    public class GetMailInfosResponse
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);

        public List<MailInfoObject> MailInfos = new();

        public GetMailInfosResponse(Dictionary<byte, object> parameters)
        {
            ConsoleManager.WriteLineForNetworkHandler(GetType().Name, parameters);

            MailInfos.Clear();

            try
            {
                if (!parameters.ContainsKey(0) || parameters[0] == null || 
                    !parameters.ContainsKey(3) || parameters[3] == null || 
                    !parameters.ContainsKey(6) || parameters[6] == null ||
                    !parameters.ContainsKey(10) || parameters[10] == null)
                {
                    return;
                }

                var guid = parameters[0].ObjectToGuid();
                var mailIdArray = ((long[])parameters[3]).ToArray();
                var clusterIndexArray = ((string[])parameters[6]).ToArray();
                var mailTypeArray = ((string[])parameters[10]).ToArray();

                var length = Utilities.GetHighestLength(mailIdArray, clusterIndexArray, mailTypeArray);

                for (var i = 0; i < length; i++)
                {
                    var mailId = mailIdArray[i];
                    var clusterIndex = clusterIndexArray[i];
                    var mailType = mailTypeArray[i];

                    MailInfos.Add(new MailInfoObject(guid, mailId, clusterIndex, MailController.ConvertToMailType(mailType)));
                }
            }
            catch (Exception e)
            {
                Log.Error(nameof(GetMailInfosResponse), e);
            }
        }
    }
}