using log4net;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Models.NetworkModel;
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
                    !parameters.ContainsKey(10) || parameters[10] == null ||
                    !parameters[3].GetType().IsArray ||
                    typeof(long[]).Name != parameters[3].GetType().Name)
                {
                    return;
                }

                var guid = parameters[0].ObjectToGuid();
                var mailIdArray = ((long[])parameters[3]).ToArray();

                if (mailIdArray is not {Length: > 0})
                {
                    return;
                }

                var subjectArray = ((string[])parameters[6]).ToArray();
                var mailTypeTextArray = ((string[])parameters[10]).ToArray();
                var timeStampArray = ((long[])parameters[11]).ToArray();

                var length = Utilities.GetHighestLength(mailIdArray, subjectArray, mailTypeTextArray);

                for (var i = 0; i < length; i++)
                {
                    var mailId = mailIdArray[i];
                    var subject = subjectArray[i];
                    var mailTypeText = mailTypeTextArray[i];
                    var timeStamp = timeStampArray[i];

                    MailInfos.Add(new MailInfoObject(guid, mailId, subject, mailTypeText, timeStamp));
                }
            }
            catch (Exception e)
            {
                Log.Error(nameof(GetMailInfosResponse), e);
            }
        }
    }
}